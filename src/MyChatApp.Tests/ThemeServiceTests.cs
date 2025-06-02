using Microsoft.JSInterop;
using NSubstitute;
using MyChatApp.Web.Services;

namespace MyChatApp.Tests;

public class ThemeServiceTests
{
    [Test]
    public void ThemeService_InitiallyNotDarkMode()
    {
        // Arrange
        var mockJSRuntime = Substitute.For<IJSRuntime>();
        var themeService = new ThemeService(mockJSRuntime);

        // Act & Assert
        Assert.That(themeService.IsDarkMode, Is.False);
    }

    [Test]
    public async Task ToggleThemeAsync_ChangesDarkModeState()
    {
        // Arrange
        var mockJSRuntime = Substitute.For<IJSRuntime>();
        var themeService = new ThemeService(mockJSRuntime);
        var initialState = themeService.IsDarkMode;

        // Act
        await themeService.ToggleThemeAsync();

        // Assert
        Assert.That(themeService.IsDarkMode, Is.Not.EqualTo(initialState));
    }

    [Test]
    public async Task InitializeAsync_LoadsSavedTheme()
    {
        // Arrange
        var mockJSRuntime = Substitute.For<IJSRuntime>();
        mockJSRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(ValueTask.FromResult<string?>("dark"));
        
        var themeService = new ThemeService(mockJSRuntime);

        // Act
        await themeService.InitializeAsync();

        // Assert
        Assert.That(themeService.IsDarkMode, Is.True);
    }

    [Test]
    public async Task OnThemeChanged_EventFired_WhenThemeToggled()
    {
        // Arrange
        var mockJSRuntime = Substitute.For<IJSRuntime>();
        var themeService = new ThemeService(mockJSRuntime);
        var eventFired = false;
        
        themeService.OnThemeChanged += () => eventFired = true;

        // Act
        await themeService.ToggleThemeAsync();

        // Assert
        Assert.That(eventFired, Is.True);
    }

    [Test]
    public async Task InitializeAsync_HandlesJSExceptions_Gracefully()
    {
        // Arrange
        var mockJSRuntime = Substitute.For<IJSRuntime>();
        mockJSRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns<ValueTask<string?>>(x => throw new InvalidOperationException("JavaScript error"));
        
        var themeService = new ThemeService(mockJSRuntime);

        // Act & Assert - Should not throw
        await themeService.InitializeAsync();
        Assert.That(themeService.IsDarkMode, Is.False); // Should fallback to light mode
    }
}