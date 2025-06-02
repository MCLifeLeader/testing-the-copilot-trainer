using Microsoft.AspNetCore.Components.Forms;

namespace MyChatApp.Web.Services
{
    /// <summary>
    /// Service for handling file uploads and storage
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Uploads an avatar image file and returns the URL
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <param name="userId">The user ID for file naming</param>
        /// <returns>The URL to access the uploaded file</returns>
        Task<string> UploadAvatarAsync(IBrowserFile file, string userId);

        /// <summary>
        /// Deletes an avatar file
        /// </summary>
        /// <param name="avatarUrl">The URL of the avatar to delete</param>
        Task DeleteAvatarAsync(string avatarUrl);

        /// <summary>
        /// Validates if the file is a valid image
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if valid image, false otherwise</returns>
        bool IsValidImage(IBrowserFile file);
    }

    /// <summary>
    /// Local filesystem implementation of file upload service
    /// </summary>
    public class LocalFileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalFileUploadService> _logger;
        private const string AvatarsFolder = "uploads/avatars";
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public LocalFileUploadService(IWebHostEnvironment environment, ILogger<LocalFileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;

            // Ensure the avatars directory exists
            var avatarsPath = Path.Combine(_environment.WebRootPath, AvatarsFolder);
            if (!Directory.Exists(avatarsPath))
            {
                Directory.CreateDirectory(avatarsPath);
            }
        }

        public bool IsValidImage(IBrowserFile file)
        {
            if (file is null || file.Size == 0)
                return false;

            // Check file size
            if (file.Size > MaxFileSize)
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Check content type
            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        public async Task<string> UploadAvatarAsync(IBrowserFile file, string userId)
        {
            if (!IsValidImage(file))
                throw new ArgumentException("Invalid image file");

            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_environment.WebRootPath, AvatarsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.OpenReadStream(MaxFileSize).CopyToAsync(stream);
            }

            var url = $"/{AvatarsFolder}/{fileName}";
            _logger.LogInformation("Avatar uploaded for user {UserId}: {Url}", userId, url);

            return url;
        }

        public Task DeleteAvatarAsync(string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
                return Task.CompletedTask;

            try
            {
                // Remove leading slash if present
                var relativePath = avatarUrl.StartsWith('/') ? avatarUrl[1..] : avatarUrl;
                var filePath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Avatar deleted: {AvatarUrl}", avatarUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting avatar file: {AvatarUrl}", avatarUrl);
            }

            return Task.CompletedTask;
        }
    }
}