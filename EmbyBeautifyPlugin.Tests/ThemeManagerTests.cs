using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    public class ThemeManagerTests
    {
        private readonly Mock<ILogger<EmbyBeautifyPlugin.Abstracts.BaseThemeManager>> _mockLogger;
        private readonly Mock<IConfigurationManager> _mockConfigManager;
        private readonly ThemeManager _themeManager;

        public ThemeManagerTests()
        {
            _mockLogger = new Mock<ILogger<EmbyBeautifyPlugin.Abstracts.BaseThemeManager>>();
            _mockConfigManager = new Mock<IConfigurationManager>();
            _themeManager = new ThemeManager(_mockLogger.Object, _mockConfigManager.Object);
        }

        private Theme CreateValidTheme(string id = "test-theme")
        {
            return new Theme
            {
                Id = id,
                Name = "Test Theme",
                Version = "1.0.0",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Background = "#ffffff",
                    Text = "#000000"
                }
            };
        }

        private BeautifyConfig CreateDefaultConfig()
        {
            return new BeautifyConfig
            {
                ActiveThemeId = "default",
                EnableAnimations = true,
                EnableCustomFonts = true,
                AnimationDuration = 300
            };
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ThemeManager(null, _mockConfigManager.Object));
        }

        [Fact]
        public void Constructor_WithNullConfigurationManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ThemeManager(_mockLogger.Object, null));
        }

        [Fact]
        public async Task InitializeAsync_ShouldCompleteSuccessfully()
        {
            // Arrange
            _mockConfigManager.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(CreateDefaultConfig());

            // Act
            await _themeManager.InitializeAsync();

            // Assert
            var themes = await _themeManager.GetAvailableThemesAsync();
            Assert.NotEmpty(themes);
        }

        [Fact]
        public async Task RegisterThemeAsync_WithValidTheme_ReturnsTrue()
        {
            // Arrange
            var theme = CreateValidTheme();

            // Act
            var result = await _themeManager.RegisterThemeAsync(theme);

            // Assert
            Assert.True(result);
            var themes = await _themeManager.GetAvailableThemesAsync();
            Assert.Contains(themes, t => t.Id == theme.Id);
        }

        [Fact]
        public async Task RegisterThemeAsync_WithNullTheme_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _themeManager.RegisterThemeAsync(null));
        }

        [Fact]
        public async Task RegisterThemeAsync_WithInvalidTheme_ReturnsFalse()
        {
            // Arrange
            var invalidTheme = new Theme
            {
                // Missing required fields
                Colors = new ThemeColors
                {
                    Primary = "invalid-color",
                    Background = "#ffffff",
                    Text = "#000000"
                }
            };

            // Act
            var result = await _themeManager.RegisterThemeAsync(invalidTheme);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RegisterThemeAsync_WithExistingTheme_UpdatesTheme()
        {
            // Arrange
            var theme1 = CreateValidTheme("test-theme");
            var theme2 = CreateValidTheme("test-theme");
            theme2.Name = "Updated Theme";

            // Act
            await _themeManager.RegisterThemeAsync(theme1);
            await _themeManager.RegisterThemeAsync(theme2);

            // Assert
            var themes = await _themeManager.GetAvailableThemesAsync();
            var registeredTheme = themes.Find(t => t.Id == "test-theme");
            Assert.NotNull(registeredTheme);
            Assert.Equal("Updated Theme", registeredTheme.Name);
        }

        [Fact]
        public async Task SetActiveThemeAsync_WithValidThemeId_SetsActiveTheme()
        {
            // Arrange
            var theme = CreateValidTheme();
            var config = CreateDefaultConfig();
            
            _mockConfigManager.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);
            _mockConfigManager.Setup(x => x.SaveConfigurationAsync(It.IsAny<BeautifyConfig>()))
                .Returns(Task.CompletedTask);

            await _themeManager.RegisterThemeAsync(theme);

            // Act
            await _themeManager.SetActiveThemeAsync(theme.Id);

            // Assert
            var activeTheme = await _themeManager.GetActiveThemeAsync();
            Assert.Equal(theme.Id, activeTheme.Id);
            
            _mockConfigManager.Verify(x => x.SaveConfigurationAsync(It.Is<BeautifyConfig>(
                c => c.ActiveThemeId == theme.Id)), Times.Once);
        }

        [Fact]
        public async Task SetActiveThemeAsync_WithNonExistentThemeId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _themeManager.SetActiveThemeAsync("non-existent-theme"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task SetActiveThemeAsync_WithNullOrEmptyThemeId_ThrowsArgumentException(string themeId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _themeManager.SetActiveThemeAsync(themeId));
        }

        [Fact]
        public async Task UnregisterThemeAsync_WithValidThemeId_ReturnsTrue()
        {
            // Arrange
            var theme = CreateValidTheme();
            await _themeManager.RegisterThemeAsync(theme);

            // Act
            var result = await _themeManager.UnregisterThemeAsync(theme.Id);

            // Assert
            Assert.True(result);
            var themes = await _themeManager.GetAvailableThemesAsync();
            Assert.DoesNotContain(themes, t => t.Id == theme.Id);
        }

        [Fact]
        public async Task UnregisterThemeAsync_WithNonExistentThemeId_ReturnsFalse()
        {
            // Act
            var result = await _themeManager.UnregisterThemeAsync("non-existent-theme");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task UnregisterThemeAsync_WithNullOrEmptyThemeId_ThrowsArgumentException(string themeId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _themeManager.UnregisterThemeAsync(themeId));
        }

        [Fact]
        public async Task UnregisterThemeAsync_WithActiveTheme_SwitchesToDefault()
        {
            // Arrange
            var theme = CreateValidTheme();
            var config = CreateDefaultConfig();
            
            _mockConfigManager.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);
            _mockConfigManager.Setup(x => x.SaveConfigurationAsync(It.IsAny<BeautifyConfig>()))
                .Returns(Task.CompletedTask);

            await _themeManager.RegisterThemeAsync(theme);
            await _themeManager.SetActiveThemeAsync(theme.Id);

            // Act
            await _themeManager.UnregisterThemeAsync(theme.Id);

            // Assert
            var activeTheme = await _themeManager.GetActiveThemeAsync();
            Assert.NotEqual(theme.Id, activeTheme.Id);
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithValidTheme_ReturnsValidCss()
        {
            // Arrange
            var theme = new Theme
            {
                Id = "test-theme",
                Name = "Test Theme",
                Version = "1.0.0",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Background = "#ffffff",
                    Text = "#000000"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "Arial, sans-serif",
                    FontSize = "16px"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "4px",
                    SpacingUnit = "1rem"
                }
            };

            // Act
            var css = await _themeManager.GenerateThemeCssAsync(theme);

            // Assert
            Assert.NotNull(css);
            Assert.Contains(":root {", css);
            Assert.Contains("--primary-color: #007acc;", css);
            Assert.Contains("--background-color: #ffffff;", css);
            Assert.Contains("--text-color: #000000;", css);
            Assert.Contains("--font-family: Arial, sans-serif;", css);
            Assert.Contains("--font-size: 16px;", css);
            Assert.Contains("--border-radius: 4px;", css);
            Assert.Contains("--spacing-unit: 1rem;", css);
            Assert.Contains("body {", css);
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithNullTheme_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _themeManager.GenerateThemeCssAsync(null));
        }

        [Fact]
        public async Task GenerateThemeCssAsync_WithCustomProperties_IncludesCustomProperties()
        {
            // Arrange
            var theme = CreateValidTheme();
            theme.CustomProperties = new Dictionary<string, string>
            {
                { "custom-color", "#ff5733" },
                { "custom-size", "24px" }
            };

            // Act
            var css = await _themeManager.GenerateThemeCssAsync(theme);

            // Assert
            Assert.Contains("--custom-color: #ff5733;", css);
            Assert.Contains("--custom-size: 24px;", css);
        }

        [Fact]
        public async Task GetActiveThemeAsync_WithNoActiveTheme_ReturnsDefaultTheme()
        {
            // Arrange
            _mockConfigManager.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(new BeautifyConfig());

            // Act
            var activeTheme = await _themeManager.GetActiveThemeAsync();

            // Assert
            Assert.NotNull(activeTheme);
            Assert.Equal("default", activeTheme.Id);
        }

        [Fact]
        public async Task GetAvailableThemesAsync_ReturnsAllRegisteredThemes()
        {
            // Arrange
            var theme1 = CreateValidTheme("theme1");
            var theme2 = CreateValidTheme("theme2");
            
            await _themeManager.RegisterThemeAsync(theme1);
            await _themeManager.RegisterThemeAsync(theme2);

            // Act
            var themes = await _themeManager.GetAvailableThemesAsync();

            // Assert
            Assert.Contains(themes, t => t.Id == "theme1");
            Assert.Contains(themes, t => t.Id == "theme2");
        }
    }
}