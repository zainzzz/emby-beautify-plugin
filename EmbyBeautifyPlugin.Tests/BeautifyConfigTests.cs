using EmbyBeautifyPlugin.Models;
using System.Collections.Generic;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// Unit tests for BeautifyConfig model
    /// </summary>
    public class BeautifyConfigTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var config = new BeautifyConfig();

            // Assert
            Assert.Equal("default", config.ActiveThemeId);
            Assert.True(config.EnableAnimations);
            Assert.True(config.EnableCustomFonts);
            Assert.Equal(300, config.AnimationDuration);
            Assert.NotNull(config.ResponsiveSettings);
            Assert.NotNull(config.CustomSettings);
            Assert.Empty(config.CustomSettings);
        }

        [Fact]
        public void ResponsiveSettings_ShouldInitializeWithDefaultBreakpoints()
        {
            // Act
            var config = new BeautifyConfig();

            // Assert
            Assert.NotNull(config.ResponsiveSettings.Desktop);
            Assert.NotNull(config.ResponsiveSettings.Tablet);
            Assert.NotNull(config.ResponsiveSettings.Mobile);
            
            // Check default values for each breakpoint
            Assert.Equal(4, config.ResponsiveSettings.Desktop.GridColumns);
            Assert.Equal("1rem", config.ResponsiveSettings.Desktop.GridGap);
            Assert.Equal(1.0, config.ResponsiveSettings.Desktop.FontScale);
        }

        [Fact]
        public void CustomSettings_ShouldAllowAddingCustomValues()
        {
            // Arrange
            var config = new BeautifyConfig();

            // Act
            config.CustomSettings["customThemeColor"] = "#FF5733";
            config.CustomSettings["enableExperimentalFeatures"] = true;
            config.CustomSettings["maxCacheSize"] = 100;

            // Assert
            Assert.Equal("#FF5733", config.CustomSettings["customThemeColor"]);
            Assert.Equal(true, config.CustomSettings["enableExperimentalFeatures"]);
            Assert.Equal(100, config.CustomSettings["maxCacheSize"]);
            Assert.Equal(3, config.CustomSettings.Count);
        }

        [Fact]
        public void Properties_ShouldAllowModification()
        {
            // Arrange
            var config = new BeautifyConfig();

            // Act
            config.ActiveThemeId = "dark-mode";
            config.EnableAnimations = false;
            config.EnableCustomFonts = false;
            config.AnimationDuration = 500;

            // Assert
            Assert.Equal("dark-mode", config.ActiveThemeId);
            Assert.False(config.EnableAnimations);
            Assert.False(config.EnableCustomFonts);
            Assert.Equal(500, config.AnimationDuration);
        }

        [Fact]
        public void ResponsiveSettings_ShouldAllowBreakpointModification()
        {
            // Arrange
            var config = new BeautifyConfig();

            // Act
            config.ResponsiveSettings.Desktop.GridColumns = 8;
            config.ResponsiveSettings.Desktop.GridGap = "2rem";
            config.ResponsiveSettings.Desktop.FontScale = 1.2;

            config.ResponsiveSettings.Mobile.GridColumns = 2;
            config.ResponsiveSettings.Mobile.GridGap = "0.5rem";
            config.ResponsiveSettings.Mobile.FontScale = 0.9;

            // Assert
            Assert.Equal(8, config.ResponsiveSettings.Desktop.GridColumns);
            Assert.Equal("2rem", config.ResponsiveSettings.Desktop.GridGap);
            Assert.Equal(1.2, config.ResponsiveSettings.Desktop.FontScale);

            Assert.Equal(2, config.ResponsiveSettings.Mobile.GridColumns);
            Assert.Equal("0.5rem", config.ResponsiveSettings.Mobile.GridGap);
            Assert.Equal(0.9, config.ResponsiveSettings.Mobile.FontScale);
        }
    }
}