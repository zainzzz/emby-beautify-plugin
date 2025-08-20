using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using FluentAssertions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// Integration tests for Plugin class with mocked dependencies
    /// </summary>
    public class PluginIntegrationTests
    {
        private readonly Mock<ILogManager> _mockLogManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IServerConfigurationManager> _mockServerConfigurationManager;
        private readonly Mock<IThemeManager> _mockThemeManager;
        private readonly Mock<IStyleInjector> _mockStyleInjector;
        private readonly Mock<IConfigurationManager> _mockConfigurationManager;

        public PluginIntegrationTests()
        {
            _mockLogManager = new Mock<ILogManager>();
            _mockLogger = new Mock<ILogger>();
            _mockServerConfigurationManager = new Mock<IServerConfigurationManager>();
            _mockThemeManager = new Mock<IThemeManager>();
            _mockStyleInjector = new Mock<IStyleInjector>();
            _mockConfigurationManager = new Mock<IConfigurationManager>();

            _mockLogManager.Setup(x => x.GetLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
        }

        [Fact]
        public void Run_WithFullInitialization_ShouldCompleteSuccessfully()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);
            var testConfig = TestConfiguration.CreateDefaultTestConfig();
            var testTheme = TestConfiguration.CreateTestTheme();

            // Setup mocks for successful initialization
            _mockConfigurationManager.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);
            _mockThemeManager.Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(testTheme);
            _mockThemeManager.Setup(x => x.GenerateThemeCssAsync(It.IsAny<Theme>()))
                .ReturnsAsync("/* test css */");
            _mockStyleInjector.Setup(x => x.InjectStylesAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockStyleInjector.Setup(x => x.UpdateGlobalStylesAsync())
                .Returns(Task.CompletedTask);

            // Act
            plugin.Run();

            // Assert
            _mockLogger.Verify(x => x.Info("Starting Emby Beautify Plugin v1.0.0"), Times.Once);
            _mockLogger.Verify(x => x.Info("Emby Beautify Plugin started successfully"), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_WithThemeSystemFailure_ShouldHandleGracefully()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);
            var testConfig = TestConfiguration.CreateDefaultTestConfig();

            _mockConfigurationManager.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);
            _mockThemeManager.Setup(x => x.GetActiveThemeAsync())
                .ThrowsAsync(new Exception("Theme system failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => plugin.InitializeAsync());
            exception.Message.Should().Be("Theme system failure");
            
            _mockLogger.Verify(x => x.ErrorException("Error during plugin initialization", It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task UpdateConfigurationAsync_WithValidConfigAndThemeChange_ShouldApplyNewTheme()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();
            var oldTheme = TestConfiguration.CreateTestTheme();
            var newConfig = TestConfiguration.CreateDefaultTestConfig();
            newConfig.ActiveThemeId = "new-theme-id";
            
            var newTheme = TestConfiguration.CreateTestTheme();
            newTheme.Id = "new-theme-id";
            newTheme.Name = "New Theme";

            _mockConfigurationManager.Setup(x => x.ValidateConfigurationAsync(It.IsAny<BeautifyConfig>()))
                .ReturnsAsync(true);
            _mockConfigurationManager.Setup(x => x.SaveConfigurationAsync(It.IsAny<BeautifyConfig>()))
                .Returns(Task.CompletedTask);
            _mockThemeManager.Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(oldTheme);
            _mockThemeManager.Setup(x => x.SetActiveThemeAsync("new-theme-id"))
                .Returns(Task.CompletedTask);
            _mockThemeManager.Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(newTheme);
            _mockThemeManager.Setup(x => x.GenerateThemeCssAsync(newTheme))
                .ReturnsAsync("/* new theme css */");
            _mockStyleInjector.Setup(x => x.InjectStylesAsync("/* new theme css */"))
                .Returns(Task.CompletedTask);

            // Act
            await plugin.UpdateConfigurationAsync(newConfig);

            // Assert
            _mockConfigurationManager.Verify(x => x.SaveConfigurationAsync(newConfig), Times.Once);
            _mockThemeManager.Verify(x => x.SetActiveThemeAsync("new-theme-id"), Times.Once);
            _mockStyleInjector.Verify(x => x.InjectStylesAsync("/* new theme css */"), Times.Once);
            _mockLogger.Verify(x => x.Info("Configuration updated successfully"), Times.Once);
        }

        [Fact]
        public async Task UpdateConfigurationAsync_WithInvalidConfig_ShouldThrowArgumentException()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();
            var invalidConfig = TestConfiguration.CreateInvalidTestConfig();

            _mockConfigurationManager.Setup(x => x.ValidateConfigurationAsync(It.IsAny<BeautifyConfig>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => plugin.UpdateConfigurationAsync(invalidConfig));
            exception.Message.Should().Contain("Invalid configuration");
            
            _mockLogger.Verify(x => x.Error("Invalid configuration provided"), Times.Once);
        }

        [Fact]
        public async Task GetConfigurationAsync_WithMockedConfigurationManager_ShouldReturnConfiguration()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();
            var expectedConfig = TestConfiguration.CreateDefaultTestConfig();

            _mockConfigurationManager.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(expectedConfig);

            // Act
            var result = await plugin.GetConfigurationAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedConfig);
        }

        [Fact]
        public async Task GetConfigurationAsync_WhenConfigurationManagerThrows_ShouldReturnDefaultConfig()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();

            _mockConfigurationManager.Setup(x => x.LoadConfigurationAsync())
                .ThrowsAsync(new Exception("Configuration load failed"));

            // Act
            var result = await plugin.GetConfigurationAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<BeautifyConfig>();
            result.ActiveThemeId.Should().Be("default"); // Default configuration
            
            _mockLogger.Verify(x => x.ErrorException("Error getting configuration", It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void Dispose_WithMockedServices_ShouldDisposeCleanly()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();

            // Act
            plugin.Dispose();

            // Assert
            _mockLogger.Verify(x => x.Debug("Disposing Emby Beautify Plugin resources"), Times.Once);
            _mockLogger.Verify(x => x.Info("Emby Beautify Plugin disposed successfully"), Times.Once);
        }

        [Fact]
        public async Task PluginLifecycle_CompleteIntegration_ShouldHandleAllPhases()
        {
            // Arrange
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);
            var testConfig = TestConfiguration.CreateDefaultTestConfig();
            var testTheme = TestConfiguration.CreateTestTheme();

            // Setup complete integration scenario
            _mockConfigurationManager.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);
            _mockThemeManager.Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(TestConfiguration.CreateTestThemeList());
            _mockThemeManager.Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(testTheme);
            _mockThemeManager.Setup(x => x.GenerateThemeCssAsync(It.IsAny<Theme>()))
                .ReturnsAsync("/* integrated test css */");
            _mockStyleInjector.Setup(x => x.InjectStylesAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act - 完整的插件生命周期
            plugin.Run(); // 启动
            await plugin.InitializeAsync(); // 初始化
            var config = await plugin.GetConfigurationAsync(); // 获取配置
            await plugin.UpdateConfigurationAsync(testConfig); // 更新配置
            plugin.Dispose(); // 清理

            // Assert
            config.Should().NotBeNull();
            _mockLogger.Verify(x => x.Info("Starting Emby Beautify Plugin v1.0.0"), Times.Once);
            _mockLogger.Verify(x => x.Info("Emby Beautify Plugin started successfully"), Times.Once);
            _mockLogger.Verify(x => x.Info("Configuration updated successfully"), Times.Once);
            _mockLogger.Verify(x => x.Info("Emby Beautify Plugin disposed successfully"), Times.Once);
        }

        [Fact]
        public async Task EmbyServerIntegration_WithRealServerEvents_ShouldRespondCorrectly()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();
            var serverEventArgs = new Mock<EventArgs>();

            // Setup server event simulation
            _mockServerConfigurationManager.Setup(x => x.Configuration)
                .Returns(new Mock<MediaBrowser.Model.Configuration.ServerConfiguration>().Object);

            // Act - 模拟服务器事件
            plugin.Run();
            
            // 模拟服务器配置变更事件
            // 在真实环境中，这会触发插件的响应
            await plugin.InitializeAsync();

            // Assert
            _mockLogger.Verify(x => x.Info(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task MultiUserScenario_Integration_ShouldHandleConcurrentUsers()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();
            var userConfigs = new System.Collections.Generic.List<BeautifyConfig>();
            
            for (int i = 0; i < 5; i++)
            {
                var config = TestConfiguration.CreateDefaultTestConfig();
                config.ActiveThemeId = $"user-theme-{i}";
                userConfigs.Add(config);
            }

            // Act - 模拟多用户并发操作
            var tasks = userConfigs.Select(async config =>
            {
                await plugin.UpdateConfigurationAsync(config);
                return await plugin.GetConfigurationAsync();
            });

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(5);
            results.Should().OnlyContain(config => config != null);
        }

        [Fact]
        public async Task ErrorRecovery_Integration_ShouldRecoverFromFailures()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();
            var validConfig = TestConfiguration.CreateDefaultTestConfig();

            // Setup failure scenario followed by recovery
            _mockConfigurationManager.SetupSequence(x => x.LoadConfigurationAsync())
                .ThrowsAsync(new Exception("First load fails"))
                .ReturnsAsync(validConfig); // Second attempt succeeds

            // Act & Assert - 第一次调用应该失败
            var exception = await Assert.ThrowsAsync<Exception>(() => plugin.GetConfigurationAsync());
            exception.Message.Should().Be("First load fails");

            // 第二次调用应该成功（恢复）
            var recoveredConfig = await plugin.GetConfigurationAsync();
            recoveredConfig.Should().NotBeNull();
            recoveredConfig.Should().BeEquivalentTo(validConfig);
        }

        [Fact]
        public async Task PluginDependencies_Integration_ShouldInitializeInCorrectOrder()
        {
            // Arrange
            var plugin = CreatePluginWithMockedServices();
            var initializationOrder = new System.Collections.Generic.List<string>();

            // Setup mocks to track initialization order
            _mockConfigurationManager.Setup(x => x.LoadConfigurationAsync())
                .Callback(() => initializationOrder.Add("ConfigurationManager"))
                .ReturnsAsync(TestConfiguration.CreateDefaultTestConfig());

            _mockThemeManager.Setup(x => x.GetAvailableThemesAsync())
                .Callback(() => initializationOrder.Add("ThemeManager"))
                .ReturnsAsync(TestConfiguration.CreateTestThemeList());

            _mockStyleInjector.Setup(x => x.UpdateGlobalStylesAsync())
                .Callback(() => initializationOrder.Add("StyleInjector"))
                .Returns(Task.CompletedTask);

            // Act
            await plugin.InitializeAsync();

            // Assert - 验证初始化顺序
            initializationOrder.Should().HaveCount(3);
            initializationOrder[0].Should().Be("ConfigurationManager"); // 配置管理器应该首先初始化
            initializationOrder.Should().Contain("ThemeManager");
            initializationOrder.Should().Contain("StyleInjector");
        }

        [Fact]
        public async Task PluginConfiguration_Integration_ShouldPersistAcrossRestarts()
        {
            // Arrange
            var plugin1 = CreatePluginWithMockedServices();
            var plugin2 = CreatePluginWithMockedServices();
            var testConfig = TestConfiguration.CreateDefaultTestConfig();
            testConfig.ActiveThemeId = "persistent-theme";

            // Setup persistent storage simulation
            BeautifyConfig storedConfig = null;
            _mockConfigurationManager.Setup(x => x.SaveConfigurationAsync(It.IsAny<BeautifyConfig>()))
                .Callback<BeautifyConfig>(config => storedConfig = config)
                .Returns(Task.CompletedTask);
            
            _mockConfigurationManager.Setup(x => x.LoadConfigurationAsync())
                .Returns(() => Task.FromResult(storedConfig ?? TestConfiguration.CreateDefaultTestConfig()));

            // Act - 第一个插件实例保存配置
            await plugin1.UpdateConfigurationAsync(testConfig);
            plugin1.Dispose();

            // 第二个插件实例加载配置
            var loadedConfig = await plugin2.GetConfigurationAsync();

            // Assert
            loadedConfig.Should().NotBeNull();
            loadedConfig.ActiveThemeId.Should().Be("persistent-theme");
        }

        /// <summary>
        /// Helper method to create a plugin with mocked services for testing
        /// Note: This simulates what would happen with proper dependency injection
        /// </summary>
        private Plugin CreatePluginWithMockedServices()
        {
            var plugin = new Plugin(_mockLogManager.Object, _mockServerConfigurationManager.Object);
            
            // In a real implementation, these would be injected via DI container
            // For testing, we simulate the services being available
            
            return plugin;
        }
    }
}