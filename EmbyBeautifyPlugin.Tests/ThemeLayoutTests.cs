using EmbyBeautifyPlugin.Models;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    public class ThemeLayoutTests
    {
        [Theory]
        [InlineData("4px")]
        [InlineData("0.5rem")]
        [InlineData("1em")]
        [InlineData("10%")]
        [InlineData("auto")]
        [InlineData("0")]
        public void ThemeLayout_Validate_WithValidCssLength_ReturnsNoErrors(string length)
        {
            // Arrange
            var layout = new ThemeLayout
            {
                BorderRadius = length,
                SpacingUnit = length,
                MaxWidth = length
            };

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("4 px")]
        [InlineData("px4")]
        [InlineData("4px5px")]
        public void ThemeLayout_Validate_WithInvalidCssLength_ReturnsError(string invalidLength)
        {
            // Arrange
            var layout = new ThemeLayout
            {
                BorderRadius = invalidLength
            };

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("BorderRadius is not a valid CSS length value"));
        }

        [Fact]
        public void ThemeLayout_Validate_WithInvalidSpacingUnit_ReturnsError()
        {
            // Arrange
            var layout = new ThemeLayout
            {
                SpacingUnit = "invalid-spacing"
            };

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("SpacingUnit is not a valid CSS length value"));
        }

        [Fact]
        public void ThemeLayout_Validate_WithInvalidMaxWidth_ReturnsError()
        {
            // Arrange
            var layout = new ThemeLayout
            {
                MaxWidth = "invalid-width"
            };

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("MaxWidth is not a valid CSS length value"));
        }

        [Theory]
        [InlineData("none")]
        [InlineData("0 2px 4px rgba(0,0,0,0.1)")]
        [InlineData("2px 2px 4px #000000")]
        [InlineData("inset 0 1px 3px rgba(0,0,0,0.3)")]
        public void ThemeLayout_Validate_WithValidBoxShadow_ReturnsNoErrors(string boxShadow)
        {
            // Arrange
            var layout = new ThemeLayout
            {
                BoxShadow = boxShadow
            };

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ThemeLayout_Validate_WithInvalidBoxShadow_ReturnsError(string invalidBoxShadow)
        {
            // Arrange
            var layout = new ThemeLayout
            {
                BoxShadow = invalidBoxShadow
            };

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("BoxShadow is not a valid CSS box-shadow value"));
        }

        [Fact]
        public void ThemeLayout_Validate_WithAllValidProperties_ReturnsNoErrors()
        {
            // Arrange
            var layout = new ThemeLayout
            {
                BorderRadius = "8px",
                SpacingUnit = "1rem",
                BoxShadow = "0 2px 4px rgba(0,0,0,0.1)",
                MaxWidth = "1200px"
            };

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ThemeLayout_Validate_WithNullProperties_ReturnsNoErrors()
        {
            // Arrange
            var layout = new ThemeLayout();

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ThemeLayout_Validate_WithMultipleInvalidProperties_ReturnsMultipleErrors()
        {
            // Arrange
            var layout = new ThemeLayout
            {
                BorderRadius = "invalid-radius",
                SpacingUnit = "invalid-spacing",
                MaxWidth = "invalid-width"
            };

            // Act
            var errors = layout.Validate();

            // Assert
            Assert.Equal(3, errors.Count);
            Assert.Contains(errors, e => e.Contains("BorderRadius"));
            Assert.Contains(errors, e => e.Contains("SpacingUnit"));
            Assert.Contains(errors, e => e.Contains("MaxWidth"));
        }
    }
}