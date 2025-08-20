using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using FluentAssertions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// Unit tests for the main Plugin class
    /// </summary>
    public class PluginTests
    {
        private readonly Mock<ILogManager> _mockLogManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IServerConfigurationManager> _mockServerConfigurationManager;
        private readonly Mock<IThemeManager> _mockThemeManager;
        private readonly Mock<IStyleInjector> _mockStyleInjector;
        private readonly Mock<IConfigurationManager> _mockConfigurationManager;
        private readonly Plugin _plugin;

        public PluginTests()
        {
            _mockLogManager = new Mock<ILogManager>();
            _mockLogger = new Mock<ILogger>();
            _mockServerConfigurationManager = new Mock<IServerConfigurationManager>();
            _mockThemeManager = new Mock<IThemeManager>();
            _mockStyleInjector = new Mock<IStyleInjector>();
            _mockConfigurationManager = new Mock<IConfigurationManager>();

            _mockLogManager.Setup(x => x.GetLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

            _plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Assert
            plugin.Should().NotBeNull();
            plugin.Name.Should().Be("Emby Beautify Plugin");
            plugin.Description.Should().Be("A plugin to beautify Emby Server interface with custom themes and enhanced UI");
        }

        [Fact]
        public void Constructor_WithNullLogManager_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new Plugin(null, _mockServerConfigurationManager.Object));
        }

        [Fact]
        public void Constructor_WithNullServerConfigurationManager_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new Plugin(_mockLogManager.Object, null));
        }

        [Fact]
        public void Run_WhenCalled_ShouldLogStartupMessages()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Act
            plugin.Run();

            // Assert
            _mockLogger.Verify(x => x.Info("Starting Emby Beautify Plugin v1.0.0"), Times.Once);
            _mockLogger.Verify(x => x.Info("Emby Beautify Plugin started successfully"), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_WhenCalledMultipleTimes_ShouldOnlyInitializeOnce()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Act
            await plugin.InitializeAsync();
            await plugin.InitializeAsync(); // Second call should be skipped

            // Assert
            _mockLogger.Verify(x => x.Debug("Plugin already initialized, skipping"), Times.Once);
        }

        [Fact]
        public async Task GetConfigurationAsync_WhenConfigurationManagerIsNull_ShouldReturnDefaultConfiguration()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Act
            var result = await plugin.GetConfigurationAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<BeautifyConfig>();
            result.ActiveThemeId.Should().Be("default");
            _mockLogger.Verify(x => x.Warn("Configuration manager not initialized, returning default configuration"), Times.Once);
        }

        [Fact]
        public async Task UpdateConfigurationAsync_WithNullConfig_ShouldThrowArgumentNullException()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => plugin.UpdateConfigurationAsync(null));
        }

        [Fact]
        public async Task UpdateConfigurationAsync_WhenConfigurationManagerIsNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);
            var config = new BeautifyConfig();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => plugin.UpdateConfigurationAsync(config));
            exception.Message.Should().Be("Plugin not properly initialized");
            _mockLogger.Verify(x => x.Error("Configuration manager not initialized, cannot update configuration"), Times.Once);
        }

        [Fact]
        public void Dispose_WhenCalled_ShouldLogDisposalMessage()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Act
            plugin.Dispose();

            // Assert
            _mockLogger.Verify(x => x.Debug("Disposing Emby Beautify Plugin resources"), Times.Once);
            _mockLogger.Verify(x => x.Info("Emby Beautify Plugin disposed successfully"), Times.Once);
        }

        [Fact]
        public void Dispose_WhenExceptionOccurs_ShouldLogError()
        {
            // Arrange
            var mockLogManager = new Mock<ILogManager>();
            var mockLogger = new Mock<ILogger>();
            
            // Setup logger to throw exception on Debug call
            mockLogger.Setup(x => x.Debug(It.IsAny<string>())).Throws(new Exception("Test exception"));
            mockLogManager.Setup(x => x.GetLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            
            var plugin = new Plugin(mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Act
            plugin.Dispose();

            // Assert
            mockLogger.Verify(x => x.ErrorException("Error disposing plugin resources", It.IsAny<Exception>()), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Name_ShouldAlwaysReturnValidString(string input)
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Act
            var name = plugin.Name;

            // Assert
            name.Should().NotBeNullOrWhiteSpace();
            name.Should().Be("Emby Beautify Plugin");
        }

        [Fact]
        public void Description_ShouldReturnValidDescription()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);

            // Act
            var description = plugin.Description;

            // Assert
            description.Should().NotBeNullOrWhiteSpace();
            description.Should().Contain("beautify");
            description.Should().Contain("Emby Server");
        }
    }
}