using System.Collections.Generic;
using EmbyBeautifyPlugin.Models;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    public class ThemeTests
    {
        [Fact]
        public void Theme_Constructor_InitializesCustomProperties()
        {
            // Arrange & Act
            var theme = new Theme();

            // Assert
            Assert.NotNull(theme.CustomProperties);
            Assert.Empty(theme.CustomProperties);
        }

        [Fact]
        public void Theme_Validate_WithValidData_ReturnsNoErrors()
        {
            // Arrange
            var theme = new Theme
            {
                Id = "test-theme",
                Name = "Test Theme",
                Description = "A test theme",
                Version = "1.0.0",
                Author = "Test Author",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Background = "#ffffff",
                    Text = "#000000"
                }
            };

            // Act
            var errors = theme.Validate();

            // Assert
            Assert.Empty(errors);
            Assert.True(theme.IsValid);
        }

        [Fact]
        public void Theme_Validate_WithMissingRequiredFields_ReturnsErrors()
        {
            // Arrange
            var theme = new Theme();

            // Act
            var errors = theme.Validate();

            // Assert
            Assert.NotEmpty(errors);
            Assert.False(theme.IsValid);
            Assert.Contains(errors, e => e.Contains("Id"));
            Assert.Contains(errors, e => e.Contains("Name"));
            Assert.Contains(errors, e => e.Contains("Version"));
            Assert.Contains(errors, e => e.Contains("Colors"));
        }

        [Fact]
        public void Theme_Validate_WithInvalidVersion_ReturnsError()
        {
            // Arrange
            var theme = new Theme
            {
                Id = "test",
                Name = "Test",
                Version = "invalid-version",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Background = "#ffffff",
                    Text = "#000000"
                }
            };

            // Act
            var errors = theme.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("Version must be in format x.y.z"));
        }

        [Fact]
        public void Theme_Validate_WithTooLongId_ReturnsError()
        {
            // Arrange
            var theme = new Theme
            {
                Id = new string('a', 51), // Too long
                Name = "Test",
                Version = "1.0.0",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Background = "#ffffff",
                    Text = "#000000"
                }
            };

            // Act
            var errors = theme.Validate();

            // Assert
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void Theme_Validate_WithInvalidColors_ReturnsColorErrors()
        {
            // Arrange
            var theme = new Theme
            {
                Id = "test",
                Name = "Test",
                Version = "1.0.0",
                Colors = new ThemeColors
                {
                    Primary = "invalid-color",
                    Background = "#ffffff",
                    Text = "#000000"
                }
            };

            // Act
            var errors = theme.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("Primary color is not a valid CSS color value"));
        }

        [Fact]
        public void Theme_Validate_WithInvalidTypography_ReturnsTypographyErrors()
        {
            // Arrange
            var theme = new Theme
            {
                Id = "test",
                Name = "Test",
                Version = "1.0.0",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Background = "#ffffff",
                    Text = "#000000"
                },
                Typography = new ThemeTypography
                {
                    FontSize = "invalid-size"
                }
            };

            // Act
            var errors = theme.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("FontSize is not a valid CSS font-size value"));
        }

        [Fact]
        public void Theme_Validate_WithInvalidLayout_ReturnsLayoutErrors()
        {
            // Arrange
            var theme = new Theme
            {
                Id = "test",
                Name = "Test",
                Version = "1.0.0",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Background = "#ffffff",
                    Text = "#000000"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "invalid-radius"
                }
            };

            // Act
            var errors = theme.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("BorderRadius is not a valid CSS length value"));
        }
    }
}