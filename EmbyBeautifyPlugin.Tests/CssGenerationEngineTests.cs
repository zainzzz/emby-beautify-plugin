using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// CSS生成引擎的单元测试
    /// </summary>
    public class CssGenerationEngineTests
    {
        private readonly Mock<ILogger<CssGenerationEngine>> _mockLogger;
        private readonly CssGenerationEngine _cssEngine;

        public CssGenerationEngineTests()
        {
            _mockLogger = new Mock<ILogger<CssGenerationEngine>>();
            _cssEngine = new CssGenerationEngine(_mockLogger.Object);
        }

        [Fact]
        public async Task GenerateThemeCssAsync_ValidTheme_GeneratesValidCss()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain(":root");
            css.Should().Contain("--primary-color");
            css.Should().Contain(theme.Colors.Primary);
        }

        [Fact]
        public async Task GenerateThemeCssAsync_NullTheme_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _cssEngine.Invoking(e => e.GenerateThemeCssAsync(null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithMinifyOption_GeneratesMinifiedCss()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                Minify = true,
                IncludeComments = false
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().NotContain("/*"); // 不应包含注释
            css.Should().NotContain("\n  "); // 不应包含缩进
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithoutComments_ExcludesComments()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeComments = false
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().NotContain($"/* Theme: {theme.Name}");
            css.Should().NotContain($"/* Author: {theme.Author}");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithComments_IncludesComments()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeComments = true
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain($"/* Theme: {theme.Name}");
            css.Should().Contain($"/* Author: {theme.Author}");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithoutVariables_ExcludesVariables()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeVariables = false
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().NotContain(":root");
            css.Should().NotContain("--primary-color");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithVariables_IncludesVariables()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeVariables = true
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain(":root");
            css.Should().Contain("--primary-color");
            css.Should().Contain("--secondary-color");
            css.Should().Contain("--background-color");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithBaseStyles_IncludesBaseStyles()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeBaseStyles = true
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("body {");
            css.Should().Contain("h1, h2, h3, h4, h5, h6");
            css.Should().Contain("a {");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithoutBaseStyles_ExcludesBaseStyles()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeBaseStyles = false
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().NotContain("body {");
            css.Should().NotContain("h1, h2, h3, h4, h5, h6");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithComponentStyles_IncludesComponentStyles()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeComponentStyles = true
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain(".container, .main-container");
            css.Should().Contain(".card, .media-card");
            css.Should().Contain(".button, .btn, button");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithResponsiveStyles_IncludesMediaQueries()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeResponsiveStyles = true
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("@media (min-width: 1200px)");
            css.Should().Contain("@media (min-width: 768px) and (max-width: 1199px)");
            css.Should().Contain("@media (max-width: 767px)");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithAnimations_IncludesAnimationStyles()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeAnimations = true
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("@keyframes embyBeautifyFadeIn");
            css.Should().Contain("@keyframes embyBeautifySlideUp");
            css.Should().Contain(".emby-beautify-fade-in");
            css.Should().Contain(".emby-beautify-slide-up");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithComputedVariables_IncludesRgbVariables()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeComputedVariables = true
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("--primary-color-rgb");
            css.Should().Contain("--secondary-color-rgb");
            css.Should().Contain("--accent-color-rgb");
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithCache_UsesCachedResult()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                UseCache = true
            };

            // Act
            var css1 = await _cssEngine.GenerateThemeCssAsync(theme, options);
            var css2 = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css1.Should().Be(css2);
            css1.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithOptimization_OptimizesCss()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                Optimize = true,
                Minify = false
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            // CSS应该被优化但不压缩
        }

        [Fact]
        public async Task GenerateThemeCssAsync_ComplexTheme_GeneratesCompleteCSS()
        {
            // Arrange
            var theme = new Theme
            {
                Id = "complex-theme",
                Name = "Complex Test Theme",
                Description = "A complex theme for testing",
                Version = "2.0.0",
                Author = "Test Author",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Secondary = "#6c757d",
                    Background = "#ffffff",
                    Surface = "#f8f9fa",
                    Text = "#212529",
                    Accent = "#28a745"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "Arial, sans-serif",
                    FontSize = "16px",
                    HeadingWeight = "700",
                    BodyWeight = "400",
                    LineHeight = "1.6"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "12px",
                    SpacingUnit = "1.5rem",
                    BoxShadow = "0 4px 8px rgba(0,0,0,0.15)",
                    MaxWidth = "1400px"
                },
                CustomProperties = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "glow-effect", "0 0 20px rgba(0, 123, 204, 0.3)" },
                    { "gradient-primary", "linear-gradient(135deg, #007acc, #28a745)" }
                }
            };

            var options = new CssGenerationOptions
            {
                IncludeComments = true,
                IncludeVariables = true,
                IncludeBaseStyles = true,
                IncludeComponentStyles = true,
                IncludeResponsiveStyles = true,
                IncludeAnimations = true,
                IncludeComputedVariables = true
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("Complex Test Theme");
            css.Should().Contain("--glow-effect");
            css.Should().Contain("--gradient-primary");
            css.Should().Contain("1.5rem");
            css.Should().Contain("12px");
            css.Should().Contain("1400px");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CssGenerationEngine(null));
        }

        [Fact]
        public void ClearCache_CallsClearCache_DoesNotThrow()
        {
            // Act & Assert
            _cssEngine.Invoking(e => e.ClearCache())
                .Should().NotThrow();
        }

        [Theory]
        [InlineData(true, true, true, true, true)]
        [InlineData(false, false, false, false, false)]
        [InlineData(true, false, true, false, true)]
        public async Task GenerateThemeCssAsync_VariousOptions_GeneratesAppropriateCSS(
            bool includeComments, bool includeVariables, bool includeBaseStyles, 
            bool includeComponentStyles, bool includeResponsiveStyles)
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var options = new CssGenerationOptions
            {
                IncludeComments = includeComments,
                IncludeVariables = includeVariables,
                IncludeBaseStyles = includeBaseStyles,
                IncludeComponentStyles = includeComponentStyles,
                IncludeResponsiveStyles = includeResponsiveStyles
            };

            // Act
            var css = await _cssEngine.GenerateThemeCssAsync(theme, options);

            // Assert
            css.Should().NotBeNullOrEmpty();
            
            if (includeComments)
                css.Should().Contain("/*");
            else
                css.Should().NotContain($"/* Theme: {theme.Name}");
                
            if (includeVariables)
                css.Should().Contain(":root");
            else
                css.Should().NotContain(":root");
        }
    }
}