using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 集成测试配置和环境管理
    /// </summary>
    public static class IntegrationTestConfiguration
    {
        /// <summary>
        /// 测试环境配置
        /// </summary>
        public static class Environment
        {
            public const string TestDataDirectory = "TestData";
            public const string TestConfigFile = "test-config.json";
            public const string TestThemeDirectory = "TestThemes";
            public const string TestOutputDirectory = "TestOutput";
        }

        /// <summary>
        /// 性能测试基准值
        /// </summary>
        public static class PerformanceBenchmarks
        {
            public const int ThemeGenerationMaxTimeMs = 50;
            public const int StyleInjectionMaxTimeMs = 20;
            public const int ThemeSwitchMaxTimeMs = 100;
            public const int ConfigLoadMaxTimeMs = 10;
            public const int ResponsiveCssMaxTimeMs = 75;
            public const int ConcurrentOperationsMaxTimeMs = 200;
            public const int LargeThemeMaxTimeMs = 150;
            public const long MaxMemoryIncreaseMB = 100;
        }

        /// <summary>
        /// 兼容性测试配置
        /// </summary>
        public static class Compatibility
        {
            public static readonly string[] SupportedEmbyVersions = 
            {
                "4.7.0", "4.7.5", "4.8.0", "4.8.5"
            };

            public static readonly string[] SupportedPlatforms = 
            {
                "Windows", "Linux", "macOS", "Docker"
            };

            public static readonly Dictionary<string, string> SupportedBrowsers = new()
            {
                { "Chrome", "100.0" },
                { "Firefox", "95.0" },
                { "Safari", "15.0" },
                { "Edge", "100.0" }
            };

            public static readonly string[] SupportedLocales = 
            {
                "en-US", "zh-CN", "ja-JP", "de-DE", "fr-FR"
            };

            public static readonly Dictionary<string, (int width, int height)> TestResolutions = new()
            {
                { "Desktop", (1920, 1080) },
                { "Desktop2K", (2560, 1440) },
                { "Desktop4K", (3840, 2160) },
                { "TabletLandscape", (1024, 768) },
                { "TabletPortrait", (768, 1024) },
                { "Mobile", (375, 667) },
                { "MobileLarge", (414, 896) }
            };
        }

        /// <summary>
        /// 测试数据生成器
        /// </summary>
        public static class TestDataGenerator
        {
            /// <summary>
            /// 生成大量测试主题
            /// </summary>
            public static List<EmbyBeautifyPlugin.Models.Theme> GenerateTestThemes(int count)
            {
                var themes = new List<EmbyBeautifyPlugin.Models.Theme>();
                
                for (int i = 0; i < count; i++)
                {
                    themes.Add(new EmbyBeautifyPlugin.Models.Theme
                    {
                        Id = $"generated-theme-{i}",
                        Name = $"Generated Theme {i}",
                        Description = $"Auto-generated theme for testing purposes - {i}",
                        Version = "1.0.0",
                        Author = "Test Generator",
                        Colors = new EmbyBeautifyPlugin.Models.ThemeColors
                        {
                            Primary = GenerateRandomColor(i),
                            Secondary = GenerateRandomColor(i + 1),
                            Background = i % 2 == 0 ? "#ffffff" : "#000000",
                            Surface = i % 2 == 0 ? "#f8f9fa" : "#1a1a1a",
                            Text = i % 2 == 0 ? "#212529" : "#ffffff",
                            Accent = GenerateRandomColor(i + 2)
                        },
                        Typography = new EmbyBeautifyPlugin.Models.ThemeTypography
                        {
                            FontFamily = GetRandomFontFamily(i),
                            FontSize = $"{12 + (i % 8)}px",
                            HeadingWeight = (400 + (i % 4) * 100).ToString(),
                            BodyWeight = "400",
                            LineHeight = (1.2 + (i % 5) * 0.1).ToString("F1")
                        },
                        Layout = new EmbyBeautifyPlugin.Models.ThemeLayout
                        {
                            BorderRadius = $"{i % 16}px",
                            SpacingUnit = $"{0.5 + (i % 4) * 0.25}rem",
                            BoxShadow = GenerateRandomBoxShadow(i),
                            MaxWidth = $"{1000 + (i % 10) * 20}px"
                        },
                        CustomProperties = GenerateCustomProperties(i)
                    });
                }
                
                return themes;
            }

            /// <summary>
            /// 生成随机颜色
            /// </summary>
            private static string GenerateRandomColor(int seed)
            {
                var random = new Random(seed);
                return $"#{random.Next(0, 256):X2}{random.Next(0, 256):X2}{random.Next(0, 256):X2}";
            }

            /// <summary>
            /// 获取随机字体族
            /// </summary>
            private static string GetRandomFontFamily(int seed)
            {
                var fonts = new[]
                {
                    "Arial, sans-serif",
                    "Helvetica, Arial, sans-serif",
                    "Georgia, serif",
                    "Times New Roman, serif",
                    "Courier New, monospace",
                    "Verdana, sans-serif",
                    "Trebuchet MS, sans-serif",
                    "Impact, sans-serif"
                };
                
                return fonts[seed % fonts.Length];
            }

            /// <summary>
            /// 生成随机阴影
            /// </summary>
            private static string GenerateRandomBoxShadow(int seed)
            {
                var shadows = new[]
                {
                    "none",
                    "0 1px 3px rgba(0,0,0,0.1)",
                    "0 2px 4px rgba(0,0,0,0.1)",
                    "0 4px 8px rgba(0,0,0,0.1)",
                    "0 8px 16px rgba(0,0,0,0.1)",
                    "0 1px 3px rgba(0,0,0,0.2), 0 1px 2px rgba(0,0,0,0.1)"
                };
                
                return shadows[seed % shadows.Length];
            }

            /// <summary>
            /// 生成自定义属性
            /// </summary>
            private static Dictionary<string, string> GenerateCustomProperties(int seed)
            {
                var properties = new Dictionary<string, string>();
                var count = seed % 10 + 1; // 1-10个属性
                
                for (int i = 0; i < count; i++)
                {
                    properties[$"--custom-prop-{seed}-{i}"] = $"value-{seed}-{i}";
                }
                
                return properties;
            }
        }

        /// <summary>
        /// 测试环境设置
        /// </summary>
        public static class TestEnvironment
        {
            /// <summary>
            /// 设置测试环境
            /// </summary>
            public static async Task SetupAsync()
            {
                // 创建测试目录
                CreateTestDirectories();
                
                // 设置环境变量
                SetupEnvironmentVariables();
                
                // 初始化测试数据
                await InitializeTestDataAsync();
            }

            /// <summary>
            /// 清理测试环境
            /// </summary>
            public static async Task CleanupAsync()
            {
                // 清理测试文件
                CleanupTestFiles();
                
                // 重置环境变量
                ResetEnvironmentVariables();
                
                await Task.CompletedTask;
            }

            /// <summary>
            /// 创建测试目录
            /// </summary>
            private static void CreateTestDirectories()
            {
                var directories = new[]
                {
                    Environment.TestDataDirectory,
                    Environment.TestThemeDirectory,
                    Environment.TestOutputDirectory
                };

                foreach (var dir in directories)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
            }

            /// <summary>
            /// 设置环境变量
            /// </summary>
            private static void SetupEnvironmentVariables()
            {
                System.Environment.SetEnvironmentVariable("EMBY_BEAUTIFY_TEST_MODE", "true");
                System.Environment.SetEnvironmentVariable("EMBY_BEAUTIFY_TEST_DATA_PATH", Environment.TestDataDirectory);
            }

            /// <summary>
            /// 初始化测试数据
            /// </summary>
            private static async Task InitializeTestDataAsync()
            {
                // 创建测试配置文件
                var testConfigPath = Path.Combine(Environment.TestDataDirectory, Environment.TestConfigFile);
                if (!File.Exists(testConfigPath))
                {
                    var testConfig = TestConfiguration.CreateDefaultTestConfig();
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(testConfig, Newtonsoft.Json.Formatting.Indented);
                    await File.WriteAllTextAsync(testConfigPath, json);
                }
            }

            /// <summary>
            /// 清理测试文件
            /// </summary>
            private static void CleanupTestFiles()
            {
                var directories = new[]
                {
                    Environment.TestDataDirectory,
                    Environment.TestThemeDirectory,
                    Environment.TestOutputDirectory
                };

                foreach (var dir in directories)
                {
                    if (Directory.Exists(dir))
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                        }
                        catch
                        {
                            // 忽略清理错误
                        }
                    }
                }
            }

            /// <summary>
            /// 重置环境变量
            /// </summary>
            private static void ResetEnvironmentVariables()
            {
                System.Environment.SetEnvironmentVariable("EMBY_BEAUTIFY_TEST_MODE", null);
                System.Environment.SetEnvironmentVariable("EMBY_BEAUTIFY_TEST_DATA_PATH", null);
            }
        }

        /// <summary>
        /// 测试断言辅助方法
        /// </summary>
        public static class TestAssertions
        {
            /// <summary>
            /// 断言CSS内容有效
            /// </summary>
            public static void AssertValidCss(string css)
            {
                if (string.IsNullOrWhiteSpace(css))
                    throw new ArgumentException("CSS content cannot be null or empty");

                // 基本CSS语法检查
                var openBraces = css.Split('{').Length - 1;
                var closeBraces = css.Split('}').Length - 1;
                
                if (openBraces != closeBraces)
                    throw new ArgumentException("CSS has mismatched braces");
            }

            /// <summary>
            /// 断言主题有效
            /// </summary>
            public static void AssertValidTheme(EmbyBeautifyPlugin.Models.Theme theme)
            {
                if (theme == null)
                    throw new ArgumentNullException(nameof(theme));

                if (string.IsNullOrWhiteSpace(theme.Id))
                    throw new ArgumentException("Theme ID cannot be null or empty");

                if (string.IsNullOrWhiteSpace(theme.Name))
                    throw new ArgumentException("Theme Name cannot be null or empty");

                if (theme.Colors == null)
                    throw new ArgumentException("Theme Colors cannot be null");

                if (theme.Typography == null)
                    throw new ArgumentException("Theme Typography cannot be null");

                if (theme.Layout == null)
                    throw new ArgumentException("Theme Layout cannot be null");
            }

            /// <summary>
            /// 断言配置有效
            /// </summary>
            public static void AssertValidConfiguration(EmbyBeautifyPlugin.Models.BeautifyConfig config)
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                if (string.IsNullOrWhiteSpace(config.ActiveThemeId))
                    throw new ArgumentException("ActiveThemeId cannot be null or empty");

                if (config.AnimationDuration < 0)
                    throw new ArgumentException("AnimationDuration cannot be negative");

                if (config.ResponsiveSettings == null)
                    throw new ArgumentException("ResponsiveSettings cannot be null");
            }
        }
    }
}