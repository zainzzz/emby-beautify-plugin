using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// Unit tests for ConfigurationManager
    /// </summary>
    public class ConfigurationManagerTests : IDisposable
    {
        private readonly Mock<ILogger<EmbyBeautifyPlugin.Abstracts.BaseConfigurationManager>> _mockLogger;
        private readonly string _testConfigDirectory;
        private readonly ConfigurationManager _configManager;

        public ConfigurationManagerTests()
        {
            _mockLogger = new Mock<ILogger<EmbyBeautifyPlugin.Abstracts.BaseConfigurationManager>>();
            _testConfigDirectory = Path.Combine(Path.GetTempPath(), "EmbyBeautifyPluginTests", Guid.NewGuid().ToString());
            _configManager = new ConfigurationManager(_mockLogger.Object, _testConfigDirectory);
        }

        [Fact]
        public async Task LoadConfigurationAsync_WhenFileDoesNotExist_ShouldCreateDefaultConfiguration()
        {
            // Act
            var config = await _configManager.LoadConfigurationAsync();

            // Assert
            Assert.NotNull(config);
            Assert.Equal("default", config.ActiveThemeId);
            Assert.True(config.EnableAnimations);
            Assert.True(config.EnableCustomFonts);
            Assert.Equal(300, config.AnimationDuration);
            Assert.NotNull(config.ResponsiveSettings);
            Assert.NotNull(config.CustomSettings);
        }

        [Fact]
        public async Task SaveConfigurationAsync_WithValidConfiguration_ShouldSaveSuccessfully()
        {
            // Arrange
            var config = new BeautifyConfig
            {
                ActiveThemeId = "dark",
                EnableAnimations = false,
                AnimationDuration = 500
            };

            // Act
            await _configManager.SaveConfigurationAsync(config);

            // Assert
            var loadedConfig = await _configManager.LoadConfigurationAsync();
            Assert.Equal("dark", loadedConfig.ActiveThemeId);
            Assert.False(loadedConfig.EnableAnimations);
            Assert.Equal(500, loadedConfig.AnimationDuration);
        }

        [Fact]
        public async Task SaveConfigurationAsync_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _configManager.SaveConfigurationAsync(null));
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithValidConfiguration_ShouldReturnTrue()
        {
            // Arrange
            var config = new BeautifyConfig();

            // Act
            var result = await _configManager.ValidateConfigurationAsync(config);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithNullConfiguration_ShouldReturnFalse()
        {
            // Act
            var result = await _configManager.ValidateConfigurationAsync(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithEmptyThemeId_ShouldReturnFalse()
        {
            // Arrange
            var config = new BeautifyConfig
            {
                ActiveThemeId = ""
            };

            // Act
            var result = await _configManager.ValidateConfigurationAsync(config);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithNegativeAnimationDuration_ShouldReturnFalse()
        {
            // Arrange
            var config = new BeautifyConfig
            {
                AnimationDuration = -100
            };

            // Act
            var result = await _configManager.ValidateConfigurationAsync(config);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithExcessiveAnimationDuration_ShouldReturnFalse()
        {
            // Arrange
            var config = new BeautifyConfig
            {
                AnimationDuration = 10000
            };

            // Act
            var result = await _configManager.ValidateConfigurationAsync(config);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithNullResponsiveSettings_ShouldReturnFalse()
        {
            // Arrange
            var config = new BeautifyConfig
            {
                ResponsiveSettings = null
            };

            // Act
            var result = await _configManager.ValidateConfigurationAsync(config);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithInvalidBreakpointSettings_ShouldReturnFalse()
        {
            // Arrange
            var config = new BeautifyConfig();
            config.ResponsiveSettings.Desktop.GridColumns = -1;

            // Act
            var result = await _configManager.ValidateConfigurationAsync(config);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task LoadConfigurationAsync_WithCorruptedFile_ShouldReturnDefaultConfiguration()
        {
            // Arrange
            var configPath = _configManager.GetConfigFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            await File.WriteAllTextAsync(configPath, "invalid json content");

            // Act
            var config = await _configManager.LoadConfigurationAsync();

            // Assert
            Assert.NotNull(config);
            Assert.Equal("default", config.ActiveThemeId);
        }

        [Fact]
        public async Task SaveAndLoadConfiguration_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var originalConfig = new BeautifyConfig
            {
                ActiveThemeId = "custom",
                EnableAnimations = false,
                EnableCustomFonts = false,
                AnimationDuration = 1000
            };
            originalConfig.CustomSettings["testKey"] = "testValue";
            originalConfig.ResponsiveSettings.Desktop.GridColumns = 6;

            // Act
            await _configManager.SaveConfigurationAsync(originalConfig);
            var loadedConfig = await _configManager.LoadConfigurationAsync();

            // Assert
            Assert.Equal(originalConfig.ActiveThemeId, loadedConfig.ActiveThemeId);
            Assert.Equal(originalConfig.EnableAnimations, loadedConfig.EnableAnimations);
            Assert.Equal(originalConfig.EnableCustomFonts, loadedConfig.EnableCustomFonts);
            Assert.Equal(originalConfig.AnimationDuration, loadedConfig.AnimationDuration);
            Assert.Equal(6, loadedConfig.ResponsiveSettings.Desktop.GridColumns);
            Assert.True(loadedConfig.CustomSettings.ContainsKey("testKey"));
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testConfigDirectory))
                {
                    Directory.Delete(_testConfigDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}