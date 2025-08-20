using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Abstracts
{
    /// <summary>
    /// Abstract base class for theme managers
    /// </summary>
    public abstract class BaseThemeManager : IThemeManager
    {
        protected readonly ILogger<BaseThemeManager> _logger;
        protected readonly List<Theme> _themes;

        protected BaseThemeManager(ILogger<BaseThemeManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _themes = new List<Theme>();
        }

        /// <summary>
        /// Get all available themes
        /// </summary>
        public virtual async Task<List<Theme>> GetAvailableThemesAsync()
        {
            try
            {
                _logger.LogDebug("Getting available themes");
                return await Task.FromResult(new List<Theme>(_themes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available themes");
                throw;
            }
        }

        /// <summary>
        /// Get the currently active theme
        /// </summary>
        public abstract Task<Theme> GetActiveThemeAsync();

        /// <summary>
        /// Set the active theme by ID
        /// </summary>
        public abstract Task SetActiveThemeAsync(string themeId);

        /// <summary>
        /// Get a theme by its ID
        /// </summary>
        public abstract Task<Theme> GetThemeByIdAsync(string themeId);

        /// <summary>
        /// Generate CSS for a specific theme
        /// </summary>
        public abstract Task<string> GenerateThemeCssAsync(Theme theme);

        /// <summary>
        /// Register a theme with the manager
        /// </summary>
        protected virtual void RegisterTheme(Theme theme)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            if (string.IsNullOrEmpty(theme.Id))
                throw new ArgumentException("Theme ID cannot be null or empty", nameof(theme));

            _themes.Add(theme);
            _logger.LogDebug("Registered theme: {ThemeId}", theme.Id);
        }
    }
}