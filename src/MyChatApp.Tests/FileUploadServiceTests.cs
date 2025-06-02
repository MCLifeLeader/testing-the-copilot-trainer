using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MyChatApp.Web.Services;
using NSubstitute;

namespace MyChatApp.Tests;

[TestFixture]
public class FileUploadServiceTests
{
    private LocalFileUploadService _service;
    private IWebHostEnvironment _environment;
    private ILogger<LocalFileUploadService> _logger;
    private string _tempDirectory;

    [SetUp]
    public void Setup()
    {
        _environment = Substitute.For<IWebHostEnvironment>();
        _logger = Substitute.For<ILogger<LocalFileUploadService>>();

        // Create a temp directory for testing
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        Directory.CreateDirectory(Path.Combine(_tempDirectory, "uploads", "avatars"));

        _environment.WebRootPath.Returns(_tempDirectory);
        _service = new LocalFileUploadService(_environment, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up temp directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Test]
    public void IsValidImage_ReturnsTrue_ForValidJpegFile()
    {
        // Arrange
        var file = CreateMockImageFile("test.jpg", "image/jpeg", 1024);

        // Act
        var result = _service.IsValidImage(file);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidImage_ReturnsTrue_ForValidPngFile()
    {
        // Arrange
        var file = CreateMockImageFile("test.png", "image/png", 2048);

        // Act
        var result = _service.IsValidImage(file);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidImage_ReturnsFalse_ForOversizedFile()
    {
        // Arrange
        var file = CreateMockImageFile("test.jpg", "image/jpeg", 6 * 1024 * 1024); // 6MB

        // Act
        var result = _service.IsValidImage(file);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidImage_ReturnsFalse_ForInvalidMimeType()
    {
        // Arrange
        var file = CreateMockImageFile("test.txt", "text/plain", 1024);

        // Act
        var result = _service.IsValidImage(file);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidImage_ReturnsFalse_ForInvalidExtension()
    {
        // Arrange
        var file = CreateMockImageFile("test.exe", "image/jpeg", 1024);

        // Act
        var result = _service.IsValidImage(file);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidImage_ReturnsFalse_ForNullFile()
    {
        // Act
        var result = _service.IsValidImage(null!);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidImage_ReturnsFalse_ForEmptyFile()
    {
        // Arrange
        var file = CreateMockImageFile("test.jpg", "image/jpeg", 0);

        // Act
        var result = _service.IsValidImage(file);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UploadAvatarAsync_CreatesFile_WhenValidImage()
    {
        // Arrange
        var file = CreateMockImageFile("test.jpg", "image/jpeg", 1024);
        var userId = "test-user-id";

        // Act
        var result = await _service.UploadAvatarAsync(file, userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.StartWith("/uploads/avatars/"));
        Assert.That(result, Does.Contain(userId));
        Assert.That(result, Does.EndWith(".jpg"));

        // Verify file was created
        var fileName = Path.GetFileName(result);
        var filePath = Path.Combine(_tempDirectory, "uploads", "avatars", fileName);
        Assert.That(File.Exists(filePath), Is.True);
    }

    [Test]
    public void UploadAvatarAsync_ThrowsException_WhenInvalidImage()
    {
        // Arrange
        var file = CreateMockImageFile("test.txt", "text/plain", 1024);
        var userId = "test-user-id";

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => 
            await _service.UploadAvatarAsync(file, userId));
    }

    [Test]
    public async Task DeleteAvatarAsync_DeletesFile_WhenFileExists()
    {
        // Arrange
        var avatarUrl = "/uploads/avatars/test-avatar.jpg";
        var filePath = Path.Combine(_tempDirectory, "uploads", "avatars", "test-avatar.jpg");
        
        // Create the file first
        await File.WriteAllTextAsync(filePath, "test content");
        Assert.That(File.Exists(filePath), Is.True);

        // Act
        await _service.DeleteAvatarAsync(avatarUrl);

        // Assert
        Assert.That(File.Exists(filePath), Is.False);
    }

    [Test]
    public Task DeleteAvatarAsync_DoesNotThrow_WhenFileDoesNotExist()
    {
        // Arrange
        var avatarUrl = "/uploads/avatars/nonexistent.jpg";

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.DeleteAvatarAsync(avatarUrl));
        return Task.CompletedTask;
    }

    [Test]
    public Task DeleteAvatarAsync_DoesNotThrow_WhenUrlIsNull()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.DeleteAvatarAsync(null!));
        return Task.CompletedTask;
    }

    [Test]
    public Task DeleteAvatarAsync_DoesNotThrow_WhenUrlIsEmpty()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _service.DeleteAvatarAsync(string.Empty));
        return Task.CompletedTask;
    }

    private static IBrowserFile CreateMockImageFile(string fileName, string contentType, long size)
    {
        var file = Substitute.For<IBrowserFile>();
        file.Name.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Size.Returns(size);

        // Create a memory stream with some dummy content
        var content = new byte[Math.Min(size, 1024)]; // Limit to 1KB for testing
        for (int i = 0; i < content.Length; i++)
        {
            content[i] = (byte)(i % 256);
        }
        var stream = new MemoryStream(content);
        
        file.OpenReadStream(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(stream);
        file.OpenReadStream(Arg.Any<long>()).Returns(stream);

        return file;
    }
}