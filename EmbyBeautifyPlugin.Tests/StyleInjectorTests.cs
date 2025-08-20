using EmbyBeautifyPlugin.Exceptions;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 样式注入器的单元测试
    /// </summary>
    public class StyleInjectorTests
    {
        private readonly Mock<ILogger<StyleInjector>> _mockLogger;
        private readonly Mock<IThemeManager> _mockThemeManager;
        private readonly Mock<IConfigurationManager> _mockConfigurationManager;
        private readonly Mock<ErrorHandlingService> _mockErrorHandlingService;
        private readonly StyleInjector _styleInjector;

        public StyleInjectorTests()
        {
            _mockLogger = new Mock<ILogger<StyleInjector>>();
            _mockThemeManager = new Mock<IThemeManager>();
            _mockConfigurationManager = new Mock<IConfigurationManager>();
            _mockErrorHandlingService = new Mock<ErrorHandlingService>(Mock.Of<ILogger<ErrorHandlingService>>());
            var mockCacheService = new Mock<StyleCacheService>(
                Mock.Of<ILogger<StyleCacheService>>(),
                _mockConfigurationManager.Object);
            
            _styleInjector = new StyleInjector(
                _mockLogger.Object,
                _mockThemeManager.Object,
                _mockConfigurationManager.Object,
                _mockErrorHandlingService.Object,
                mockCacheService.Object);
        }

        [Fact]
        public async Task InjectStylesAsync_ValidCss_InjectsSuccessfully()
        {
            // Arrange
            var css = ".test { color: red; }";

            // Act
            await _styleInjector.InjectStylesAsync(css);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().HaveCount(1);
            injectedStyles[0].ContentLength.Should().Be(css.Length);
        }

        [Fact]
        public async Task InjectStylesAsync_EmptyCss_DoesNotInject()
        {
            // Arrange
            var css = "";

            // Act
            await _styleInjector.InjectStylesAsync(css);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().BeEmpty();
        }

        [Fact]
        public async Task InjectStylesAsync_NullCss_DoesNotInject()
        {
            // Arrange
            string css = null;

            // Act
            await _styleInjector.InjectStylesAsync(css);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().BeEmpty();
        }

        [Fact]
        public async Task InjectStylesAsync_WithStyleIdAndPriority_InjectsWithCorrectProperties()
        {
            // Arrange
            var css = ".test { color: blue; }";
            var styleId = "test-style";
            var priority = 500;

            // Act
            await _styleInjector.InjectStylesAsync(css, styleId, priority);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().HaveCount(1);
            injectedStyles[0].Id.Should().Be(styleId);
            injectedStyles[0].Priority.Should().Be(priority);
        }

        [Fact]
        public async Task InjectStylesAsync_DangerousCss_SanitizesContent()
        {
            // Arrange
            var dangerousCss = @"
                .test { color: red; }
                .evil { background: url('javascript:alert(1)'); }
                .script { content: '<script>alert(1)</script>'; }
            ";

            // Act
            await _styleInjector.InjectStylesAsync(dangerousCss);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().HaveCount(1);
            // 内容应该被清理，长度会减少
            injectedStyles[0].ContentLength.Should().BeLessThan(dangerousCss.Length);
        }

        [Fact]
        public async Task RemoveStylesAsync_ExistingStyle_RemovesSuccessfully()
        {
            // Arrange
            var css = ".test { color: green; }";
            var styleId = "removable-style";
            await _styleInjector.InjectStylesAsync(css, styleId);

            // Act
            await _styleInjector.RemoveStylesAsync(styleId);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveStylesAsync_NonExistentStyle_DoesNotThrow()
        {
            // Arrange
            var styleId = "non-existent-style";

            // Act & Assert
            await _styleInjector.Invoking(s => s.RemoveStylesAsync(styleId))
                .Should().NotThrowAsync();
        }

        [Fact]
        public async Task RemoveStylesAsync_EmptyStyleId_DoesNotThrow()
        {
            // Arrange
            var styleId = "";

            // Act & Assert
            await _styleInjector.Invoking(s => s.RemoveStylesAsync(styleId))
                .Should().NotThrowAsync();
        }

        [Fact]
        public async Task UpdateGlobalStylesAsync_WithActiveTheme_InjectsThemeStyles()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            var theme = TestConfiguration.GetSampleTheme();
            var themeCss = ".theme { background: var(--primary-color); }";

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);

            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(theme);

            _mockThemeManager
                .Setup(x => x.GenerateThemeCssAsync(theme))
                .ReturnsAsync(themeCss);

            // Act
            await _styleInjector.UpdateGlobalStylesAsync();

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().NotBeEmpty();
            
            // 应该包含主题样式和系统样式
            injectedStyles.Should().Contain(s => s.Id == "global_theme");
            injectedStyles.Should().Contain(s => s.Id == "system_styles");
        }

        [Fact]
        public async Task UpdateGlobalStylesAsync_WithAnimationsEnabled_InjectsAnimationStyles()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            config.EnableAnimations = true;
            config.AnimationDuration = 300;
            
            var theme = TestConfiguration.GetSampleTheme();
            var themeCss = ".theme { background: var(--primary-color); }";

            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);

            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(theme);

            _mockThemeManager
                .Setup(x => x.GenerateThemeCssAsync(theme))
                .ReturnsAsync(themeCss);

            // Act
            await _styleInjector.UpdateGlobalStylesAsync();

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().Contain(s => s.Id == "animation_styles");
        }

        [Fact]
        public async Task UpdateGlobalStylesAsync_NoActiveTheme_LogsWarning()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            
            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);

            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync((Theme)null);

            // Act
            await _styleInjector.UpdateGlobalStylesAsync();

            // Assert
            // 验证记录了警告日志
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("没有活动主题")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetInjectedStylesAsync_MultipleStyles_ReturnsOrderedByPriority()
        {
            // Arrange
            await _styleInjector.InjectStylesAsync(".low { color: red; }", "low-priority", 100);
            await _styleInjector.InjectStylesAsync(".high { color: blue; }", "high-priority", 900);
            await _styleInjector.InjectStylesAsync(".medium { color: green; }", "medium-priority", 500);

            // Act
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();

            // Assert
            injectedStyles.Should().HaveCount(3);
            injectedStyles[0].Id.Should().Be("high-priority");
            injectedStyles[1].Id.Should().Be("medium-priority");
            injectedStyles[2].Id.Should().Be("low-priority");
        }

        [Fact]
        public async Task CleanupStylesAsync_ExpiredStyles_RemovesOldStyles()
        {
            // Arrange
            await _styleInjector.InjectStylesAsync(".test1 { color: red; }", "style1");
            await _styleInjector.InjectStylesAsync(".test2 { color: blue; }", "style2");
            
            // 设置一个很短的过期时间
            var maxAge = TimeSpan.FromMilliseconds(1);
            await Task.Delay(10); // 确保样式过期

            // Act
            await _styleInjector.CleanupStylesAsync(maxAge);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().BeEmpty();
        }

        [Fact]
        public async Task CleanupStylesAsync_RecentStyles_KeepsStyles()
        {
            // Arrange
            await _styleInjector.InjectStylesAsync(".test1 { color: red; }", "style1");
            await _styleInjector.InjectStylesAsync(".test2 { color: blue; }", "style2");
            
            // 设置一个很长的过期时间
            var maxAge = TimeSpan.FromHours(1);

            // Act
            await _styleInjector.CleanupStylesAsync(maxAge);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().HaveCount(2);
        }

        [Theory]
        [InlineData(".test { color: red; }", true)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData(null, false)]
        public async Task InjectStylesAsync_VariousInputs_HandlesCorrectly(string css, bool shouldInject)
        {
            // Act
            await _styleInjector.InjectStylesAsync(css);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            if (shouldInject)
            {
                injectedStyles.Should().HaveCount(1);
            }
            else
            {
                injectedStyles.Should().BeEmpty();
            }
        }

        [Fact]
        public void Constructor_NullDependencies_ThrowsArgumentNullException()
        {
            // Act & Assert
            var mockCacheService = new Mock<StyleCacheService>(
                Mock.Of<ILogger<StyleCacheService>>(),
                _mockConfigurationManager.Object);

            Assert.Throws<ArgumentNullException>(() => new StyleInjector(
                null, _mockThemeManager.Object, _mockConfigurationManager.Object, _mockErrorHandlingService.Object, mockCacheService.Object));
            
            Assert.Throws<ArgumentNullException>(() => new StyleInjector(
                _mockLogger.Object, null, _mockConfigurationManager.Object, _mockErrorHandlingService.Object, mockCacheService.Object));
            
            Assert.Throws<ArgumentNullException>(() => new StyleInjector(
                _mockLogger.Object, _mockThemeManager.Object, null, _mockErrorHandlingService.Object, mockCacheService.Object));
            
            Assert.Throws<ArgumentNullException>(() => new StyleInjector(
                _mockLogger.Object, _mockThemeManager.Object, _mockConfigurationManager.Object, null, mockCacheService.Object));
            
            Assert.Throws<ArgumentNullException>(() => new StyleInjector(
                _mockLogger.Object, _mockThemeManager.Object, _mockConfigurationManager.Object, _mockErrorHandlingService.Object, null));
        }

        [Fact]
        public async Task InjectStylesAsync_ConcurrentCalls_HandlesCorrectly()
        {
            // Arrange
            var tasks = new List<Task>();
            
            // Act - 并发注入多个样式
            for (int i = 0; i < 10; i++)
            {
                var css = $".test{i} {{ color: red; }}";
                var styleId = $"concurrent-style-{i}";
                tasks.Add(_styleInjector.InjectStylesAsync(css, styleId));
            }
            
            await Task.WhenAll(tasks);

            // Assert
            var injectedStyles = await _styleInjector.GetInjectedStylesAsync();
            injectedStyles.Should().HaveCount(10);
        }

        [Fact]
        public void Dispose_CallsDispose_DoesNotThrow()
        {
            // Act & Assert
            _styleInjector.Invoking(s => s.Dispose())
                .Should().NotThrow();
        }
    }
}