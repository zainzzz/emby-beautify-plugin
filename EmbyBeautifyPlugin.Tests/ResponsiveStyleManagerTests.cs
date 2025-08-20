using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 响应式样式管理器的单元测试
    /// </summary>
    public class ResponsiveStyleManagerTests
    {
        private readonly Mock<ILogger<ResponsiveStyleManager>> _mockLogger;
        private readonly ResponsiveStyleManager _responsiveManager;

        public ResponsiveStyleManagerTests()
        {
            _mockLogger = new Mock<ILogger<ResponsiveStyleManager>>();
            _responsiveManager = new ResponsiveStyleManager(_mockLogger.Object);
        }

        [Fact]
        public async Task GenerateResponsiveCssAsync_ValidSettings_GeneratesCSS()
        {
            // Arrange
            var settings = new ResponsiveSettings
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

            // Act
            var css = await _responsiveManager.GenerateResponsiveCssAsync(settings);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("@media (min-width: 1200px)");
            css.Should().Contain("@media (min-width: 768px) and (max-width: 1199px)");
            css.Should().Contain("@media (max-width: 767px)");
            css.Should().Contain("--responsive-columns: 6");
            css.Should().Contain("--responsive-columns: 4");
            css.Should().Contain("--responsive-columns: 2");
        }

        [Fact]
        public async Task GenerateResponsiveCssAsync_NullSettings_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _responsiveManager.Invoking(r => r.GenerateResponsiveCssAsync(null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GenerateResponsiveCssAsync_OnlyMobileSettings_GeneratesMobileCSS()
        {
            // Arrange
            var settings = new ResponsiveSettings
            {
                Mobile = new BreakpointSettings
                {
                    MinWidth = 0,
                    MaxWidth = 767,
                    GridColumns = 1,
                    GridGap = "0.5rem",
                    FontScale = 0.8
                }
            };

            // Act
            var css = await _responsiveManager.GenerateResponsiveCssAsync(settings);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("@media (max-width: 767px)");
            css.Should().Contain("--responsive-columns: 1");
            css.Should().Contain("hide-mobile");
            css.Should().Contain("show-mobile");
            css.Should().NotContain("@media (min-width: 1200px)");
            css.Should().NotContain("@media (min-width: 768px)");
        }

        [Fact]
        public async Task GenerateResponsiveCssAsync_WithTheme_GeneratesThemeAwareCSS()
        {
            // Arrange
            var settings = new ResponsiveSettings
            {
                Desktop = new BreakpointSettings
                {
                    MinWidth = 1200,
                    MaxWidth = 9999,
                    GridColumns = 6,
                    GridGap = "1.5rem",
                    FontScale = 1.0
                }
            };
            var theme = TestConfiguration.GetSampleTheme();

            // Act
            var css = await _responsiveManager.GenerateResponsiveCssAsync(settings, theme);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("@media (min-width: 1200px)");
        }

        [Fact]
        public async Task GenerateResponsiveCssAsync_IncludesUtilityClasses()
        {
            // Arrange
            var settings = new ResponsiveSettings
            {
                Mobile = new BreakpointSettings
                {
                    MinWidth = 0,
                    MaxWidth = 767,
                    GridColumns = 2,
                    GridGap = "0.5rem",
                    FontScale = 0.8
                }
            };

            // Act
            var css = await _responsiveManager.GenerateResponsiveCssAsync(settings);

            // Assert
            css.Should().Contain("responsive-text");
            css.Should().Contain("responsive-spacing");
            css.Should().Contain("responsive-padding");
            css.Should().Contain("flex-responsive");
            css.Should().Contain("responsive-image");
            css.Should().Contain("responsive-video");
        }

        [Fact]
        public void GetBreakpoint_ExistingBreakpoint_ReturnsDefinition()
        {
            // Act
            var breakpoint = _responsiveManager.GetBreakpoint("mobile");

            // Assert
            breakpoint.Should().NotBeNull();
            breakpoint.Name.Should().Be("mobile");
            breakpoint.MinWidth.Should().Be(0);
            breakpoint.MaxWidth.Should().Be(767);
        }

        [Fact]
        public void GetBreakpoint_NonExistentBreakpoint_ReturnsNull()
        {
            // Act
            var breakpoint = _responsiveManager.GetBreakpoint("nonexistent");

            // Assert
            breakpoint.Should().BeNull();
        }

        [Fact]
        public void GetAllBreakpoints_ReturnsAllDefaultBreakpoints()
        {
            // Act
            var breakpoints = _responsiveManager.GetAllBreakpoints();

            // Assert
            breakpoints.Should().HaveCount(3);
            breakpoints.Should().Contain(b => b.Name == "mobile");
            breakpoints.Should().Contain(b => b.Name == "tablet");
            breakpoints.Should().Contain(b => b.Name == "desktop");
        }

        [Fact]
        public void AddBreakpoint_ValidDefinition_AddsBreakpoint()
        {
            // Arrange
            var customBreakpoint = new BreakpointDefinition
            {
                Name = "large-desktop",
                MinWidth = 1920,
                MaxWidth = int.MaxValue,
                Description = "大屏桌面"
            };

            // Act
            _responsiveManager.AddBreakpoint(customBreakpoint);
            var result = _responsiveManager.GetBreakpoint("large-desktop");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("large-desktop");
            result.MinWidth.Should().Be(1920);
        }

        [Fact]
        public void AddBreakpoint_NullDefinition_ThrowsArgumentNullException()
        {
            // Act & Assert
            _responsiveManager.Invoking(r => r.AddBreakpoint(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ValidateResponsiveSettings_ValidSettings_ReturnsValid()
        {
            // Arrange
            var settings = new ResponsiveSettings
            {
                Desktop = new BreakpointSettings
                {
                    GridColumns = 6,
                    GridGap = "1.5rem",
                    FontScale = 1.0
                },
                Mobile = new BreakpointSettings
                {
                    GridColumns = 2,
                    GridGap = "0.5rem",
                    FontScale = 0.8
                }
            };

            // Act
            var result = _responsiveManager.ValidateResponsiveSettings(settings);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateResponsiveSettings_NullSettings_ReturnsInvalid()
        {
            // Act
            var result = _responsiveManager.ValidateResponsiveSettings(null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("响应式设置不能为空");
        }

        [Fact]
        public void ValidateResponsiveSettings_InvalidGridColumns_ReturnsInvalid()
        {
            // Arrange
            var settings = new ResponsiveSettings
            {
                Desktop = new BreakpointSettings
                {
                    GridColumns = 0, // 无效值
                    GridGap = "1.5rem",
                    FontScale = 1.0
                }
            };

            // Act
            var result = _responsiveManager.ValidateResponsiveSettings(settings);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Desktop: 网格列数必须大于0");
        }

        [Fact]
        public void ValidateResponsiveSettings_InvalidFontScale_ReturnsInvalid()
        {
            // Arrange
            var settings = new ResponsiveSettings
            {
                Mobile = new BreakpointSettings
                {
                    GridColumns = 2,
                    GridGap = "0.5rem",
                    FontScale = 0 // 无效值
                }
            };

            // Act
            var result = _responsiveManager.ValidateResponsiveSettings(settings);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Mobile: 字体缩放比例必须大于0");
        }

        [Fact]
        public void ValidateResponsiveSettings_EmptyGridGap_ReturnsWarning()
        {
            // Arrange
            var settings = new ResponsiveSettings
            {
                Tablet = new BreakpointSettings
                {
                    GridColumns = 4,
                    GridGap = "", // 空值
                    FontScale = 0.9
                }
            };

            // Act
            var result = _responsiveManager.ValidateResponsiveSettings(settings);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Warnings.Should().Contain("Tablet: 网格间距为空，将使用默认值");
        }

        [Theory]
        [InlineData("mobile", 0, 767)]
        [InlineData("tablet", 768, 1199)]
        [InlineData("desktop", 1200, int.MaxValue)]
        public void GetBreakpoint_DefaultBreakpoints_ReturnsCorrectValues(string name, int expectedMinWidth, int expectedMaxWidth)
        {
            // Act
            var breakpoint = _responsiveManager.GetBreakpoint(name);

            // Assert
            breakpoint.Should().NotBeNull();
            breakpoint.MinWidth.Should().Be(expectedMinWidth);
            breakpoint.MaxWidth.Should().Be(expectedMaxWidth);
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ResponsiveStyleManager(null));
        }

        [Fact]
        public async Task GenerateResponsiveCssAsync_ComplexSettings_GeneratesCompleteCSS()
        {
            // Arrange
            var settings = new ResponsiveSettings
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
                    GridColumns = 4,
                    GridGap = "1rem",
                    FontScale = 0.95
                },
                Mobile = new BreakpointSettings
                {
                    MinWidth = 0,
                    MaxWidth = 767,
                    GridColumns = 1,
                    GridGap = "0.5rem",
                    FontScale = 0.85
                }
            };

            // Act
            var css = await _responsiveManager.GenerateResponsiveCssAsync(settings);

            // Assert
            css.Should().NotBeNullOrEmpty();
            
            // 验证所有断点都存在
            css.Should().Contain("@media (min-width: 1200px)");
            css.Should().Contain("@media (min-width: 768px) and (max-width: 1199px)");
            css.Should().Contain("@media (max-width: 767px)");
            
            // 验证网格列数
            css.Should().Contain("--responsive-columns: 8");
            css.Should().Contain("--responsive-columns: 4");
            css.Should().Contain("--responsive-columns: 1");
            
            // 验证间距
            css.Should().Contain("--responsive-gap: 2rem");
            css.Should().Contain("--responsive-gap: 1rem");
            css.Should().Contain("--responsive-gap: 0.5rem");
            
            // 验证字体缩放
            css.Should().Contain("--responsive-font-scale: 1.1");
            css.Should().Contain("--responsive-font-scale: 0.95");
            css.Should().Contain("--responsive-font-scale: 0.85");
            
            // 验证工具类
            css.Should().Contain("hide-mobile");
            css.Should().Contain("show-tablet");
            css.Should().Contain("desktop-multi-column");
        }
    }
}