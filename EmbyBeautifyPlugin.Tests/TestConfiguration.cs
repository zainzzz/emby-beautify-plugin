using EmbyBeautifyPlugin.Models;
using System.Collections.Generic;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// Test configuration and helper methods for unit tests
    /// </summary>
    public static class TestConfiguration
    {
        /// <summary>
        /// Create a default test configuration
        /// </summary>
        public static BeautifyConfig CreateDefaultTestConfig()
        {
            return new BeautifyConfig
            {
                ActiveThemeId = "test-theme",
                EnableAnimations = true,
                EnableCustomFonts = true,
                AnimationDuration = 300,
                ResponsiveSettings = new ResponsiveSettings
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
                },
                CustomSettings = new Dictionary<string, object>
                {
                    { "testSetting1", "testValue1" },
                    { "testSetting2", 42 },
                    { "testSetting3", true }
                }
            };
        }

        /// <summary>
        /// Create a test theme
        /// </summary>
        public static Theme CreateTestTheme()
        {
            return new Theme
            {
                Id = "test-theme",
                Name = "Test Theme",
                Description = "A theme for testing purposes",
                Version = "1.0.0",
                Author = "Test Author",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Secondary = "#6c757d",
                    Background = "#ffffff",
                    Surface = "#f8f9fa",
                    Text = "#212529",
                    Accent = "#28a745"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "Arial, sans-serif",
                    FontSize = "14px",
                    HeadingWeight = "600",
                    BodyWeight = "400",
                    LineHeight = "1.5"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "8px",
                    SpacingUnit = "1rem",
                    BoxShadow = "0 2px 4px rgba(0,0,0,0.1)",
                    MaxWidth = "1200px"
                },
                CustomProperties = new Dictionary<string, string>
                {
                    { "--custom-property-1", "value1" },
                    { "--custom-property-2", "value2" }
                }
            };
        }

        /// <summary>
        /// Create an invalid configuration for testing validation
        /// </summary>
        public static BeautifyConfig CreateInvalidTestConfig()
        {
            return new BeautifyConfig
            {
                ActiveThemeId = "", // Invalid: empty theme ID
                EnableAnimations = true,
                EnableCustomFonts = true,
                AnimationDuration = -100, // Invalid: negative duration
                ResponsiveSettings = null, // Invalid: null responsive settings
                CustomSettings = null
            };
        }

        /// <summary>
        /// Create a list of test themes
        /// </summary>
        public static List<Theme> CreateTestThemeList()
        {
            return new List<Theme>
            {
                CreateTestTheme(),
                new Theme
                {
                    Id = "dark-theme",
                    Name = "Dark Theme",
                    Description = "A dark theme for testing",
                    Version = "1.0.0",
                    Author = "Test Author",
                    Colors = new ThemeColors
                    {
                        Primary = "#0d6efd",
                        Secondary = "#6c757d",
                        Background = "#212529",
                        Surface = "#343a40",
                        Text = "#ffffff",
                        Accent = "#20c997"
                    }
                },
                new Theme
                {
                    Id = "light-theme",
                    Name = "Light Theme",
                    Description = "A light theme for testing",
                    Version = "1.0.0",
                    Author = "Test Author",
                    Colors = new ThemeColors
                    {
                        Primary = "#0d6efd",
                        Secondary = "#6c757d",
                        Background = "#ffffff",
                        Surface = "#f8f9fa",
                        Text = "#212529",
                        Accent = "#fd7e14"
                    }
                }
            };
        }

        /// <summary>
        /// Get a sample BeautifyConfig for testing
        /// </summary>
        public static BeautifyConfig GetSampleBeautifyConfig()
        {
            return CreateDefaultTestConfig();
        }

        /// <summary>
        /// Get a sample Theme for testing
        /// </summary>
        public static Theme GetSampleTheme()
        {
            return CreateTestTheme();
        }

        /// <summary>
        /// Get a list of sample themes for testing
        /// </summary>
        public static List<Theme> GetSampleThemes()
        {
            return CreateTestThemeList();
        }

        /// <summary>
        /// Create a complex theme for performance testing
        /// </summary>
        public static Theme GetComplexTheme()
        {
            var customProperties = new Dictionary<string, string>();
            
            // 添加大量自定义属性用于性能测试
            for (int i = 0; i < 50; i++)
            {
                customProperties[$"--test-property-{i}"] = $"test-value-{i}";
            }

            return new Theme
            {
                Id = "complex-test-theme",
                Name = "Complex Test Theme",
                Description = "A complex theme for performance and stress testing",
                Version = "1.0.0",
                Author = "Test Suite",
                Colors = new ThemeColors
                {
                    Primary = "#007acc",
                    Secondary = "#6c757d",
                    Background = "#ffffff",
                    Surface = "#f8f9fa",
                    Text = "#212529",
                    Accent = "#28a745"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "Arial, Helvetica, sans-serif",
                    FontSize = "14px",
                    HeadingWeight = "600",
                    BodyWeight = "400",
                    LineHeight = "1.5"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "8px",
                    SpacingUnit = "1rem",
                    BoxShadow = "0 2px 4px rgba(0,0,0,0.1)",
                    MaxWidth = "1200px"
                },
                CustomProperties = customProperties
            };
        }

        /// <summary>
        /// Create a minimal theme for basic testing
        /// </summary>
        public static Theme GetMinimalTheme()
        {
            return new Theme
            {
                Id = "minimal-theme",
                Name = "Minimal Theme",
                Description = "A minimal theme for basic testing",
                Version = "1.0.0",
                Author = "Test Suite",
                Colors = new ThemeColors
                {
                    Primary = "#000000",
                    Secondary = "#666666",
                    Background = "#ffffff",
                    Surface = "#f0f0f0",
                    Text = "#000000",
                    Accent = "#0066cc"
                },
                Typography = new ThemeTypography
                {
                    FontFamily = "Arial",
                    FontSize = "14px",
                    HeadingWeight = "bold",
                    BodyWeight = "normal",
                    LineHeight = "1.4"
                },
                Layout = new ThemeLayout
                {
                    BorderRadius = "4px",
                    SpacingUnit = "8px",
                    BoxShadow = "none",
                    MaxWidth = "100%"
                },
                CustomProperties = new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Create test configuration with specific responsive settings
        /// </summary>
        public static BeautifyConfig CreateResponsiveTestConfig()
        {
            return new BeautifyConfig
            {
                ActiveThemeId = "responsive-test-theme",
                EnableAnimations = true,
                EnableCustomFonts = true,
                AnimationDuration = 250,
                ResponsiveSettings = new ResponsiveSettings
                {
                    Desktop = new BreakpointSettings
                    {
                        MinWidth = 1200,
                        MaxWidth = 9999,
                        GridColumns = 8,
                        GridGap = "2rem",
                        FontScale = 1.1
                    },
                    Tablet = new BreakpointSettings
                    {
                        MinWidth = 768,
                        MaxWidth = 1199,
                        GridColumns = 6,
                        GridGap = "1.5rem",
                        FontScale = 1.0
                    },
                    Mobile = new BreakpointSettings
                    {
                        MinWidth = 0,
                        MaxWidth = 767,
                        GridColumns = 3,
                        GridGap = "1rem",
                        FontScale = 0.9
                    }
                },
                CustomSettings = new Dictionary<string, object>
                {
                    { "responsiveTest", true },
                    { "testMode", "responsive" }
                }
            };
        }

        /// <summary>
        /// Create animation settings for testing
        /// </summary>
        public static AnimationSettings CreateTestAnimationSettings()
        {
            return new AnimationSettings
            {
                EnableAnimations = true,
                GlobalDuration = 300,
                EnableHardwareAcceleration = true,
                ReducedMotion = false
            };
        }

        /// <summary>
        /// Create interaction settings for testing
        /// </summary>
        public static InteractionSettings CreateTestInteractionSettings()
        {
            return new InteractionSettings
            {
                EnableInteractionEnhancements = true,
                GlobalSettings = new GlobalInteractionConfig
                {
                    EnableHardwareAcceleration = true,
                    RespectReducedMotion = true,
                    GlobalDurationMultiplier = 1.0,
                    EnableTouchOptimization = true
                }
            };
        }

        /// <summary>
        /// Get test CSS content for injection testing
        /// </summary>
        public static string GetTestCssContent()
        {
            return @"
                /* Test CSS Content */
                .emby-beautify-test {
                    color: #007acc;
                    background-color: #ffffff;
                    border-radius: 8px;
                    padding: 1rem;
                    margin: 0.5rem;
                    transition: all 0.3s ease;
                }
                
                .emby-beautify-test:hover {
                    background-color: #f8f9fa;
                    transform: translateY(-2px);
                    box-shadow: 0 4px 8px rgba(0,0,0,0.1);
                }
                
                @media (max-width: 768px) {
                    .emby-beautify-test {
                        padding: 0.5rem;
                        margin: 0.25rem;
                    }
                }
            ";
        }

        /// <summary>
        /// Create test data for performance benchmarking
        /// </summary>
        public static Dictionary<string, object> CreatePerformanceTestData()
        {
            return new Dictionary<string, object>
            {
                { "iterations", 100 },
                { "maxTimeMs", 50 },
                { "memoryLimitMB", 100 },
                { "concurrentUsers", 10 },
                { "testDataSize", "large" }
            };
        }
    }
}