using EmbyBeautifyPlugin.Models;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    public class ThemeTypographyTests
    {
        [Theory]
        [InlineData("16px")]
        [InlineData("1.2em")]
        [InlineData("1.5rem")]
        [InlineData("100%")]
        [InlineData("12pt")]
        public void ThemeTypography_Validate_WithValidFontSize_ReturnsNoErrors(string fontSize)
        {
            // Arrange
            var typography = new ThemeTypography
            {
                FontSize = fontSize
            };

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("16")]
        [InlineData("px16")]
        [InlineData("16 px")]
        public void ThemeTypography_Validate_WithInvalidFontSize_ReturnsError(string fontSize)
        {
            // Arrange
            var typography = new ThemeTypography
            {
                FontSize = fontSize
            };

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("FontSize is not a valid CSS font-size value"));
        }

        [Theory]
        [InlineData("normal")]
        [InlineData("bold")]
        [InlineData("bolder")]
        [InlineData("lighter")]
        [InlineData("100")]
        [InlineData("400")]
        [InlineData("700")]
        [InlineData("900")]
        public void ThemeTypography_Validate_WithValidFontWeight_ReturnsNoErrors(string fontWeight)
        {
            // Arrange
            var typography = new ThemeTypography
            {
                HeadingWeight = fontWeight,
                BodyWeight = fontWeight
            };

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("50")]
        [InlineData("1000")]
        [InlineData("bold-italic")]
        public void ThemeTypography_Validate_WithInvalidFontWeight_ReturnsError(string fontWeight)
        {
            // Arrange
            var typography = new ThemeTypography
            {
                HeadingWeight = fontWeight
            };

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("HeadingWeight is not a valid CSS font-weight value"));
        }

        [Theory]
        [InlineData("normal")]
        [InlineData("1.5")]
        [InlineData("2")]
        [InlineData("24px")]
        [InlineData("1.2em")]
        [InlineData("150%")]
        public void ThemeTypography_Validate_WithValidLineHeight_ReturnsNoErrors(string lineHeight)
        {
            // Arrange
            var typography = new ThemeTypography
            {
                LineHeight = lineHeight
            };

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("px24")]
        [InlineData("1.5 em")]
        public void ThemeTypography_Validate_WithInvalidLineHeight_ReturnsError(string lineHeight)
        {
            // Arrange
            var typography = new ThemeTypography
            {
                LineHeight = lineHeight
            };

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Contains(errors, e => e.Contains("LineHeight is not a valid CSS line-height value"));
        }

        [Fact]
        public void ThemeTypography_Validate_WithAllValidProperties_ReturnsNoErrors()
        {
            // Arrange
            var typography = new ThemeTypography
            {
                FontFamily = "Arial, sans-serif",
                FontSize = "16px",
                HeadingWeight = "bold",
                BodyWeight = "normal",
                LineHeight = "1.5"
            };

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ThemeTypography_Validate_WithNullProperties_ReturnsNoErrors()
        {
            // Arrange
            var typography = new ThemeTypography();

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ThemeTypography_Validate_WithEmptyProperties_ReturnsNoErrors()
        {
            // Arrange
            var typography = new ThemeTypography
            {
                FontFamily = "",
                FontSize = "",
                HeadingWeight = "",
                BodyWeight = "",
                LineHeight = ""
            };

            // Act
            var errors = typography.Validate();

            // Assert
            Assert.Empty(errors);
        }
    }
}