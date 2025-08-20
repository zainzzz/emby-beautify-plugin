using System;
using System.Collections.Generic;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    public class ThemeCssGeneratorTests
    {
        private Theme CreateTestTheme()
        {
            return new Theme
            {
                Id = "test-theme",
                Name = "Test Theme",
                Description = "A test theme",
                Version = "1.0.0",
                Author = "Test Author",
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
                    FontFamily = "Arial, sans-serif",
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
                },
                CustomProperties = new Dictionary<string, string>
                {
                    { "custom-color", "#ff5733" },
                    { "transition-duration", "0.2s" }
                }
            };
        }

        [Fact]
        public void GenerateThemeCss_WithValidTheme_ReturnsValidCss()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.NotNull(css);
            Assert.NotEmpty(css);
        }

        [Fact]
        public void GenerateThemeCss_WithNullTheme_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ThemeCssGenerator.GenerateThemeCss(null));
        }

        [Fact]
        public void GenerateThemeCss_IncludesThemeHeader()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains("/* Theme: Test Theme v1.0.0 */", css);
            Assert.Contains("/* Author: Test Author */", css);
            Assert.Contains("/* Description: A test theme */", css);
        }

        [Fact]
        public void GenerateThemeCss_IncludesRootVariables()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains(":root {", css);
            Assert.Contains("--primary-color: #007acc;", css);
            Assert.Contains("--background-color: #ffffff;", css);
            Assert.Contains("--text-color: #212529;", css);
        }

        [Fact]
        public void GenerateThemeCss_IncludesTypographyVariables()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains("--font-family: Arial, sans-serif;", css);
            Assert.Contains("--font-size: 14px;", css);
            Assert.Contains("--heading-weight: 600;", css);
            Assert.Contains("--body-weight: 400;", css);
            Assert.Contains("--line-height: 1.5;", css);
        }

        [Fact]
        public void GenerateThemeCss_IncludesLayoutVariables()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains("--border-radius: 4px;", css);
            Assert.Contains("--spacing-unit: 1rem;", css);
            Assert.Contains("--box-shadow: 0 2px 4px rgba(0,0,0,0.1);", css);
            Assert.Contains("--max-width: 1200px;", css);
        }

        [Fact]
        public void GenerateThemeCss_IncludesCustomProperties()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains("--custom-color: #ff5733;", css);
            Assert.Contains("--transition-duration: 0.2s;", css);
        }

        [Fact]
        public void GenerateThemeCss_IncludesBaseStyles()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains("/* Base Styles */", css);
            Assert.Contains("body {", css);
            Assert.Contains("background-color: var(--background-color);", css);
            Assert.Contains("color: var(--text-color);", css);
            Assert.Contains("font-family: var(--font-family);", css);
        }

        [Fact]
        public void GenerateThemeCss_IncludesComponentStyles()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains("/* Component Styles */", css);
            Assert.Contains(".container, .main-container {", css);
            Assert.Contains(".card, .media-card {", css);
            Assert.Contains(".button, .btn, button {", css);
        }

        [Fact]
        public void GenerateThemeCss_WithMinimalTheme_HandlesNullProperties()
        {
            // Arrange
            var theme = new Theme
            {
                Id = "minimal",
                Name = "Minimal",
                Version = "1.0.0",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Background = "#ffffff",
                    Text = "#000000"
                }
                // Typography, Layout, and CustomProperties are null
            };

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.NotNull(css);
            Assert.Contains("--primary-color: #007acc;", css);
            Assert.Contains("--background-color: #ffffff;", css);
            Assert.Contains("--text-color: #000000;", css);
            // Should not throw exceptions for null properties
        }

        [Fact]
        public void GenerateThemeCss_WithEmptyCustomProperties_DoesNotIncludeCustomSection()
        {
            // Arrange
            var theme = CreateTestTheme();
            theme.CustomProperties = new Dictionary<string, string>();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.NotNull(css);
            Assert.DoesNotContain("/* Custom Properties */", css);
        }

        [Fact]
        public void GenerateThemeCss_WithNullCustomProperties_DoesNotIncludeCustomSection()
        {
            // Arrange
            var theme = CreateTestTheme();
            theme.CustomProperties = null;

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.NotNull(css);
            Assert.DoesNotContain("/* Custom Properties */", css);
        }

        [Fact]
        public void GenerateThemeCss_IncludesHoverEffects()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains(".card:hover, .media-card:hover {", css);
            Assert.Contains(".button:hover, .btn:hover, button:hover {", css);
            Assert.Contains("a:hover {", css);
        }

        [Fact]
        public void GenerateThemeCss_IncludesFormStyles()
        {
            // Arrange
            var theme = CreateTestTheme();

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains(".form-control, input, textarea, select {", css);
            Assert.Contains(".form-control:focus, input:focus, textarea:focus, select:focus {", css);
        }

        [Fact]
        public void GenerateThemeCss_WithGlowEffect_IncludesGlowInHover()
        {
            // Arrange
            var theme = CreateTestTheme();
            theme.CustomProperties["glow-effect"] = "0 0 20px rgba(0, 123, 255, 0.3)";

            // Act
            var css = ThemeCssGenerator.GenerateThemeCss(theme);

            // Assert
            Assert.Contains("box-shadow: var(--glow-effect);", css);
        }
    }
}