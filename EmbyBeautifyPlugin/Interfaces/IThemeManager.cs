using EmbyBeautifyPlugin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Interfaces
{
    /// <summary>
    /// Interface for managing themes in the beautify plugin
    /// </summary>
    public interface IThemeManager
    {
        /// <summary>
        /// Get all available themes
        /// </summary>
        /// <returns>List of available themes</returns>
        Task<List<Theme>> GetAvailableThemesAsync();

        /// <summary>
        /// Get the currently active theme
        /// </summary>
        /// <returns>Currently active theme</returns>
        Task<Theme> GetActiveThemeAsync();

        /// <summary>
        /// Set the active theme by ID
        /// </summary>
        /// <param name="themeId">ID of the theme to activate</param>
        /// <returns>Task representing the operation</returns>
        Task SetActiveThemeAsync(string themeId);

        /// <summary>
        /// Get a theme by its ID
        /// </summary>
        /// <param name="themeId">ID of the theme to retrieve</param>
        /// <returns>Theme with the specified ID, or null if not found</returns>
        Task<Theme> GetThemeByIdAsync(string themeId);

        /// <summary>
        /// Generate CSS for a specific theme
        /// </summary>
        /// <param name="theme">Theme to generate CSS for</param>
        /// <returns>Generated CSS string</returns>
        Task<string> GenerateThemeCssAsync(Theme theme);
    }
}