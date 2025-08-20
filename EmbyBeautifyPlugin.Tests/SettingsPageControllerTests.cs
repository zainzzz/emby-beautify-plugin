using EmbyBeautifyPlugin.Controllers;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 设置页面控制器的单元测试
    /// </summary>
    public class SettingsPageControllerTests
    {
        private readonly Mock<IConfigurationManager> _mockConfigurationManager;
        private readonly Mock<IThemeManager> _mockThemeManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<ILogManager> _mockLogManager;
        private readonly SettingsPageController _controller;

        public SettingsPageControllerTests()
        {
            _mockConfigurationManager = new Mock<IConfigurationManager>();
            _mockThemeManager = new Mock<IThemeManager>();
            _mockLogger = new Mock<ILogger>();
            _mockLogManager = new Mock<ILogManager>();

            _mockLogManager.Setup(x => x.GetLogger(It.IsAny<string>()))
                          .Returns(_mockLogger.Object);

            _controller = new SettingsPageController(
                _mockConfigurationManager.Object,
                _mockThemeManager.Object,
                _mockLogManager.Object);
        }

        [Fact]
        public async Task Get_GetSettingsPageRequest_ReturnsHtmlContent()
        {
            // Arrange
            var request = new GetSettingsPageRequest();
            var testConfig = TestConfiguration.GetSampleBeautifyConfig();
            var testThemes = TestConfiguration.GetSampleThemes();

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<string>();
            
            var htmlContent = result as string;
            htmlContent.Should().Contain("Emby 美化插件");
            htmlContent.Should().Contain("设置");
        }

        [Fact]
        public async Task Get_GetSettingsPageRequest_ConfigurationManagerThrows_ReturnsErrorPage()
        {
            // Arrange
            var request = new GetSettingsPageRequest();
            var errorMessage = "配置加载失败";

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<string>();
            
            var htmlContent = result as string;
            htmlContent.Should().Contain("错误");
            htmlContent.Should().Contain("加载设置页面失败");
        }

        [Fact]
        public async Task Get_GetPreviewPageRequest_ReturnsHtmlContent()
        {
            // Arrange
            var request = new GetPreviewPageRequest();
            var testThemes = TestConfiguration.GetSampleThemes();

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<string>();
            
            var htmlContent = result as string;
            htmlContent.Should().Contain("主题预览");
            htmlContent.Should().Contain("实时预览");
        }

        [Fact]
        public void Constructor_NullConfigurationManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new SettingsPageController(null, _mockThemeManager.Object, _mockLogManager.Object));
        }

        [Fact]
        public void Constructor_NullThemeManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new SettingsPageController(_mockConfigurationManager.Object, null, _mockLogManager.Object));
        }

        [Fact]
        public void Constructor_NullLogManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new SettingsPageController(_mockConfigurationManager.Object, _mockThemeManager.Object, null));
        }

        [Fact]
        public async Task Get_GetSettingsPageRequest_LogsDebugMessages()
        {
            // Arrange
            var request = new GetSettingsPageRequest();
            var testConfig = TestConfiguration.GetSampleBeautifyConfig();
            var testThemes = TestConfiguration.GetSampleThemes();

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            // Act
            await _controller.Get(request);

            // Assert
            _mockLogger.Verify(
                x => x.Debug("开始获取设置页面"),
                Times.Once);

            _mockLogger.Verify(
                x => x.Debug("成功获取设置页面"),
                Times.Once);
        }

        [Fact]
        public async Task Get_GetSettingsPageRequest_ConfigurationDataIsEmbedded()
        {
            // Arrange
            var request = new GetSettingsPageRequest();
            var testConfig = TestConfiguration.GetSampleBeautifyConfig();
            var testThemes = TestConfiguration.GetSampleThemes();

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            // Act
            var result = await _controller.Get(request);

            // Assert
            var htmlContent = result as string;
            
            // 验证配置数据被嵌入到HTML中
            htmlContent.Should().Contain(testConfig.ActiveThemeId);
            htmlContent.Should().Contain(testConfig.AnimationDuration.ToString());
            
            // 验证主题数据被嵌入到HTML中
            foreach (var theme in testThemes)
            {
                htmlContent.Should().Contain(theme.Id);
                htmlContent.Should().Contain(theme.Name);
            }
        }

        [Fact]
        public async Task Get_GetSettingsPageRequest_VerifyServiceCalls()
        {
            // Arrange
            var request = new GetSettingsPageRequest();
            var testConfig = TestConfiguration.GetSampleBeautifyConfig();
            var testThemes = TestConfiguration.GetSampleThemes();

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            // Act
            await _controller.Get(request);

            // Assert
            _mockConfigurationManager.Verify(x => x.LoadConfigurationAsync(), Times.Once);
            _mockThemeManager.Verify(x => x.GetAvailableThemesAsync(), Times.Once);
        }
    }
}