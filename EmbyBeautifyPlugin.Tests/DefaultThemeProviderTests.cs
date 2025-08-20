using System.Linq;
using EmbyBeautifyPlugin.Services;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    public class DefaultThemeProviderTests
    {
        [Fact]
        public void GetBuiltInThemes_ReturnsAllBuiltInThemes()
        {
            // Act
            var themes = DefaultThemeProvider.GetBuiltInThemes();

            // Assert
            Assert.NotNull(themes);
            Assert.Equal(4, themes.Count);
            Assert.Contains(themes, t => t.Id == "default-light");
            Assert.Contains(themes, t => t.Id == "default-dark");
            Assert.Contains(themes, t => t.Id == "modern-light");
            Assert.Contains(themes, t => t.Id == "modern-dark");
        }

        [Fact]
        public void GetDefaultLightTheme_ReturnsValidTheme()
        {
            // Act
            var theme = DefaultThemeProvider.GetDefaultLightTheme();

            // Assert
            Assert.NotNull(theme);
            Assert.Equal("default-light", theme.Id);
            Assert.Equal("Default Light", theme.Name);
            Assert.NotNull(theme.Colors);
            Assert.NotNull(theme.Typography);
            Assert.NotNull(theme.Layout);
            Assert.True(theme.IsValid);
        }

        [Fact]
        public void GetDefaultDarkTheme_ReturnsValidTheme()
        {
            // Act
            var theme = DefaultThemeProvider.GetDefaultDarkTheme();

            // Assert
            Assert.NotNull(theme);
            Assert.Equal("default-dark", theme.Id);
            Assert.Equal("Default Dark", theme.Name);
            Assert.NotNull(theme.Colors);
            Assert.NotNull(theme.Typography);
            Assert.NotNull(theme.Layout);
            Assert.True(theme.IsValid);
        }

        [Fact]
        public void GetModernLightTheme_ReturnsValidTheme()
        {
            // Act
            var theme = DefaultThemeProvider.GetModernLightTheme();

            // Assert
            Assert.NotNull(theme);
            Assert.Equal("modern-light", theme.Id);
            Assert.Equal("Modern Light", theme.Name);
            Assert.NotNull(theme.Colors);
            Assert.NotNull(theme.Typography);
            Assert.NotNull(theme.Layout);
            Assert.NotNull(theme.CustomProperties);
            Assert.True(theme.CustomProperties.Count > 0);
            Assert.True(theme.IsValid);
        }

        [Fact]
        public void GetModernDarkTheme_ReturnsValidTheme()
        {
            // Act
            var theme = DefaultThemeProvider.GetModernDarkTheme();

            // Assert
            Assert.NotNull(theme);
            Assert.Equal("modern-dark", theme.Id);
            Assert.Equal("Modern Dark", theme.Name);
            Assert.NotNull(theme.Colors);
            Assert.NotNull(theme.Typography);
            Assert.NotNull(theme.Layout);
            Assert.NotNull(theme.CustomProperties);
            Assert.True(theme.CustomProperties.Count > 0);
            Assert.True(theme.IsValid);
        }

        [Theory]
        [InlineData("default-light")]
        [InlineData("default-dark")]
        [InlineData("modern-light")]
        [InlineData("modern-dark")]
        public void GetThemeById_WithValidId_ReturnsTheme(string themeId)
        {
            // Act
            var theme = DefaultThemeProvider.GetThemeById(themeId);

            // Assert
            Assert.NotNull(theme);
            Assert.Equal(themeId, theme.Id);
        }

        [Theory]
        [InlineData("non-existent")]
        [InlineData("")]
        [InlineData(null)]
        public void GetThemeById_WithInvalidId_ReturnsNull(string themeId)
        {
            // Act
            var theme = DefaultThemeProvider.GetThemeById(themeId);

            // Assert
            Assert.Null(theme);
        }

        [Theory]
        [InlineData("default-light", true)]
        [InlineData("default-dark", true)]
        [InlineData("modern-light", true)]
        [InlineData("modern-dark", true)]
        [InlineData("custom-theme", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsBuiltInTheme_ReturnsCorrectResult(string themeId, bool expected)
        {
            // Act
            var result = DefaultThemeProvider.IsBuiltInTheme(themeId);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void AllBuiltInThemes_HaveUniqueIds()
        {
            // Act
            var themes = DefaultThemeProvider.GetBuiltInThemes();
            var ids = themes.Select(t => t.Id).ToList();

            // Assert
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }

        [Fact]
        public void AllBuiltInThemes_AreValid()
        {
            // Act
            var themes = DefaultThemeProvider.GetBuiltInThemes();

            // Assert
            foreach (var theme in themes)
            {
                Assert.True(theme.IsValid, $"Theme {theme.Id} is not valid");
            }
        }

        [Fact]
        public void AllBuiltInThemes_HaveRequiredProperties()
        {
            // Act
            var themes = DefaultThemeProvider.GetBuiltInThemes();

            // Assert
            foreach (var theme in themes)
            {
                Assert.NotNull(theme.Id);
                Assert.NotNull(theme.Name);
                Assert.NotNull(theme.Version);
                Assert.NotNull(theme.Colors);
                Assert.NotNull(theme.Colors.Primary);
                Assert.NotNull(theme.Colors.Background);
                Assert.NotNull(theme.Colors.Text);
            }
        }

        [Fact]
        public void ModernThemes_HaveCustomProperties()
        {
            // Act
            var modernLight = DefaultThemeProvider.GetModernLightTheme();
            var modernDark = DefaultThemeProvider.GetModernDarkTheme();

            // Assert
            Assert.NotNull(modernLight.CustomProperties);
            Assert.True(modernLight.CustomProperties.Count > 0);
            Assert.Contains("gradient-primary", modernLight.CustomProperties.Keys);
            
            Assert.NotNull(modernDark.CustomProperties);
            Assert.True(modernDark.CustomProperties.Count > 0);
            Assert.Contains("gradient-primary", modernDark.CustomProperties.Keys);
            Assert.Contains("glow-effect", modernDark.CustomProperties.Keys);
        }
    }
}