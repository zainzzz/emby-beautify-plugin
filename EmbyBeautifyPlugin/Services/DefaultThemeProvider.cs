using EmbyBeautifyPlugin.Models;
using System.Collections.Generic;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// Provides built-in default themes for the beautify plugin
    /// </summary>
    public static class DefaultThemeProvider
    {
        /// <summary>
        /// Get all built-in themes
        /// </summary>
        /// <returns>List of built-in themes</returns>
        public static List<Theme> GetBuiltInThemes()
        {
            return new List<Theme>
            {
                GetDefaultLightTheme(),
                GetDefaultDarkTheme(),
                GetModernLightTheme(),
                GetModernDarkTheme()
            };
        }

        /// <summary>
        /// Get the default light theme
        /// </summary>
        /// <returns>Default light theme</returns>
        public static Theme GetDefaultLightTheme()
        {
            return new Theme
            {
                Id = "default-light",
                Name = "Default Light",
                Description = "Clean and bright default light theme",
                Version = "1.0.0",
                Author = "Emby Beautify Plugin",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Secondary = "#5a9fd4",
                    Background = "#ffffff",
                    Surface = "#f8f9fa",
                    Text = "#212529",
                    Accent = "#0056b3"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
                    FontSize = "14px",
                    HeadingWeight = "600",
                    BodyWeight = "400",
                    LineHeight = "1.5"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "4px",
                    SpacingUnit = "1rem",
                    BoxShadow = "0 2px 4px rgba(0,0,0,0.1)",
                    MaxWidth = "1200px"
                }
            };
        }

        /// <summary>
        /// Get the default dark theme
        /// </summary>
        /// <returns>Default dark theme</returns>
        public static Theme GetDefaultDarkTheme()
        {
            return new Theme
            {
                Id = "default-dark",
                Name = "Default Dark",
                Description = "Elegant dark theme for comfortable viewing",
                Version = "1.0.0",
                Author = "Emby Beautify Plugin",
                Colors = new ThemeColors
                {
                    Primary = "#4dabf7",
                    Secondary = "#74c0fc",
                    Background = "#1a1a1a",
                    Surface = "#2d2d2d",
                    Text = "#ffffff",
                    Accent = "#339af0"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
                    FontSize = "14px",
                    HeadingWeight = "600",
                    BodyWeight = "400",
                    LineHeight = "1.5"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "4px",
                    SpacingUnit = "1rem",
                    BoxShadow = "0 2px 8px rgba(0,0,0,0.3)",
                    MaxWidth = "1200px"
                }
            };
        }

        /// <summary>
        /// Get the modern light theme
        /// </summary>
        /// <returns>Modern light theme</returns>
        public static Theme GetModernLightTheme()
        {
            return new Theme
            {
                Id = "modern-light",
                Name = "Modern Light",
                Description = "Contemporary light theme with modern design elements",
                Version = "1.0.0",
                Author = "Emby Beautify Plugin",
                Colors = new ThemeColors
                {
                    Primary = "#6366f1",
                    Secondary = "#8b5cf6",
                    Background = "#ffffff",
                    Surface = "#f1f5f9",
                    Text = "#0f172a",
                    Accent = "#f59e0b"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
                    FontSize = "15px",
                    HeadingWeight = "700",
                    BodyWeight = "400",
                    LineHeight = "1.6"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "8px",
                    SpacingUnit = "1.25rem",
                    BoxShadow = "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)",
                    MaxWidth = "1280px"
                },
                CustomProperties = new Dictionary<string, string>
                {
                    { "gradient-primary", "linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)" },
                    { "transition-duration", "0.2s" },
                    { "hover-transform", "translateY(-1px)" }
                }
            };
        }

        /// <summary>
        /// Get the modern dark theme
        /// </summary>
        /// <returns>Modern dark theme</returns>
        public static Theme GetModernDarkTheme()
        {
            return new Theme
            {
                Id = "modern-dark",
                Name = "Modern Dark",
                Description = "Contemporary dark theme with sleek modern aesthetics",
                Version = "1.0.0",
                Author = "Emby Beautify Plugin",
                Colors = new ThemeColors
                {
                    Primary = "#818cf8",
                    Secondary = "#a78bfa",
                    Background = "#0f172a",
                    Surface = "#1e293b",
                    Text = "#f1f5f9",
                    Accent = "#fbbf24"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
                    FontSize = "15px",
                    HeadingWeight = "700",
                    BodyWeight = "400",
                    LineHeight = "1.6"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "8px",
                    SpacingUnit = "1.25rem",
                    BoxShadow = "0 10px 15px -3px rgba(0, 0, 0, 0.3), 0 4px 6px -2px rgba(0, 0, 0, 0.2)",
                    MaxWidth = "1280px"
                },
                CustomProperties = new Dictionary<string, string>
                {
                    { "gradient-primary", "linear-gradient(135deg, #818cf8 0%, #a78bfa 100%)" },
                    { "transition-duration", "0.2s" },
                    { "hover-transform", "translateY(-1px)" },
                    { "glow-effect", "0 0 20px rgba(129, 140, 248, 0.3)" }
                }
            };
        }

        /// <summary>
        /// Get a theme by ID
        /// </summary>
        /// <param name="themeId">Theme ID</param>
        /// <returns>Theme if found, null otherwise</returns>
        public static Theme GetThemeById(string themeId)
        {
            return themeId switch
            {
                "default-light" => GetDefaultLightTheme(),
                "default-dark" => GetDefaultDarkTheme(),
                "modern-light" => GetModernLightTheme(),
                "modern-dark" => GetModernDarkTheme(),
                _ => null
            };
        }

        /// <summary>
        /// Check if a theme ID is a built-in theme
        /// </summary>
        /// <param name="themeId">Theme ID to check</param>
        /// <returns>True if built-in theme</returns>
        public static bool IsBuiltInTheme(string themeId)
        {
            return themeId switch
            {
                "default-light" or "default-dark" or "modern-light" or "modern-dark" => true,
                _ => false
            };
        }
    }
}