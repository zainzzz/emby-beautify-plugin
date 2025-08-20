using EmbyBeautifyPlugin.Abstracts;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// Concrete implementation of theme manager
    /// </summary>
    public class ThemeManager : BaseThemeManager
    {
        private readonly IConfigurationManager _configurationManager;
        private Theme _activeTheme;
        private readonly string _themesDirectory;

        public ThemeManager(ILogger<BaseThemeManager> logger, IConfigurationManager configurationManager) 
            : base(logger)
        {
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            _themesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes");
            
            // Ensure themes directory exists
            if (!Directory.Exists(_themesDirectory))
            {
                Directory.CreateDirectory(_themesDirectory);
            }
        }

        /// <summary>
        /// Initialize the theme manager and discover themes
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing theme manager");
                
                // Clear existing themes
                _themes.Clear();
                
                // Register built-in themes
                await RegisterBuiltInThemesAsync();
                
                // Discover themes from directory
                await DiscoverThemesFromDirectoryAsync();
                
                // Load active theme from configuration
                await LoadActiveThemeAsync();
                
                _logger.LogInformation("Theme manager initialized with {ThemeCount} themes", _themes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing theme manager");
                throw;
            }
        }

        /// <summary>
        /// Get the currently active theme
        /// </summary>
        public override async Task<Theme> GetActiveThemeAsync()
        {
            try
            {
                if (_activeTheme == null)
                {
                    await LoadActiveThemeAsync();
                }
                
                return _activeTheme ?? await GetDefaultThemeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active theme");
                return await GetDefaultThemeAsync();
            }
        }

        /// <summary>
        /// Set the active theme by ID
        /// </summary>
        public override async Task SetActiveThemeAsync(string themeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(themeId))
                    throw new ArgumentException("Theme ID cannot be null or empty", nameof(themeId));

                var theme = _themes.FirstOrDefault(t => t.Id == themeId);
                if (theme == null)
                    throw new ArgumentException($"Theme with ID '{themeId}' not found", nameof(themeId));

                _activeTheme = theme;
                
                // Update configuration
                var config = await _configurationManager.LoadConfigurationAsync();
                config.ActiveThemeId = themeId;
                await _configurationManager.SaveConfigurationAsync(config);
                
                _logger.LogInformation("Active theme set to: {ThemeId}", themeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting active theme to {ThemeId}", themeId);
                throw;
            }
        }

        /// <summary>
        /// Get a theme by its ID
        /// </summary>
        public override async Task<Theme> GetThemeByIdAsync(string themeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(themeId))
                    return null;

                var themes = await GetAvailableThemesAsync();
                return themes.FirstOrDefault(t => t.Id == themeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting theme by ID: {ThemeId}", themeId);
                return null;
            }
        }

        /// <summary>
        /// Generate CSS for a specific theme
        /// </summary>
        public override async Task<string> GenerateThemeCssAsync(Theme theme)
        {
            try
            {
                if (theme == null)
                    throw new ArgumentNullException(nameof(theme));

                var css = ThemeCssGenerator.GenerateThemeCss(theme);
                
                _logger.LogDebug("Generated CSS for theme: {ThemeId}", theme.Id);
                return await Task.FromResult(css);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSS for theme: {ThemeId}", theme?.Id);
                throw;
            }
        }

        /// <summary>
        /// Register a theme and validate it
        /// </summary>
        public async Task<bool> RegisterThemeAsync(Theme theme)
        {
            try
            {
                if (theme == null)
                    throw new ArgumentNullException(nameof(theme));

                // Validate theme
                var validationErrors = theme.Validate();
                if (validationErrors.Count > 0)
                {
                    _logger.LogWarning("Theme validation failed for {ThemeId}: {Errors}", 
                        theme.Id, string.Join(", ", validationErrors));
                    return false;
                }

                // Check if theme already exists
                var existingTheme = _themes.FirstOrDefault(t => t.Id == theme.Id);
                if (existingTheme != null)
                {
                    _logger.LogInformation("Updating existing theme: {ThemeId}", theme.Id);
                    _themes.Remove(existingTheme);
                }

                RegisterTheme(theme);
                _logger.LogInformation("Successfully registered theme: {ThemeId}", theme.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering theme: {ThemeId}", theme?.Id);
                return false;
            }
        }

        /// <summary>
        /// Unregister a theme by ID
        /// </summary>
        public async Task<bool> UnregisterThemeAsync(string themeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(themeId))
                    throw new ArgumentException("Theme ID cannot be null or empty", nameof(themeId));

                var theme = _themes.FirstOrDefault(t => t.Id == themeId);
                if (theme == null)
                {
                    _logger.LogWarning("Theme not found for unregistration: {ThemeId}", themeId);
                    return false;
                }

                _themes.Remove(theme);
                
                // If this was the active theme, switch to default
                if (_activeTheme?.Id == themeId)
                {
                    _activeTheme = await GetDefaultThemeAsync();
                    var config = await _configurationManager.LoadConfigurationAsync();
                    config.ActiveThemeId = _activeTheme.Id;
                    await _configurationManager.SaveConfigurationAsync(config);
                }

                _logger.LogInformation("Successfully unregistered theme: {ThemeId}", themeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering theme: {ThemeId}", themeId);
                return false;
            }
        }

        /// <summary>
        /// Register built-in themes
        /// </summary>
        private async Task RegisterBuiltInThemesAsync()
        {
            try
            {
                var builtInThemes = DefaultThemeProvider.GetBuiltInThemes();
                
                foreach (var theme in builtInThemes)
                {
                    await RegisterThemeAsync(theme);
                }
                
                _logger.LogInformation("Registered {ThemeCount} built-in themes", builtInThemes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering built-in themes");
            }
        }

        /// <summary>
        /// Discover themes from the themes directory
        /// </summary>
        private async Task DiscoverThemesFromDirectoryAsync()
        {
            try
            {
                if (!Directory.Exists(_themesDirectory))
                    return;

                var themeFiles = Directory.GetFiles(_themesDirectory, "*.json", SearchOption.AllDirectories);
                
                foreach (var themeFile in themeFiles)
                {
                    try
                    {
                        var theme = await ThemeSerializer.LoadFromFileAsync(themeFile);
                        await RegisterThemeAsync(theme);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load theme from file: {ThemeFile}", themeFile);
                    }
                }
                
                _logger.LogDebug("Discovered {ThemeCount} themes from directory", themeFiles.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering themes from directory");
            }
        }

        /// <summary>
        /// Load the active theme from configuration
        /// </summary>
        private async Task LoadActiveThemeAsync()
        {
            try
            {
                var config = await _configurationManager.LoadConfigurationAsync();
                if (!string.IsNullOrEmpty(config.ActiveThemeId))
                {
                    var theme = _themes.FirstOrDefault(t => t.Id == config.ActiveThemeId);
                    if (theme != null)
                    {
                        _activeTheme = theme;
                        _logger.LogDebug("Loaded active theme: {ThemeId}", config.ActiveThemeId);
                        return;
                    }
                }
                
                // Fallback to default theme
                _activeTheme = await GetDefaultThemeAsync();
                _logger.LogDebug("Using default theme as active theme");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active theme");
                _activeTheme = await GetDefaultThemeAsync();
            }
        }

        /// <summary>
        /// Get the default theme
        /// </summary>
        private async Task<Theme> GetDefaultThemeAsync()
        {
            // Try to find default-light theme first
            var defaultTheme = _themes.FirstOrDefault(t => t.Id == "default-light") ?? 
                              _themes.FirstOrDefault(t => t.Id == "default") ?? 
                              _themes.FirstOrDefault();
            
            if (defaultTheme == null)
            {
                // Register the default light theme if no themes exist
                defaultTheme = DefaultThemeProvider.GetDefaultLightTheme();
                await RegisterThemeAsync(defaultTheme);
            }
            
            return defaultTheme;
        }
    }
}