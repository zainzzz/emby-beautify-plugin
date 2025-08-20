using EmbyBeautifyPlugin.Models;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    public class ThemeColorsTests
    {
        [Theory]
        [InlineData("#000000")]
        [InlineData("#fff")]
        [InlineData("#FF5733")]
        [InlineData("#ff5733aa")]
        [InlineData("rgb(255, 0, 0)")]
        [InlineData("rgba(255, 0, 0, 0.5)")]
        [InlineData("hsl(120, 100%, 50%)")]
        [InlineData("hsla(120, 100%, 50%, 0.8)")]
        [InlineData("red")]
        [InlineData("transparent")]
        public void ThemeColors_Validate_WithValidColors_ReturnsNoErrors(string color)
        {
            // Arrange
            var colors = new ThemeColors
            {
                Primary = color,
                Background = "#ffffff",
                Text = "#000000"
            };

            // Act
            var errors = colors.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid")]
        [InlineData("#gg0000")]
        [InlineData("rgb(300, 0, 0)")]
        [InlineData("#12345")]
        public void ThemeColors_Validate_WithInvalidPrimaryColor_ReturnsError(string invalidColor)
        {
            // Arrange
            var colors = new ThemeColors
            {
                Primary = invalidColor,
                Background = "#ffffff",
                Text = "#000000"
            };

            // Act
            var errors = colors.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("Primary color is not a valid CSS color value"));
        }

        [Fact]
        public void ThemeColors_Validate_WithMissingRequiredColors_ReturnsErrors()
        {
            // Arrange
            var colors = new ThemeColors();

            // Act
            var errors = colors.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("Primary color is not a valid CSS color value"));
            Assert.Contains(errors, e => e.Contains("Background color is not a valid CSS color value"));
            Assert.Contains(errors, e => e.Contains("Text color is not a valid CSS color value"));
        }

        [Fact]
        public void ThemeColors_Validate_WithValidOptionalColors_ReturnsNoErrors()
        {
            // Arrange
            var colors = new ThemeColors
            {
                Primary = "#007acc",
                Secondary = "#ff5733",
                Background = "#ffffff",
                Surface = "#f5f5f5",
                Text = "#000000",
                Accent = "#ffa500"
            };

            // Act
            var errors = colors.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ThemeColors_Validate_WithInvalidOptionalColors_ReturnsErrors()
        {
            // Arrange
            var colors = new ThemeColors
            {
                Primary = "#007acc",
                Secondary = "invalid-color",
                Background = "#ffffff",
                Surface = "another-invalid",
                Text = "#000000",
                Accent = "bad-accent"
            };

            // Act
            var errors = colors.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("Secondary color is not a valid CSS color value"));
            Assert.Contains(errors, e => e.Contains("Surface color is not a valid CSS color value"));
            Assert.Contains(errors, e => e.Contains("Accent color is not a valid CSS color value"));
        }

        [Fact]
        public void ThemeColors_Validate_WithNullOptionalColors_ReturnsNoErrors()
        {
            // Arrange
            var colors = new ThemeColors
            {
                Primary = "#007acc",
                Secondary = null,
                Background = "#ffffff",
                Surface = null,
                Text = "#000000",
                Accent = null
            };

            // Act
            var errors = colors.Validate();

            // Assert
            Assert.Empty(errors);
        }
    }
}