using Microsoft.JSInterop;

namespace MyChatApp.Web.Services;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isDarkMode = false;

    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IsDarkMode => _isDarkMode;

    public async Task InitializeAsync()
    {
        try
        {
            var savedTheme = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "theme");
            _isDarkMode = savedTheme == "dark";
            await ApplyThemeAsync();
        }
        catch
        {
            // Fallback to light mode if localStorage is not available
            _isDarkMode = false;
            await ApplyThemeAsync();
        }
    }

    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await SaveThemeAsync();
        await ApplyThemeAsync();
        OnThemeChanged?.Invoke();
    }

    private async Task SaveThemeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", _isDarkMode ? "dark" : "light");
        }
        catch
        {
            // Ignore localStorage errors
        }
    }

    private async Task ApplyThemeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", _isDarkMode ? "dark" : "light");
        }
        catch
        {
            // Ignore JS errors during pre-rendering
        }
    }
}