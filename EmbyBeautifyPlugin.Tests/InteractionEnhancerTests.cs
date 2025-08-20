using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using EmbyBeautifyPlugin.Services;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Interfaces;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 交互增强器测试类
    /// </summary>
    public class InteractionEnhancerTests
    {
        private readonly Mock<ILogger<InteractionEnhancer>> _mockLogger;
        private readonly InteractionEnhancer _interactionEnhancer;

        public InteractionEnhancerTests()
        {
            _mockLogger = new Mock<ILogger<InteractionEnhancer>>();
            _interactionEnhancer = new InteractionEnhancer(_mockLogger.Object);
        }

        [Fact]
        public async Task GenerateHoverEffectCssAsync_EnabledConfig_ShouldReturnValidCss()
        {
            // Arrange
            var selector = ".test-element";
            var config = new HoverEffectConfig
            {
                Enabled = true,
                Duration = 200,
                Easing = "ease",
                Transform = new TransformEffect { Scale = 1.05, TranslateY = -2 },
                Shadow = new ShadowEffect { Enabled = true, BoxShadow = "0 4px 8px rgba(0,0,0,0.1)" }
            };

            // Act
            var css = await _interactionEnhancer.GenerateHoverEffectCssAsync(selector, config);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain(selector);
            css.Should().Contain("transition:");
            css.Should().Contain("200ms");
            css.Should().Contain("ease");
            css.Should().Contain(":hover");
            css.Should().Contain("scale(1.05)");
            css.Should().Contain("translate(0px, -2px)");
            css.Should().Contain("box-shadow:");
        }

        [Fact]
        public async Task GenerateHoverEffectCssAsync_DisabledConfig_ShouldReturnEmptyString()
        {
            // Arrange
            var selector = ".test-element";
            var config = new HoverEffectConfig { Enabled = false };

            // Act
            var css = await _interactionEnhancer.GenerateHoverEffectCssAsync(selector, config);

            // Assert
            css.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateHoverEffectCssAsync_WithColorEffects_ShouldIncludeColorProperties()
        {
            // Arrange
            var selector = ".test-element";
            var config = new HoverEffectConfig
            {
                Enabled = true,
                Color = new ColorEffect
                {
                    BackgroundColor = "#ff0000",
                    TextColor = "#ffffff",
                    BorderColor = "#00ff00"
                }
            };

            // Act
            var css = await _interactionEnhancer.GenerateHoverEffectCssAsync(selector, config);

            // Assert
            css.Should().Contain("background-color: #ff0000");
            css.Should().Contain("color: #ffffff");
            css.Should().Contain("border-color: #00ff00");
        }

        [Theory]
        [InlineData(ClickFeedbackType.Scale)]
        [InlineData(ClickFeedbackType.Ripple)]
        [InlineData(ClickFeedbackType.Pulse)]
        [InlineData(ClickFeedbackType.Flash)]
        public async Task GenerateClickFeedbackCssAsync_ValidTypes_ShouldReturnValidCss(ClickFeedbackType type)
        {
            // Arrange
            var selector = ".test-button";
            var config = new ClickFeedbackConfig
            {
                Enabled = true,
                Type = type,
                Duration = 150,
                Easing = "ease-out"
            };

            // Act
            var css = await _interactionEnhancer.GenerateClickFeedbackCssAsync(selector, config);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain(selector);
        }

        [Fact]
        public async Task GenerateClickFeedbackCssAsync_ScaleType_ShouldContainScaleTransform()
        {
            // Arrange
            var selector = ".test-button";
            var config = new ClickFeedbackConfig
            {
                Enabled = true,
                Type = ClickFeedbackType.Scale,
                Scale = new ScaleEffect { Enabled = true, Value = 0.95 }
            };

            // Act
            var css = await _interactionEnhancer.GenerateClickFeedbackCssAsync(selector, config);

            // Assert
            css.Should().Contain(":active");
            css.Should().Contain("scale(0.95)");
            css.Should().Contain("transition:");
        }

        [Fact]
        public async Task GenerateClickFeedbackCssAsync_RippleType_ShouldContainRippleAnimation()
        {
            // Arrange
            var selector = ".test-button";
            var config = new ClickFeedbackConfig
            {
                Enabled = true,
                Type = ClickFeedbackType.Ripple,
                Color = "rgba(255, 255, 255, 0.3)"
            };

            // Act
            var css = await _interactionEnhancer.GenerateClickFeedbackCssAsync(selector, config);

            // Assert
            css.Should().Contain("@keyframes ripple");
            css.Should().Contain("::before");
            css.Should().Contain("rgba(255, 255, 255, 0.3)");
            css.Should().Contain("border-radius: 50%");
        }

        [Fact]
        public async Task GenerateClickFeedbackCssAsync_PulseType_ShouldContainPulseAnimation()
        {
            // Arrange
            var selector = ".test-button";
            var config = new ClickFeedbackConfig
            {
                Enabled = true,
                Type = ClickFeedbackType.Pulse
            };

            // Act
            var css = await _interactionEnhancer.GenerateClickFeedbackCssAsync(selector, config);

            // Assert
            css.Should().Contain("@keyframes pulse");
            css.Should().Contain("box-shadow:");
            css.Should().Contain(":active");
        }

        [Fact]
        public async Task GenerateClickFeedbackCssAsync_FlashType_ShouldContainFlashAnimation()
        {
            // Arrange
            var selector = ".test-button";
            var config = new ClickFeedbackConfig
            {
                Enabled = true,
                Type = ClickFeedbackType.Flash
            };

            // Act
            var css = await _interactionEnhancer.GenerateClickFeedbackCssAsync(selector, config);

            // Assert
            css.Should().Contain("@keyframes flash");
            css.Should().Contain("opacity:");
            css.Should().Contain(":active");
        }

        [Fact]
        public async Task GenerateClickFeedbackCssAsync_DisabledConfig_ShouldReturnEmptyString()
        {
            // Arrange
            var selector = ".test-button";
            var config = new ClickFeedbackConfig { Enabled = false };

            // Act
            var css = await _interactionEnhancer.GenerateClickFeedbackCssAsync(selector, config);

            // Assert
            css.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateClickFeedbackCssAsync_NoneType_ShouldReturnEmptyString()
        {
            // Arrange
            var selector = ".test-button";
            var config = new ClickFeedbackConfig
            {
                Enabled = true,
                Type = ClickFeedbackType.None
            };

            // Act
            var css = await _interactionEnhancer.GenerateClickFeedbackCssAsync(selector, config);

            // Assert
            css.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateFocusEffectCssAsync_EnabledConfig_ShouldReturnValidCss()
        {
            // Arrange
            var selector = "input";
            var config = new FocusEffectConfig
            {
                Enabled = true,
                Duration = 150,
                Outline = new OutlineEffect
                {
                    Color = "#007acc",
                    Width = 2,
                    Offset = 2
                },
                Shadow = new ShadowEffect
                {
                    Enabled = true,
                    BoxShadow = "0 0 0 3px rgba(0, 122, 204, 0.3)"
                }
            };

            // Act
            var css = await _interactionEnhancer.GenerateFocusEffectCssAsync(selector, config);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain(":focus");
            css.Should().Contain("outline:");
            css.Should().Contain("#007acc");
            css.Should().Contain("outline-offset: 2px");
            css.Should().Contain("box-shadow:");
            css.Should().Contain("transition:");
        }

        [Fact]
        public async Task GenerateFocusEffectCssAsync_DisabledConfig_ShouldReturnEmptyString()
        {
            // Arrange
            var selector = "input";
            var config = new FocusEffectConfig { Enabled = false };

            // Act
            var css = await _interactionEnhancer.GenerateFocusEffectCssAsync(selector, config);

            // Assert
            css.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateScrollOptimizationCssAsync_EnabledConfig_ShouldReturnValidCss()
        {
            // Arrange
            var config = new ScrollOptimizationConfig
            {
                EnableSmoothScrolling = true,
                EnableScrollbarStyling = true,
                EnableScrollPerformanceOptimization = true,
                ScrollbarStyle = new ScrollbarStyle
                {
                    Width = 8,
                    TrackColor = "rgba(0, 0, 0, 0.1)",
                    ThumbColor = "rgba(0, 0, 0, 0.3)"
                }
            };

            // Act
            var css = await _interactionEnhancer.GenerateScrollOptimizationCssAsync(config);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("scroll-behavior: smooth");
            css.Should().Contain("::-webkit-scrollbar");
            css.Should().Contain("scrollbar-width: thin");
            css.Should().Contain("-webkit-overflow-scrolling: touch");
        }

        [Fact]
        public async Task GenerateInteractionCssAsync_CompleteSettings_ShouldReturnComprehensiveCss()
        {
            // Arrange
            var settings = await _interactionEnhancer.GetDefaultInteractionSettingsAsync();

            // Act
            var css = await _interactionEnhancer.GenerateInteractionCssAsync(settings);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("@media (prefers-reduced-motion: reduce)");
            css.Should().Contain(".interactive-element");
            css.Should().Contain(".touch-target");
            css.Should().Contain(":hover");
            css.Should().Contain("scroll-behavior: smooth");
        }

        [Fact]
        public async Task GenerateInteractionCssAsync_DisabledSettings_ShouldReturnEmptyString()
        {
            // Arrange
            var settings = new InteractionSettings { EnableInteractionEnhancements = false };

            // Act
            var css = await _interactionEnhancer.GenerateInteractionCssAsync(settings);

            // Assert
            css.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateInteractionSettingsAsync_ValidSettings_ShouldReturnTrue()
        {
            // Arrange
            var settings = await _interactionEnhancer.GetDefaultInteractionSettingsAsync();

            // Act
            var result = await _interactionEnhancer.ValidateInteractionSettingsAsync(settings);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateInteractionSettingsAsync_NullSettings_ShouldReturnFalse()
        {
            // Act
            var result = await _interactionEnhancer.ValidateInteractionSettingsAsync(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateInteractionSettingsAsync_InvalidHoverConfig_ShouldReturnFalse()
        {
            // Arrange
            var settings = new InteractionSettings
            {
                HoverEffects = new System.Collections.Generic.Dictionary<string, HoverEffectConfig>
                {
                    [".test"] = new HoverEffectConfig { Duration = -1 } // 无效持续时间
                }
            };

            // Act
            var result = await _interactionEnhancer.ValidateInteractionSettingsAsync(settings);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateInteractionSettingsAsync_InvalidClickConfig_ShouldReturnFalse()
        {
            // Arrange
            var settings = new InteractionSettings
            {
                ClickFeedbacks = new System.Collections.Generic.Dictionary<string, ClickFeedbackConfig>
                {
                    [".test"] = new ClickFeedbackConfig { Duration = 5000 } // 超过最大值
                }
            };

            // Act
            var result = await _interactionEnhancer.ValidateInteractionSettingsAsync(settings);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateInteractionSettingsAsync_InvalidGlobalConfig_ShouldReturnFalse()
        {
            // Arrange
            var settings = new InteractionSettings
            {
                GlobalSettings = new GlobalInteractionConfig
                {
                    GlobalDurationMultiplier = -1.0 // 无效倍数
                }
            };

            // Act
            var result = await _interactionEnhancer.ValidateInteractionSettingsAsync(settings);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetDefaultInteractionSettingsAsync_ShouldReturnValidSettings()
        {
            // Act
            var settings = await _interactionEnhancer.GetDefaultInteractionSettingsAsync();

            // Assert
            settings.Should().NotBeNull();
            settings.EnableInteractionEnhancements.Should().BeTrue();
            settings.HoverEffects.Should().NotBeEmpty();
            settings.ClickFeedbacks.Should().NotBeEmpty();
            settings.FocusEffects.Should().NotBeEmpty();
            settings.ScrollOptimization.Should().NotBeNull();
            settings.GlobalSettings.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDefaultInteractionSettingsAsync_ShouldPassValidation()
        {
            // Act
            var settings = await _interactionEnhancer.GetDefaultInteractionSettingsAsync();
            var isValid = await _interactionEnhancer.ValidateInteractionSettingsAsync(settings);

            // Assert
            isValid.Should().BeTrue();
        }
    }
}