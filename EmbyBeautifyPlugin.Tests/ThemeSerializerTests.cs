using System;
using System.IO;
using System.Threading.Tasks;
using EmbyBeautifyPlugin.Models;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    public class ThemeSerializerTests
    {
        private Theme CreateValidTheme()
        {
            return new Theme
            {
                Id = "test-theme",
                Name = "Test Theme",
                Description = "A test theme for unit testing",
                Version = "1.0.0",
                Author = "Test Author",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Secondary = "#ff5733",
                    Background = "#ffffff",
                    Surface = "#f5f5f5",
                    Text = "#000000",
                    Accent = "#ffa500"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "Arial, sans-serif",
                    FontSize = "16px",
                    HeadingWeight = "bold",
                    BodyWeight = "normal",
                    LineHeight = "1.5"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "8px",
                    SpacingUnit = "1rem",
                    BoxShadow = "0 2px 4px rgba(0,0,0,0.1)",
                    MaxWidth = "1200px"
                }
            };
        }

        [Fact]
        public void ToJson_WithValidTheme_ReturnsJsonString()
        {
            // Arrange
            var theme = CreateValidTheme();

            // Act
            var json = ThemeSerializer.ToJson(theme);

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
            Assert.Contains("test-theme", json);
            Assert.Contains("Test Theme", json);
        }

        [Fact]
        public void ToJson_WithNullTheme_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ThemeSerializer.ToJson(null));
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsTheme()
        {
            // Arrange
            var originalTheme = CreateValidTheme();
            var json = ThemeSerializer.ToJson(originalTheme);

            // Act
            var deserializedTheme = ThemeSerializer.FromJson(json);

            // Assert
            Assert.NotNull(deserializedTheme);
            Assert.Equal(originalTheme.Id, deserializedTheme.Id);
            Assert.Equal(originalTheme.Name, deserializedTheme.Name);
            Assert.Equal(originalTheme.Version, deserializedTheme.Version);
            Assert.Equal(originalTheme.Colors.Primary, deserializedTheme.Colors.Primary);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void FromJson_WithInvalidJson_ThrowsArgumentException(string invalidJson)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ThemeSerializer.FromJson(invalidJson));
        }

        [Fact]
        public void FromJson_WithMalformedJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var malformedJson = "{ invalid json }";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ThemeSerializer.FromJson(malformedJson));
        }

        [Fact]
        public async Task SaveToFileAsync_WithValidTheme_CreatesFile()
        {
            // Arrange
            var theme = CreateValidTheme();
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await ThemeSerializer.SaveToFileAsync(theme, tempFile);

                // Assert
                Assert.True(File.Exists(tempFile));
                var content = await File.ReadAllTextAsync(tempFile);
                Assert.Contains("test-theme", content);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task SaveToFileAsync_WithNullTheme_ThrowsArgumentNullException()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => 
                    ThemeSerializer.SaveToFileAsync(null, tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task SaveToFileAsync_WithInvalidFilePath_ThrowsArgumentException(string invalidPath)
        {
            // Arrange
            var theme = CreateValidTheme();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                ThemeSerializer.SaveToFileAsync(theme, invalidPath));
        }

        [Fact]
        public async Task LoadFromFileAsync_WithValidFile_ReturnsTheme()
        {
            // Arrange
            var originalTheme = CreateValidTheme();
            var tempFile = Path.GetTempFileName();

            try
            {
                await ThemeSerializer.SaveToFileAsync(originalTheme, tempFile);

                // Act
                var loadedTheme = await ThemeSerializer.LoadFromFileAsync(tempFile);

                // Assert
                Assert.NotNull(loadedTheme);
                Assert.Equal(originalTheme.Id, loadedTheme.Id);
                Assert.Equal(originalTheme.Name, loadedTheme.Name);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadFromFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentFile = "non-existent-file.json";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                ThemeSerializer.LoadFromFileAsync(nonExistentFile));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task LoadFromFileAsync_WithInvalidFilePath_ThrowsArgumentException(string invalidPath)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                ThemeSerializer.LoadFromFileAsync(invalidPath));
        }

        [Fact]
        public void FromJsonWithValidation_WithValidJson_ReturnsThemeWithNoErrors()
        {
            // Arrange
            var originalTheme = CreateValidTheme();
            var json = ThemeSerializer.ToJson(originalTheme);

            // Act
            var (theme, errors) = ThemeSerializer.FromJsonWithValidation(json);

            // Assert
            Assert.NotNull(theme);
            Assert.Empty(errors);
            Assert.Equal(originalTheme.Id, theme.Id);
        }

        [Fact]
        public void FromJsonWithValidation_WithInvalidJson_ReturnsNullWithErrors()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act
            var (theme, errors) = ThemeSerializer.FromJsonWithValidation(invalidJson);

            // Assert
            Assert.Null(theme);
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void FromJsonWithValidation_WithValidJsonButInvalidTheme_ReturnsThemeWithValidationErrors()
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
            var json = ThemeSerializer.ToJson(invalidTheme);

            // Act
            var (theme, errors) = ThemeSerializer.FromJsonWithValidation(json);

            // Assert
            Assert.NotNull(theme);
            Assert.NotEmpty(errors);
        }
    }
}