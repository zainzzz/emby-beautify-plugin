using EmbyBeautifyPlugin.Controllers;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using FluentAssertions;
using MediaBrowser.Model.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 主题API控制器的单元测试
    /// </summary>
    public class ThemeApiControllerTests
    {
        private readonly Mock<IThemeManager> _mockThemeManager;
        private readonly Mock<ILogManager> _mockLogManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly ThemeApiController _controller;

        public ThemeApiControllerTests()
        {
            _mockThemeManager = new Mock<IThemeManager>();
            _mockLogManager = new Mock<ILogManager>();
            _mockLogger = new Mock<ILogger>();
            
            _mockLogManager.Setup(x => x.GetLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            
            _controller = new ThemeApiController(_mockThemeManager.Object, _mockLogManager.Object);
        }

        [Fact]
        public async Task Get_GetThemesRequest_ReturnsAllThemes()
        {
            // Arrange
            var expectedThemes = TestConfiguration.GetSampleThemes();
            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(expectedThemes);

            var request = new GetThemesRequest();

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as GetThemesResponse;
            response.Should().NotBeNull();
            response.Themes.Should().HaveCount(expectedThemes.Count);
            response.Count.Should().Be(expectedThemes.Count);
            
            _mockThemeManager.Verify(x => x.GetAvailableThemesAsync(), Times.Once);
        }

        [Fact]
        public async Task Get_GetThemeRequest_ValidThemeId_ReturnsTheme()
        {
            // Arrange
            var themes = TestConfiguration.GetSampleThemes();
            var expectedTheme = themes[0];
            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(themes);

            var request = new GetThemeRequest { ThemeId = expectedTheme.Id };

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            var theme = result as Theme;
            theme.Should().NotBeNull();
            theme.Id.Should().Be(expectedTheme.Id);
            theme.Name.Should().Be(expectedTheme.Name);
        }

        [Fact]
        public async Task Get_GetThemeRequest_InvalidThemeId_ThrowsArgumentException()
        {
            // Arrange
            var themes = TestConfiguration.GetSampleThemes();
            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(themes);

            var request = new GetThemeRequest { ThemeId = "non-existent-theme" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.Get(request));
            
            exception.Message.Should().Contain("未找到ID为 'non-existent-theme' 的主题");
        }

        [Fact]
        public async Task Get_GetThemeRequest_EmptyThemeId_ThrowsArgumentException()
        {
            // Arrange
            var request = new GetThemeRequest { ThemeId = "" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.Get(request));
            
            exception.Message.Should().Contain("主题ID不能为空");
        }

        [Fact]
        public async Task Get_GetActiveThemeRequest_ReturnsActiveTheme()
        {
            // Arrange
            var expectedTheme = TestConfiguration.GetSampleTheme();
            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(expectedTheme);

            var request = new GetActiveThemeRequest();

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            var theme = result as Theme;
            theme.Should().NotBeNull();
            theme.Id.Should().Be(expectedTheme.Id);
            theme.Name.Should().Be(expectedTheme.Name);
            
            _mockThemeManager.Verify(x => x.GetActiveThemeAsync(), Times.Once);
        }

        [Fact]
        public async Task Get_GetActiveThemeRequest_NoActiveTheme_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync((Theme)null);

            var request = new GetActiveThemeRequest();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Get(request));
            
            exception.Message.Should().Contain("未找到活动主题");
        }

        [Fact]
        public async Task Post_SetActiveThemeRequest_ValidThemeId_ReturnsSuccessResponse()
        {
            // Arrange
            var themeId = "test-theme";
            var expectedTheme = TestConfiguration.GetSampleTheme();
            expectedTheme.Id = themeId;
            
            _mockThemeManager
                .Setup(x => x.SetActiveThemeAsync(themeId))
                .Returns(Task.CompletedTask);
            
            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(expectedTheme);

            var request = new SetActiveThemeRequest { ThemeId = themeId };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as SetActiveThemeResponse;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Message.Should().Contain("主题已成功切换");
            response.ActiveTheme.Should().NotBeNull();
            response.ActiveTheme.Id.Should().Be(themeId);
            
            _mockThemeManager.Verify(x => x.SetActiveThemeAsync(themeId), Times.Once);
            _mockThemeManager.Verify(x => x.GetActiveThemeAsync(), Times.Once);
        }

        [Fact]
        public async Task Post_SetActiveThemeRequest_EmptyThemeId_ReturnsFailureResponse()
        {
            // Arrange
            var request = new SetActiveThemeRequest { ThemeId = "" };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as SetActiveThemeResponse;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Contain("设置主题失败");
            response.ActiveTheme.Should().BeNull();
        }

        [Fact]
        public async Task Post_SetActiveThemeRequest_ThemeManagerThrows_ReturnsFailureResponse()
        {
            // Arrange
            var themeId = "test-theme";
            _mockThemeManager
                .Setup(x => x.SetActiveThemeAsync(themeId))
                .ThrowsAsync(new InvalidOperationException("Theme not found"));

            var request = new SetActiveThemeRequest { ThemeId = themeId };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as SetActiveThemeResponse;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Contain("设置主题失败");
            response.ActiveTheme.Should().BeNull();
        }

        [Fact]
        public async Task Get_GetThemeCssRequest_ValidThemeId_ReturnsCssResponse()
        {
            // Arrange
            var themeId = "test-theme";
            var theme = TestConfiguration.GetSampleTheme();
            theme.Id = themeId;
            var expectedCss = "body { background-color: #ffffff; }";
            
            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(new List<Theme> { theme });
            
            _mockThemeManager
                .Setup(x => x.GenerateThemeCssAsync(theme))
                .ReturnsAsync(expectedCss);

            var request = new GetThemeCssRequest { ThemeId = themeId };

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as GetThemeCssResponse;
            response.Should().NotBeNull();
            response.ThemeId.Should().Be(themeId);
            response.Css.Should().Be(expectedCss);
            response.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            _mockThemeManager.Verify(x => x.GetAvailableThemesAsync(), Times.Once);
            _mockThemeManager.Verify(x => x.GenerateThemeCssAsync(theme), Times.Once);
        }

        [Fact]
        public async Task Get_GetThemeCssRequest_InvalidThemeId_ThrowsArgumentException()
        {
            // Arrange
            var themes = TestConfiguration.GetSampleThemes();
            _mockThemeManager
                .Setup(x => x.GetAvailableThemesAsync())
                .ReturnsAsync(themes);

            var request = new GetThemeCssRequest { ThemeId = "non-existent-theme" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.Get(request));
            
            exception.Message.Should().Contain("未找到ID为 'non-existent-theme' 的主题");
        }

        [Fact]
        public async Task Get_GetThemeCssRequest_EmptyThemeId_ThrowsArgumentException()
        {
            // Arrange
            var request = new GetThemeCssRequest { ThemeId = "" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.Get(request));
            
            exception.Message.Should().Contain("主题ID不能为空");
        }

        [Fact]
        public async Task Constructor_NullThemeManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ThemeApiController(null, _mockLogManager.Object));
            
            exception.ParamName.Should().Be("themeManager");
        }

        [Fact]
        public async Task Constructor_NullLogManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ThemeApiController(_mockThemeManager.Object, null));
            
            exception.ParamName.Should().Be("logManager");
        }
    }
}