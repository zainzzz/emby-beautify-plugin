using System;
using System.Linq;
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
    /// 动画控制器测试类
    /// </summary>
    public class AnimationControllerTests
    {
        private readonly Mock<ILogger<AnimationController>> _mockLogger;
        private readonly AnimationController _animationController;

        public AnimationControllerTests()
        {
            _mockLogger = new Mock<ILogger<AnimationController>>();
            _animationController = new AnimationController(_mockLogger.Object);
        }

        [Fact]
        public async Task GetAvailableAnimationsAsync_ShouldReturnAllAnimationTypes()
        {
            // Act
            var animations = await _animationController.GetAvailableAnimationsAsync();

            // Assert
            animations.Should().NotBeNull();
            animations.Should().NotBeEmpty();
            animations.Should().Contain(nameof(AnimationType.FadeIn));
            animations.Should().Contain(nameof(AnimationType.FadeOut));
            animations.Should().Contain(nameof(AnimationType.SlideIn));
            animations.Should().Contain(nameof(AnimationType.ScaleIn));
            animations.Should().NotContain(nameof(AnimationType.Custom)); // Custom应该被排除
        }

        [Theory]
        [InlineData(nameof(AnimationType.FadeIn), 300, "ease")]
        [InlineData(nameof(AnimationType.FadeOut), 200, "ease-in")]
        [InlineData(nameof(AnimationType.SlideIn), 400, "ease-out")]
        [InlineData(nameof(AnimationType.ScaleIn), 250, "linear")]
        public async Task GenerateAnimationCssAsync_ValidInput_ShouldReturnValidCss(string animationType, int duration, string easing)
        {
            // Act
            var css = await _animationController.GenerateAnimationCssAsync(animationType, duration, easing);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("@keyframes");
            css.Should().ContainEquivalentOf(animationType);
            css.Should().Contain($"{duration}ms");
            css.Should().Contain(easing);
        }

        [Fact]
        public async Task GenerateAnimationCssAsync_InvalidAnimationType_ShouldThrowException()
        {
            // Arrange
            var invalidAnimationType = "InvalidAnimation";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _animationController.GenerateAnimationCssAsync(invalidAnimationType));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(15000)] // 超过最大值
        public async Task GenerateAnimationCssAsync_InvalidDuration_ShouldThrowException(int invalidDuration)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _animationController.GenerateAnimationCssAsync(nameof(AnimationType.FadeIn), invalidDuration));
        }

        [Fact]
        public async Task GenerateTransitionCssAsync_ValidProperties_ShouldReturnValidCss()
        {
            // Arrange
            var properties = new[] { "opacity", "transform", "background-color" };
            var duration = 300;
            var easing = "ease-in-out";

            // Act
            var css = await _animationController.GenerateTransitionCssAsync(properties, duration, easing);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("transition:");
            css.Should().Contain("opacity 300ms ease-in-out");
            css.Should().Contain("transform 300ms ease-in-out");
            css.Should().Contain("background-color 300ms ease-in-out");
        }

        [Fact]
        public async Task GenerateTransitionCssAsync_EmptyProperties_ShouldReturnDefaultTransition()
        {
            // Arrange
            var emptyProperties = Array.Empty<string>();

            // Act
            var css = await _animationController.GenerateTransitionCssAsync(emptyProperties);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Be("transition: all 0.3s ease;");
        }

        [Fact]
        public async Task GenerateTransitionCssAsync_NullProperties_ShouldReturnDefaultTransition()
        {
            // Act
            var css = await _animationController.GenerateTransitionCssAsync(null);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Be("transition: all 0.3s ease;");
        }

        [Theory]
        [InlineData(nameof(AnimationType.FadeIn), true)]
        [InlineData(nameof(AnimationType.SlideIn), false)]
        public async Task GetOptimizedAnimationCssAsync_ShouldIncludeOptimizations(string animationType, bool enableHardwareAcceleration)
        {
            // Act
            var css = await _animationController.GetOptimizedAnimationCssAsync(animationType, enableHardwareAcceleration);

            // Assert
            css.Should().NotBeNullOrEmpty();
            css.Should().Contain("@keyframes");
            css.Should().Contain("pointer-events: none");

            if (enableHardwareAcceleration)
            {
                css.Should().Contain("transform: translateZ(0)");
                css.Should().Contain("will-change: transform, opacity");
                css.Should().Contain("backface-visibility: hidden");
            }
            else
            {
                css.Should().NotContain("transform: translateZ(0)");
                css.Should().NotContain("will-change: transform, opacity");
                css.Should().NotContain("backface-visibility: hidden");
            }
        }

        [Theory]
        [InlineData(nameof(AnimationType.FadeIn), 300, true)]
        [InlineData(nameof(AnimationType.SlideIn), 400, true)]
        [InlineData("InvalidType", 300, false)]
        [InlineData(nameof(AnimationType.FadeIn), -1, false)]
        [InlineData(nameof(AnimationType.FadeIn), 15000, false)]
        public async Task ValidateAnimationConfigAsync_ShouldReturnExpectedResult(string animationType, int duration, bool expectedResult)
        {
            // Act
            var result = await _animationController.ValidateAnimationConfigAsync(animationType, duration);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(nameof(AnimationType.FadeIn))]
        [InlineData(nameof(AnimationType.SlideIn))]
        [InlineData(nameof(AnimationType.ScaleIn))]
        [InlineData(nameof(AnimationType.Pulse))]
        public async Task GetDefaultAnimationConfigAsync_KnownAnimationType_ShouldReturnSpecificConfig(string animationType)
        {
            // Act
            var config = await _animationController.GetDefaultAnimationConfigAsync(animationType);

            // Assert
            config.Should().NotBeNull();
            config.Type.Should().Be(animationType);
            config.Duration.Should().BeGreaterThan(0);
            config.Easing.Should().NotBeNullOrEmpty();
            config.EnableHardwareAcceleration.Should().BeTrue();
        }

        [Fact]
        public async Task GetDefaultAnimationConfigAsync_UnknownAnimationType_ShouldReturnGenericConfig()
        {
            // Arrange
            var unknownType = "UnknownAnimation";

            // Act
            var config = await _animationController.GetDefaultAnimationConfigAsync(unknownType);

            // Assert
            config.Should().NotBeNull();
            config.Type.Should().Be(unknownType);
            config.Duration.Should().Be(300);
            config.Easing.Should().Be("ease");
            config.EnableHardwareAcceleration.Should().BeTrue();
        }

        [Fact]
        public async Task GenerateAnimationCssAsync_FadeIn_ShouldContainCorrectKeyframes()
        {
            // Act
            var css = await _animationController.GenerateAnimationCssAsync(nameof(AnimationType.FadeIn));

            // Assert
            css.Should().Contain("@keyframes fadeIn");
            css.Should().Contain("0%");
            css.Should().Contain("100%");
            css.Should().Contain("opacity: 0");
            css.Should().Contain("opacity: 1");
            css.Should().Contain(".fade-in");
        }

        [Fact]
        public async Task GenerateAnimationCssAsync_SlideIn_ShouldContainCorrectTransforms()
        {
            // Act
            var css = await _animationController.GenerateAnimationCssAsync(nameof(AnimationType.SlideIn));

            // Assert
            css.Should().Contain("@keyframes slideIn");
            css.Should().Contain("translateX(-100%)");
            css.Should().Contain("translateX(0)");
            css.Should().Contain(".slide-in");
        }

        [Fact]
        public async Task GenerateAnimationCssAsync_ScaleIn_ShouldContainCorrectScaling()
        {
            // Act
            var css = await _animationController.GenerateAnimationCssAsync(nameof(AnimationType.ScaleIn));

            // Assert
            css.Should().Contain("@keyframes scaleIn");
            css.Should().Contain("scale(0)");
            css.Should().Contain("scale(1)");
            css.Should().Contain(".scale-in");
        }

        [Fact]
        public async Task GenerateAnimationCssAsync_BounceIn_ShouldContainMultipleKeyframes()
        {
            // Act
            var css = await _animationController.GenerateAnimationCssAsync(nameof(AnimationType.BounceIn));

            // Assert
            css.Should().Contain("@keyframes bounceIn");
            css.Should().Contain("0%");
            css.Should().Contain("50%");
            css.Should().Contain("70%");
            css.Should().Contain("100%");
            css.Should().Contain("scale(0.3)");
            css.Should().Contain("scale(1.05)");
            css.Should().Contain(".bounce-in");
        }

        [Fact]
        public async Task GenerateAnimationCssAsync_Pulse_ShouldContainInfiniteAnimation()
        {
            // Act
            var css = await _animationController.GenerateAnimationCssAsync(nameof(AnimationType.Pulse));

            // Assert
            css.Should().Contain("@keyframes pulse");
            css.Should().Contain("infinite");
            css.Should().Contain(".pulse");
        }

        [Fact]
        public async Task GenerateAnimationCssAsync_Shake_ShouldContainTranslateXValues()
        {
            // Act
            var css = await _animationController.GenerateAnimationCssAsync(nameof(AnimationType.Shake));

            // Assert
            css.Should().Contain("@keyframes shake");
            css.Should().Contain("translateX(0)");
            css.Should().Contain("translateX(-10px)");
            css.Should().Contain("translateX(10px)");
            css.Should().Contain(".shake");
        }

        [Fact]
        public async Task GenerateAnimationCssAsync_RotateIn_ShouldContainRotationAndScale()
        {
            // Act
            var css = await _animationController.GenerateAnimationCssAsync(nameof(AnimationType.RotateIn));

            // Assert
            css.Should().Contain("@keyframes rotateIn");
            css.Should().Contain("rotate(-180deg)");
            css.Should().Contain("rotate(0deg)");
            css.Should().Contain("scale(0)");
            css.Should().Contain("scale(1)");
            css.Should().Contain(".rotate-in");
        }
    }
}