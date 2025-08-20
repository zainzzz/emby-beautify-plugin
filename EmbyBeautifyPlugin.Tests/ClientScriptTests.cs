using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using EmbyBeautifyPlugin.Interfaces;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 客户端脚本测试类
    /// 测试客户端样式注入脚本的功能和兼容性
    /// </summary>
    public class ClientScriptTests
    {
        private readonly Mock<ILogger<StyleInjector>> _mockLogger;
        private readonly Mock<IThemeManager> _mockThemeManager;
        private readonly Mock<IConfigurationManager> _mockConfigManager;
        private readonly StyleInjector _styleInjector;

        public ClientScriptTests()
        {
            _mockLogger = new Mock<ILogger<StyleInjector>>();
            _mockThemeManager = new Mock<IThemeManager>();
            _mockConfigManager = new Mock<IConfigurationManager>();
            
            var mockErrorHandlingService = new Mock<ErrorHandlingService>(
                Mock.Of<ILogger<ErrorHandlingService>>());
            var mockCacheService = new Mock<StyleCacheService>(
                Mock.Of<ILogger<StyleCacheService>>(),
                _mockConfigManager.Object);
            
            _styleInjector = new StyleInjector(
                _mockLogger.Object,
                _mockThemeManager.Object,
                _mockConfigManager.Object,
                mockErrorHandlingService.Object,
                mockCacheService.Object
            );
        }

        [Fact]
        public async Task StyleInjectorScript_ShouldExist()
        {
            // Arrange
            var baseDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(ClientScriptTests).Assembly.Location))));
            var scriptPath = Path.Combine(baseDir, "EmbyBeautifyPlugin", "Views", "js", "style-injector.js");
            
            // Act & Assert
            File.Exists(scriptPath).Should().BeTrue($"样式注入器脚本文件应该存在于: {scriptPath}");
        }

        [Fact]
        public async Task StyleInjectorScript_ShouldContainRequiredFunctions()
        {
            // Arrange
            var baseDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(ClientScriptTests).Assembly.Location))));
            var scriptPath = Path.Combine(baseDir, "EmbyBeautifyPlugin", "Views", "js", "style-injector.js");
            var scriptContent = await File.ReadAllTextAsync(scriptPath);
            
            // Act & Assert
            scriptContent.Should().Contain("EmbyBeautifyStyleInjector", "脚本应包含主要命名空间");
            scriptContent.Should().Contain("init:", "脚本应包含初始化函数");
            scriptContent.Should().Contain("injectStyle:", "脚本应包含样式注入函数");
            scriptContent.Should().Contain("updateStyles:", "脚本应包含样式更新函数");
            scriptContent.Should().Contain("detectBrowserCompatibility:", "脚本应包含兼容性检测函数");
        }

        [Fact]
        public async Task BrowserCompatibilityScript_ShouldExist()
        {
            // Arrange
            var baseDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(ClientScriptTests).Assembly.Location))));
            var scriptPath = Path.Combine(baseDir, "EmbyBeautifyPlugin", "Views", "js", "browser-compatibility.js");
            
            // Act & Assert
            File.Exists(scriptPath).Should().BeTrue($"浏览器兼容性脚本文件应该存在于: {scriptPath}");
        }

        [Fact]
        public async Task BrowserCompatibilityScript_ShouldContainRequiredFunctions()
        {
            // Arrange
            var baseDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(ClientScriptTests).Assembly.Location))));
            var scriptPath = Path.Combine(baseDir, "EmbyBeautifyPlugin", "Views", "js", "browser-compatibility.js");
            var scriptContent = await File.ReadAllTextAsync(scriptPath);
            
            // Act & Assert
            scriptContent.Should().Contain("EmbyBeautifyCompatibility", "脚本应包含兼容性命名空间");
            scriptContent.Should().Contain("detectBrowser:", "脚本应包含浏览器检测函数");
            scriptContent.Should().Contain("detectFeatures:", "脚本应包含特性检测函数");
            scriptContent.Should().Contain("applyPolyfills:", "脚本应包含polyfill应用函数");
        }

        [Fact]
        public async Task GenerateClientScript_WithTheme_ShouldReturnValidScript()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var config = TestConfiguration.GetSampleBeautifyConfig();
            
            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync(theme);
                
            _mockConfigManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);

            // Act
            var script = await _styleInjector.GenerateClientScriptAsync();

            // Assert
            script.Should().NotBeNullOrEmpty("生成的客户端脚本不应为空");
            script.Should().Contain("EmbyBeautifyStyleInjector", "脚本应包含样式注入器");
            script.Should().Contain(theme.Name, "脚本应包含主题信息");
        }

        [Fact]
        public async Task GenerateClientScript_WithoutTheme_ShouldReturnDefaultScript()
        {
            // Arrange
            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ReturnsAsync((Theme)null);
                
            _mockConfigManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(TestConfiguration.GetSampleBeautifyConfig());

            // Act
            var script = await _styleInjector.GenerateClientScriptAsync();

            // Assert
            script.Should().NotBeNullOrEmpty("即使没有主题，也应生成默认脚本");
            script.Should().Contain("EmbyBeautifyStyleInjector", "脚本应包含样式注入器");
        }

        [Theory]
        [InlineData("Chrome", 80, true)]
        [InlineData("Firefox", 70, true)]
        [InlineData("Safari", 12, true)]
        [InlineData("Edge", 18, true)]
        [InlineData("IE", 11, false)]
        [InlineData("IE", 9, false)]
        public async Task BrowserCompatibility_ShouldDetectModernBrowser(string browserName, int version, bool expectedModern)
        {
            // Arrange
            var userAgent = GenerateUserAgent(browserName, version);
            
            // Act
            var isModern = IsModernBrowser(browserName, version);
            
            // Assert
            isModern.Should().Be(expectedModern, $"{browserName} {version} 的现代浏览器检测应该返回 {expectedModern}");
        }

        [Fact]
        public async Task StyleInjection_ShouldHandleCustomProperties()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            theme.Colors = new ThemeColors
            {
                Primary = "#007bff",
                Secondary = "#6c757d",
                Background = "#ffffff",
                Surface = "#f8f9fa",
                Text = "#333333",
                Accent = "#17a2b8"
            };

            // Act
            var css = await _styleInjector.GenerateThemeCssAsync(theme);

            // Assert
            css.Should().Contain("--emby-beautify-primary", "CSS应包含主色调自定义属性");
            css.Should().Contain("--emby-beautify-secondary", "CSS应包含次要色调自定义属性");
            css.Should().Contain("--emby-beautify-background", "CSS应包含背景色自定义属性");
            css.Should().Contain("#007bff", "CSS应包含具体的颜色值");
        }

        [Fact]
        public async Task StyleInjection_ShouldGenerateResponsiveCSS()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var config = TestConfiguration.GetSampleBeautifyConfig();
            config.ResponsiveSettings = new ResponsiveSettings
            {
                Desktop = new BreakpointSettings { MaxWidth = 1920 },
                Tablet = new BreakpointSettings { MaxWidth = 1024 },
                Mobile = new BreakpointSettings { MaxWidth = 768 }
            };

            _mockConfigManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);

            // Act
            var css = await _styleInjector.GenerateResponsiveCssAsync(theme);

            // Assert
            css.Should().Contain("@media", "CSS应包含媒体查询");
            css.Should().Contain("max-width: 768px", "CSS应包含移动端断点");
            css.Should().Contain("max-width: 1024px", "CSS应包含平板端断点");
        }

        [Fact]
        public async Task StyleInjection_ShouldHandleAnimations()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            config.EnableAnimations = true;
            config.AnimationDuration = 300;

            _mockConfigManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);

            // Act
            var css = await _styleInjector.GenerateAnimationCssAsync();

            // Assert
            css.Should().Contain("@keyframes", "CSS应包含关键帧动画");
            css.Should().Contain("transition:", "CSS应包含过渡效果");
            css.Should().Contain("0.3s", "CSS应包含指定的动画时长");
        }

        [Fact]
        public async Task StyleInjection_ShouldHandleDisabledAnimations()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            config.EnableAnimations = false;

            _mockConfigManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);

            // Act
            var css = await _styleInjector.GenerateAnimationCssAsync();

            // Assert
            css.Should().Contain("transition: none", "禁用动画时CSS应包含transition: none");
            css.Should().Contain("animation: none", "禁用动画时CSS应包含animation: none");
        }

        [Fact]
        public async Task ClientScript_ShouldHandleErrors_Gracefully()
        {
            // Arrange
            _mockThemeManager
                .Setup(x => x.GetActiveThemeAsync())
                .ThrowsAsync(new Exception("主题加载失败"));

            // Act
            var script = await _styleInjector.GenerateClientScriptAsync();

            // Assert
            script.Should().NotBeNullOrEmpty("即使出现错误，也应生成基础脚本");
            script.Should().Contain("EmbyBeautifyStyleInjector", "脚本应包含样式注入器");
        }

        [Fact]
        public async Task ClientScript_ShouldIncludeDebugMode()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            config.CustomSettings = new Dictionary<string, object>
            {
                { "DebugMode", true }
            };

            _mockConfigManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(config);

            // Act
            var script = await _styleInjector.GenerateClientScriptAsync();

            // Assert
            script.Should().Contain("config.debugMode = true", "调试模式开启时脚本应包含调试标志");
        }

        [Fact]
        public async Task BrowserCompatibility_ShouldProvidePolyfills()
        {
            // Arrange
            var baseDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(ClientScriptTests).Assembly.Location))));
            var scriptPath = Path.Combine(baseDir, "EmbyBeautifyPlugin", "Views", "js", "browser-compatibility.js");
            var scriptContent = await File.ReadAllTextAsync(scriptPath);

            // Act & Assert
            scriptContent.Should().Contain("createClassListPolyfill", "应包含classList polyfill");
            scriptContent.Should().Contain("createEventListenerPolyfill", "应包含addEventListener polyfill");
            scriptContent.Should().Contain("createPromisePolyfill", "应包含Promise polyfill");
            scriptContent.Should().Contain("createQuerySelectorPolyfill", "应包含querySelector polyfill");
        }

        [Fact]
        public async Task StyleInjection_ShouldOptimizeCSS()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var rawCSS = @"
                .test {
                    color: red;
                    background: blue;
                }
                
                .test {
                    margin: 10px;
                }
            ";

            // Act
            var optimizedCSS = await _styleInjector.OptimizeCssAsync(rawCSS);

            // Assert
            optimizedCSS.Should().NotContain("\n\n", "优化后的CSS不应包含多余的空行");
            optimizedCSS.Length.Should().BeLessThan(rawCSS.Length, "优化后的CSS应该更短");
        }

        [Theory]
        [InlineData("desktop", 1920)]
        [InlineData("tablet", 1024)]
        [InlineData("mobile", 768)]
        public async Task ResponsiveCSS_ShouldGenerateCorrectBreakpoints(string deviceType, int maxWidth)
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            var breakpointSettings = new BreakpointSettings { MaxWidth = maxWidth };

            // Act
            var css = await _styleInjector.GenerateBreakpointCssAsync(deviceType, breakpointSettings);

            // Assert
            css.Should().Contain($"max-width: {maxWidth}px", $"{deviceType}断点应包含正确的最大宽度");
        }

        [Fact]
        public async Task ClientScript_ShouldHandleMutationObserver()
        {
            // Arrange
            var baseDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(typeof(ClientScriptTests).Assembly.Location))));
            var scriptPath = Path.Combine(baseDir, "EmbyBeautifyPlugin", "Views", "js", "style-injector.js");
            var scriptContent = await File.ReadAllTextAsync(scriptPath);

            // Act & Assert
            scriptContent.Should().Contain("MutationObserver", "脚本应支持MutationObserver");
            scriptContent.Should().Contain("observe(document.body", "应观察document.body的变化");
            scriptContent.Should().Contain("childList: true", "应监听子节点变化");
            scriptContent.Should().Contain("subtree: true", "应监听子树变化");
        }

        [Fact]
        public async Task StyleInjection_ShouldSupportCaching()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            
            // Act
            var css1 = await _styleInjector.GenerateThemeCssAsync(theme);
            var css2 = await _styleInjector.GenerateThemeCssAsync(theme);

            // Assert
            css1.Should().Be(css2, "相同主题应返回相同的CSS（可能来自缓存）");
        }

        /// <summary>
        /// 生成用户代理字符串（测试辅助方法）
        /// </summary>
        private string GenerateUserAgent(string browserName, int version)
        {
            return browserName.ToLower() switch
            {
                "chrome" => $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{version}.0.0.0 Safari/537.36",
                "firefox" => $"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:{version}.0) Gecko/20100101 Firefox/{version}.0",
                "safari" => $"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/{version}.0 Safari/605.1.15",
                "edge" => $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/{version}.0.0.0",
                "ie" => $"Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:{version}.0) like Gecko",
                _ => "Mozilla/5.0 (Unknown)"
            };
        }

        /// <summary>
        /// 判断是否为现代浏览器（测试辅助方法）
        /// </summary>
        private bool IsModernBrowser(string browserName, int version)
        {
            return browserName.ToLower() switch
            {
                "chrome" => version >= 60,
                "firefox" => version >= 55,
                "safari" => version >= 11,
                "edge" => version >= 16,
                "ie" => false,
                _ => false
            };
        }
    }
}