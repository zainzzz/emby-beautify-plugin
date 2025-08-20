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
    /// 主题选择控制器的单元测试
    /// </summary>
    public class ThemeSelectionControllerTests
    {
        private readonly Mock<IThemeManager> _mockThemeManager;
        private readonly Mock<IConfigurationManager> _mockConfigurationManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<ILogManager> _mockLogManager;
        private readonly ThemeSelectionController _controller;

        public ThemeSelectionControllerTests()
        {
            _mockThemeManager = new Mock<IThemeManager>();
            _mockConfigurationManager = new Mock<IConfigurationManager>();
            _mockLogger = new Mock<ILogger>();
            _mockLogManager = new Mock<ILogManager>();

            _mockLogManager.Setup(x => x.GetLogger(It.IsAny<string>()))
                          .Returns(_mockLogger.Object);

            _controller = new ThemeSelectionController(
                _mockThemeManager.Object,
                _mockConfigurationManager.Object,
                _mockLogManager.Object);
        }

        [Fact]
        public async Task Get_GetThemeSelectionRequest_ReturnsHtmlContent()
        {
            // Arrange
            var request = new GetThemeSelectionRequest();
            var testThemes = TestConfiguration.GetSampleThemes();
            var testActiveTheme = testThemes[0];
            var testConfig = TestConfiguration.GetSampleBeautifyConfig();

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(testActiveTheme);

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<string>();
            
            var htmlContent = result as string;
            htmlContent.Should().Contain("主题选择");
            htmlContent.Should().Contain("选择您喜欢的主题");
        }

        [Fact]
        public async Task Get_GetThemePreviewRequest_ValidThemeId_ReturnsPreviewData()
        {
            // Arrange
            var themeId = "test-theme";
            var request = new GetThemePreviewRequest { ThemeId = themeId };
            var testThemes = TestConfiguration.GetSampleThemes();
            var testTheme = testThemes.Find(t => t.Id == themeId);
            var testCss = "body { background: #f0f0f0; }";

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            _mockThemeManager
                .Setup(x => x.GenerateThemeCssAsync(testTheme))
                .ReturnsAsync(testCss);

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<GetThemePreviewResponse>();
            
            var response = result as GetThemePreviewResponse;
            response.Theme.Should().NotBeNull();
            response.Theme.Id.Should().Be(themeId);
            response.Css.Should().Be(testCss);
            response.PreviewData.Should().NotBeNull();
            response.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Get_GetThemePreviewRequest_EmptyThemeId_ThrowsArgumentException()
        {
            // Arrange
            var request = new GetThemePreviewRequest { ThemeId = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Get(request));
        }

        [Fact]
        public void Constructor_NullThemeManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ThemeSelectionController(null, _mockConfigurationManager.Object, _mockLogManager.Object));
        }

        [Fact]
        public async Task Get_GetThemeSelectionRequest_LogsDebugMessages()
        {
            // Arrange
            var request = new GetThemeSelectionRequest();
            var testThemes = TestConfiguration.GetSampleThemes();
            var testActiveTheme = testThemes[0];
            var testConfig = TestConfiguration.GetSampleBeautifyConfig();

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(testActiveTheme);

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);

            // Act
            await _controller.Get(request);

            // Assert
            _mockLogger.Verify(
                x => x.Debug("开始获取主题选择界面"),
                Times.Once);

            _mockLogger.Verify(
                x => x.Debug("成功获取主题选择界面，可用主题数量: {0}", testThemes.Count),
                Times.Once);
        }

        [Fact]
        public async Task Get_GetThemeSelectionRequest_DataIsEmbeddedInHtml()
        {
            // Arrange
            var request = new GetThemeSelectionRequest();
            var testThemes = TestConfiguration.GetSampleThemes();
            var testActiveTheme = testThemes[0];
            var testConfig = TestConfiguration.GetSampleBeautifyConfig();

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(testActiveTheme);

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(testConfig);

            // Act
            var result = await _controller.Get(request);

            // Assert
            var htmlContent = result as string;
            
            // 验证主题数据被嵌入到HTML中
            foreach (var theme in testThemes)
            {
                htmlContent.Should().Contain(theme.Id);
                htmlContent.Should().Contain(theme.Name);
            }
            
            // 验证活动主题数据被嵌入
            htmlContent.Should().Contain(testActiveTheme.Id);
            
            // 验证配置数据被嵌入
            htmlContent.Should().Contain(testConfig.ActiveThemeId);
        }

        [Fact]
        public async Task Get_GetThemePreviewRequest_PreviewDataContainsExpectedStructure()
        {
            // Arrange
            var themeId = "test-theme";
            var request = new GetThemePreviewRequest { ThemeId = themeId };
            var testThemes = TestConfiguration.GetSampleThemes();
            var testTheme = testThemes.Find(t => t.Id == themeId);
            var testCss = "body { background: #f0f0f0; }";

            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(testThemes);

            _mockThemeManager
                .Setup(x => x.GenerateThemeCssAsync(testTheme))
                .ReturnsAsync(testCss);

            // Act
            var result = await _controller.Get(request);

            // Assert
            var response = result as GetThemePreviewResponse;
            response.PreviewData.Should().NotBeNull();
            
            // 验证预览数据结构
            var previewDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(response.PreviewData);
            previewDataJson.Should().Contain("ColorScheme");
            previewDataJson.Should().Contain("Typography");
            previewDataJson.Should().Contain("Layout");
            previewDataJson.Should().Contain("SampleElements");
        }
    }
}