using EmbyBeautifyPlugin.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 端到端功能测试，测试完整的用户场景和工作流程
    /// </summary>
    public class EndToEndFunctionalTests
    {
        [Fact]
        public async Task ThemeDataModel_CompleteWorkflow_ShouldMaintainIntegrity()
        {
            // Arrange - 创建测试主题和配置
            var theme = TestConfiguration.CreateTestTheme();
            var config = TestConfiguration.CreateDefaultTestConfig();
            
            // Act - 模拟完整的数据流程
            
            // 1. 验证主题数据完整性
            var validationErrors = theme.Validate();
            
            // 2. 序列化和反序列化主题
            var serializedTheme = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(theme));
            var deserializedTheme = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<Theme>(serializedTheme));
            
            // 3. 配置更新流程
            config.ActiveThemeId = theme.Id;
            var serializedConfig = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(config));
            var deserializedConfig = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<BeautifyConfig>(serializedConfig));

            // Assert - 验证数据完整性
            validationErrors.Should().BeEmpty();
            deserializedTheme.Should().BeEquivalentTo(theme);
            deserializedConfig.Should().BeEquivalentTo(config);
            deserializedConfig.ActiveThemeId.Should().Be(theme.Id);
        }

        [Fact]
        public async Task MultipleThemeConfiguration_DataConsistency_ShouldMaintainIntegrity()
        {
            // Arrange
            var themes = TestConfiguration.CreateTestThemeList();
            var config = TestConfiguration.CreateDefaultTestConfig();

            // Act - 模拟用户多次切换主题的配置变更
            var configurationHistory = new List<BeautifyConfig>();
            
            foreach (var theme in themes)
            {
                // 创建新的配置副本
                var newConfig = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<BeautifyConfig>(
                    Newtonsoft.Json.JsonConvert.SerializeObject(config)));
                
                newConfig.ActiveThemeId = theme.Id;
                configurationHistory.Add(newConfig);
            }

            // Assert
            configurationHistory.Should().HaveCount(themes.Count);
            
            for (int i = 0; i < themes.Count; i++)
            {
                configurationHistory[i].ActiveThemeId.Should().Be(themes[i].Id);
                configurationHistory[i].EnableAnimations.Should().Be(config.EnableAnimations);
                configurationHistory[i].ResponsiveSettings.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ThemeValidation_WithInvalidData_ShouldDetectErrors()
        {
            // Arrange
            var invalidTheme = new Theme
            {
                Id = "", // 无效：空ID
                Name = "Test Theme",
                Colors = null, // 无效：空颜色
                Typography = new ThemeTypography
                {
                    FontSize = "invalid-size" // 无效：非法字体大小
                }
            };

            // Act
            var validationErrors = await Task.FromResult(invalidTheme.Validate());

            // Assert
            validationErrors.Should().NotBeEmpty();
            validationErrors.Should().Contain(error => error.Contains("Id"));
            validationErrors.Should().Contain(error => error.Contains("Colors"));
        }

        [Fact]
        public async Task ResponsiveConfiguration_WithDifferentBreakpoints_ShouldHaveCorrectSettings()
        {
            // Arrange
            var config = TestConfiguration.CreateDefaultTestConfig();

            // Act - 验证响应式配置的完整性
            var desktopSettings = config.ResponsiveSettings.Desktop;
            var tabletSettings = config.ResponsiveSettings.Tablet;
            var mobileSettings = config.ResponsiveSettings.Mobile;

            // Assert
            desktopSettings.Should().NotBeNull();
            tabletSettings.Should().NotBeNull();
            mobileSettings.Should().NotBeNull();

            // 验证断点设置的逻辑性
            desktopSettings.MinWidth.Should().BeGreaterThan(tabletSettings.MaxWidth);
            tabletSettings.MinWidth.Should().BeGreaterThan(mobileSettings.MaxWidth);

            // 验证网格列数的递减
            desktopSettings.GridColumns.Should().BeGreaterThan(tabletSettings.GridColumns);
            tabletSettings.GridColumns.Should().BeGreaterThan(mobileSettings.GridColumns);

            await Task.CompletedTask; // 保持异步签名
        }

        [Fact]
        public async Task ConfigurationSerialization_WithComplexData_ShouldMaintainIntegrity()
        {
            // Arrange
            var originalConfig = TestConfiguration.CreateDefaultTestConfig();
            originalConfig.CustomSettings["complexObject"] = new { Name = "Test", Value = 42 };

            // Act - 测试配置序列化和反序列化
            var serialized = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(originalConfig, Newtonsoft.Json.Formatting.Indented));
            var deserialized = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<BeautifyConfig>(serialized));

            // 再次序列化以测试往返转换
            var reSerialized = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(deserialized, Newtonsoft.Json.Formatting.Indented));

            // Assert
            deserialized.Should().NotBeNull();
            deserialized.ActiveThemeId.Should().Be(originalConfig.ActiveThemeId);
            deserialized.EnableAnimations.Should().Be(originalConfig.EnableAnimations);
            deserialized.ResponsiveSettings.Should().NotBeNull();
            deserialized.ResponsiveSettings.Desktop.GridColumns.Should().Be(originalConfig.ResponsiveSettings.Desktop.GridColumns);
            
            // 验证序列化的一致性
            serialized.Should().NotBeNullOrEmpty();
            reSerialized.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ThemeCollectionManagement_WithMultipleThemes_ShouldHandleCorrectly()
        {
            // Arrange
            var themes = TestConfiguration.CreateTestThemeList();
            var themeDict = new Dictionary<string, Theme>();

            // Act - 模拟主题集合管理
            foreach (var theme in themes)
            {
                themeDict[theme.Id] = theme;
            }

            // 模拟主题查找和验证
            var foundThemes = new List<Theme>();
            foreach (var themeId in themeDict.Keys)
            {
                if (themeDict.TryGetValue(themeId, out var theme))
                {
                    var validationErrors = theme.Validate();
                    if (validationErrors.Count == 0)
                    {
                        foundThemes.Add(theme);
                    }
                }
            }

            // Assert
            themeDict.Should().HaveCount(themes.Count);
            foundThemes.Should().HaveCount(themes.Count);
            foundThemes.Should().OnlyContain(theme => !string.IsNullOrEmpty(theme.Id));
            foundThemes.Should().OnlyContain(theme => !string.IsNullOrEmpty(theme.Name));

            await Task.CompletedTask; // 保持异步签名
        }
    }
}