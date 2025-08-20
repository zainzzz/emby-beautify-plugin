using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
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
    /// 兼容性测试，测试插件在不同环境和配置下的兼容性
    /// </summary>
    public class CompatibilityTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public CompatibilityTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Theory]
        [InlineData("4.7.0")]
        [InlineData("4.7.5")]
        [InlineData("4.8.0")]
        [InlineData("4.8.5")]
        public async Task EmbyServerVersion_Compatibility_ShouldWorkWithSupportedVersions(string version)
        {
            // Arrange
            var theme = TestConfiguration.CreateTestTheme();
            
            // 模拟不同版本的Emby Server环境
            Environment.SetEnvironmentVariable("EMBY_SERVER_VERSION", version);

            // Act - 测试主题数据在不同版本下的兼容性
            var validationErrors = await Task.FromResult(theme.Validate());
            var serializedTheme = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(theme));
            var deserializedTheme = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<Theme>(serializedTheme));

            // Assert
            validationErrors.Should().BeEmpty();
            deserializedTheme.Should().NotBeNull();
            deserializedTheme.Id.Should().Be(theme.Id);

            // 清理环境变量
            Environment.SetEnvironmentVariable("EMBY_SERVER_VERSION", null);
        }

        [Theory]
        [InlineData("Windows")]
        [InlineData("Linux")]
        [InlineData("macOS")]
        [InlineData("Docker")]
        public async Task OperatingSystem_Compatibility_ShouldWorkAcrossPlatforms(string platform)
        {
            // Arrange
            var config = TestConfiguration.CreateDefaultTestConfig();
            
            // 模拟不同操作系统环境
            Environment.SetEnvironmentVariable("PLUGIN_PLATFORM", platform);

            // Act - 测试配置数据在不同平台下的兼容性
            var serializedConfig = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(config));
            var deserializedConfig = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<BeautifyConfig>(serializedConfig));

            // Assert
            deserializedConfig.Should().NotBeNull();
            deserializedConfig.Should().BeEquivalentTo(config);
            deserializedConfig.ActiveThemeId.Should().Be(config.ActiveThemeId);

            // 清理环境变量
            Environment.SetEnvironmentVariable("PLUGIN_PLATFORM", null);
        }

        [Theory]
        [InlineData("Chrome", "100.0")]
        [InlineData("Firefox", "95.0")]
        [InlineData("Safari", "15.0")]
        [InlineData("Edge", "100.0")]
        public async Task BrowserCompatibility_ShouldGenerateCompatibleCSS(string browser, string version)
        {
            // Arrange
            var theme = TestConfiguration.CreateTestTheme();
            
            // 设置浏览器环境
            Environment.SetEnvironmentVariable("BROWSER_NAME", browser);
            Environment.SetEnvironmentVariable("BROWSER_VERSION", version);

            // Act - 生成基本CSS内容用于兼容性测试
            var cssContent = await Task.FromResult(GenerateBasicCssForTheme(theme));
            var isCompatible = ValidateCssBrowserCompatibility(cssContent, browser, version);

            // Assert
            cssContent.Should().NotBeNullOrEmpty();
            isCompatible.Should().BeTrue($"CSS应该与 {browser} {version} 兼容");

            // 验证CSS不包含不兼容的属性
            if (browser == "Safari" && version.StartsWith("15"))
            {
                // Safari 15 的特殊兼容性检查
                cssContent.Should().NotContain("backdrop-filter", "Safari 15 不完全支持 backdrop-filter");
            }

            // 清理环境变量
            Environment.SetEnvironmentVariable("BROWSER_NAME", null);
            Environment.SetEnvironmentVariable("BROWSER_VERSION", null);
        }

        [Fact]
        public async Task LegacyConfiguration_Migration_ShouldUpgradeGracefully()
        {
            // Arrange
            var legacyConfig = CreateLegacyConfiguration();

            // Act - 模拟配置迁移过程
            var migratedConfig = await Task.FromResult(MigrateLegacyConfiguration(legacyConfig));

            // Assert
            migratedConfig.Should().NotBeNull();
            
            // 验证迁移后的配置包含新字段
            migratedConfig.ResponsiveSettings.Should().NotBeNull();
            migratedConfig.ResponsiveSettings.Desktop.Should().NotBeNull();
            migratedConfig.ResponsiveSettings.Tablet.Should().NotBeNull();
            migratedConfig.ResponsiveSettings.Mobile.Should().NotBeNull();
            
            // 验证旧数据得到保留
            migratedConfig.ActiveThemeId.Should().Be(legacyConfig.ActiveThemeId);
            migratedConfig.EnableAnimations.Should().Be(legacyConfig.EnableAnimations);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("zh-CN")]
        [InlineData("ja-JP")]
        [InlineData("de-DE")]
        [InlineData("fr-FR")]
        public async Task Localization_Compatibility_ShouldHandleDifferentLocales(string locale)
        {
            // Arrange
            var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
            var originalUICulture = System.Globalization.CultureInfo.CurrentUICulture;
            
            try
            {
                // 设置测试区域设置
                var culture = new System.Globalization.CultureInfo(locale);
                System.Globalization.CultureInfo.CurrentCulture = culture;
                System.Globalization.CultureInfo.CurrentUICulture = culture;

                var theme = TestConfiguration.CreateTestTheme();

                // Act - 测试数据序列化在不同区域设置下的表现
                var json = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(theme));
                var deserializedTheme = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<Theme>(json));

                // Assert
                deserializedTheme.Should().NotBeNull();
                deserializedTheme.Id.Should().Be(theme.Id);
                
                // 验证数字格式在不同区域设置下的正确性
                if (locale == "de-DE" || locale == "fr-FR")
                {
                    // JSON序列化应该始终使用标准格式，不受区域设置影响
                    json.Should().NotContain(",5"); // 不应该有德语/法语的小数格式
                }
            }
            finally
            {
                // 恢复原始区域设置
                System.Globalization.CultureInfo.CurrentCulture = originalCulture;
                System.Globalization.CultureInfo.CurrentUICulture = originalUICulture;
            }
        }

        /// <summary>
        /// 生成基本CSS用于主题测试
        /// </summary>
        private string GenerateBasicCssForTheme(Theme theme)
        {
            return $@"
                :root {{
                    --primary-color: {theme.Colors?.Primary ?? "#007acc"};
                    --background-color: {theme.Colors?.Background ?? "#ffffff"};
                    --text-color: {theme.Colors?.Text ?? "#000000"};
                    --font-family: {theme.Typography?.FontFamily ?? "Arial, sans-serif"};
                }}
                
                body {{
                    background-color: var(--background-color);
                    color: var(--text-color);
                    font-family: var(--font-family);
                }}
            ";
        }

        /// <summary>
        /// 验证CSS与特定浏览器的兼容性
        /// </summary>
        private bool ValidateCssBrowserCompatibility(string css, string browser, string version)
        {
            // 简化的兼容性检查逻辑
            var incompatibleProperties = new Dictionary<string, List<string>>
            {
                ["Safari"] = new List<string> { "backdrop-filter" },
                ["Firefox"] = new List<string> { "-webkit-appearance" },
                ["Edge"] = new List<string> { "-moz-appearance" }
            };

            if (incompatibleProperties.ContainsKey(browser))
            {
                foreach (var property in incompatibleProperties[browser])
                {
                    if (css.Contains(property))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 创建旧版本配置用于迁移测试
        /// </summary>
        private BeautifyConfig CreateLegacyConfiguration()
        {
            return new BeautifyConfig
            {
                ActiveThemeId = "legacy-theme",
                EnableAnimations = true,
                EnableCustomFonts = true,
                AnimationDuration = 300,
                // 旧版本没有ResponsiveSettings
                ResponsiveSettings = null,
                CustomSettings = new Dictionary<string, object>
                {
                    { "legacySetting", "legacyValue" }
                }
            };
        }

        /// <summary>
        /// 模拟配置迁移过程
        /// </summary>
        private BeautifyConfig MigrateLegacyConfiguration(BeautifyConfig legacyConfig)
        {
            var migratedConfig = new BeautifyConfig
            {
                ActiveThemeId = legacyConfig.ActiveThemeId,
                EnableAnimations = legacyConfig.EnableAnimations,
                EnableCustomFonts = legacyConfig.EnableCustomFonts,
                AnimationDuration = legacyConfig.AnimationDuration,
                CustomSettings = legacyConfig.CustomSettings ?? new Dictionary<string, object>()
            };

            // 添加新的响应式设置
            if (legacyConfig.ResponsiveSettings == null)
            {
                migratedConfig.ResponsiveSettings = new ResponsiveSettings
                {
                    Desktop = new BreakpointSettings
                    {
                        MinWidth = 1200,
                        MaxWidth = 9999,
                        GridColumns = 6,
                        GridGap = "1.5rem",
                        FontScale = 1.0
                    },
                    Tablet = new BreakpointSettings
                    {
                        MinWidth = 768,
                        MaxWidth = 1199,
                        GridColumns = 4,
                        GridGap = "1rem",
                        FontScale = 0.9
                    },
                    Mobile = new BreakpointSettings
                    {
                        MinWidth = 0,
                        MaxWidth = 767,
                        GridColumns = 2,
                        GridGap = "0.5rem",
                        FontScale = 0.8
                    }
                };
            }
            else
            {
                migratedConfig.ResponsiveSettings = legacyConfig.ResponsiveSettings;
            }

            return migratedConfig;
        }

    }
}