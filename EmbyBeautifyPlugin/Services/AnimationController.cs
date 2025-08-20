using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EmbyBeautifyPlugin.Abstracts;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Extensions;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 动画控制器实现类
    /// </summary>
    public class AnimationController : BaseAnimationController
    {
        private readonly Dictionary<string, Func<int, string, string>> _animationGenerators;

        public AnimationController(ILogger<AnimationController> logger) : base(logger)
        {
            _animationGenerators = InitializeAnimationGenerators();
        }

        /// <summary>
        /// 生成指定动画的CSS代码
        /// </summary>
        public override async Task<string> GenerateAnimationCssAsync(string animationType, int duration = 300, string easing = "ease")
        {
            using var timer = _logger.BeginOperation("GenerateAnimationCss", new Dictionary<string, object>
            {
                ["AnimationType"] = animationType,
                ["Duration"] = duration,
                ["Easing"] = easing
            });

            try
            {
                if (!await ValidateAnimationConfigAsync(animationType, duration))
                {
                    throw new ArgumentException($"无效的动画配置: {animationType}, {duration}ms");
                }

                if (_animationGenerators.TryGetValue(animationType, out var generator))
                {
                    var css = generator(duration, easing);
                    _logger.LogDebug("生成动画CSS成功: {AnimationType}, 长度: {Length}", animationType, css.Length);
                    return await Task.FromResult(css);
                }

                _logger.LogWarning("未找到动画生成器: {AnimationType}", animationType);
                return await Task.FromResult(GenerateDefaultAnimationCss(duration, easing));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成动画CSS时发生错误: {AnimationType}", animationType);
                throw;
            }
        }

        /// <summary>
        /// 初始化动画生成器
        /// </summary>
        private Dictionary<string, Func<int, string, string>> InitializeAnimationGenerators()
        {
            return new Dictionary<string, Func<int, string, string>>
            {
                [nameof(AnimationType.FadeIn)] = GenerateFadeInCss,
                [nameof(AnimationType.FadeOut)] = GenerateFadeOutCss,
                [nameof(AnimationType.SlideIn)] = GenerateSlideInCss,
                [nameof(AnimationType.SlideOut)] = GenerateSlideOutCss,
                [nameof(AnimationType.SlideUp)] = GenerateSlideUpCss,
                [nameof(AnimationType.SlideDown)] = GenerateSlideDownCss,
                [nameof(AnimationType.ScaleIn)] = GenerateScaleInCss,
                [nameof(AnimationType.ScaleOut)] = GenerateScaleOutCss,
                [nameof(AnimationType.BounceIn)] = GenerateBounceInCss,
                [nameof(AnimationType.BounceOut)] = GenerateBounceOutCss,
                [nameof(AnimationType.RotateIn)] = GenerateRotateInCss,
                [nameof(AnimationType.RotateOut)] = GenerateRotateOutCss,
                [nameof(AnimationType.Pulse)] = GeneratePulseCss,
                [nameof(AnimationType.Shake)] = GenerateShakeCss
            };
        }

        /// <summary>
        /// 生成淡入动画CSS
        /// </summary>
        private string GenerateFadeInCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("fadeIn", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> { ["opacity"] = "0" },
                ["100%"] = new Dictionary<string, string> { ["opacity"] = "1" }
            });

            return $"{keyframes}\n.fade-in {{\n  animation: fadeIn {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成淡出动画CSS
        /// </summary>
        private string GenerateFadeOutCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("fadeOut", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> { ["opacity"] = "1" },
                ["100%"] = new Dictionary<string, string> { ["opacity"] = "0" }
            });

            return $"{keyframes}\n.fade-out {{\n  animation: fadeOut {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成滑入动画CSS
        /// </summary>
        private string GenerateSlideInCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("slideIn", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "translateX(-100%)",
                    ["opacity"] = "0"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "translateX(0)",
                    ["opacity"] = "1"
                }
            });

            return $"{keyframes}\n.slide-in {{\n  animation: slideIn {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成滑出动画CSS
        /// </summary>
        private string GenerateSlideOutCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("slideOut", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "translateX(0)",
                    ["opacity"] = "1"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "translateX(100%)",
                    ["opacity"] = "0"
                }
            });

            return $"{keyframes}\n.slide-out {{\n  animation: slideOut {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成向上滑动动画CSS
        /// </summary>
        private string GenerateSlideUpCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("slideUp", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "translateY(100%)",
                    ["opacity"] = "0"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "translateY(0)",
                    ["opacity"] = "1"
                }
            });

            return $"{keyframes}\n.slide-up {{\n  animation: slideUp {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成向下滑动动画CSS
        /// </summary>
        private string GenerateSlideDownCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("slideDown", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "translateY(-100%)",
                    ["opacity"] = "0"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "translateY(0)",
                    ["opacity"] = "1"
                }
            });

            return $"{keyframes}\n.slide-down {{\n  animation: slideDown {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成缩放进入动画CSS
        /// </summary>
        private string GenerateScaleInCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("scaleIn", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(0)",
                    ["opacity"] = "0"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(1)",
                    ["opacity"] = "1"
                }
            });

            return $"{keyframes}\n.scale-in {{\n  animation: scaleIn {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成缩放退出动画CSS
        /// </summary>
        private string GenerateScaleOutCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("scaleOut", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(1)",
                    ["opacity"] = "1"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(0)",
                    ["opacity"] = "0"
                }
            });

            return $"{keyframes}\n.scale-out {{\n  animation: scaleOut {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成弹跳进入动画CSS
        /// </summary>
        private string GenerateBounceInCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("bounceIn", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(0.3)",
                    ["opacity"] = "0"
                },
                ["50%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(1.05)",
                    ["opacity"] = "1"
                },
                ["70%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(0.9)"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(1)"
                }
            });

            return $"{keyframes}\n.bounce-in {{\n  animation: bounceIn {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成弹跳退出动画CSS
        /// </summary>
        private string GenerateBounceOutCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("bounceOut", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(1)",
                    ["opacity"] = "1"
                },
                ["25%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(0.95)"
                },
                ["50%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(1.1)"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "scale(0)",
                    ["opacity"] = "0"
                }
            });

            return $"{keyframes}\n.bounce-out {{\n  animation: bounceOut {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成旋转进入动画CSS
        /// </summary>
        private string GenerateRotateInCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("rotateIn", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "rotate(-180deg) scale(0)",
                    ["opacity"] = "0"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "rotate(0deg) scale(1)",
                    ["opacity"] = "1"
                }
            });

            return $"{keyframes}\n.rotate-in {{\n  animation: rotateIn {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成旋转退出动画CSS
        /// </summary>
        private string GenerateRotateOutCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("rotateOut", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "rotate(0deg) scale(1)",
                    ["opacity"] = "1"
                },
                ["100%"] = new Dictionary<string, string> 
                { 
                    ["transform"] = "rotate(180deg) scale(0)",
                    ["opacity"] = "0"
                }
            });

            return $"{keyframes}\n.rotate-out {{\n  animation: rotateOut {duration}ms {easing} forwards;\n}}";
        }

        /// <summary>
        /// 生成脉冲动画CSS
        /// </summary>
        private string GeneratePulseCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("pulse", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%"] = new Dictionary<string, string> { ["transform"] = "scale(1)" },
                ["50%"] = new Dictionary<string, string> { ["transform"] = "scale(1.05)" },
                ["100%"] = new Dictionary<string, string> { ["transform"] = "scale(1)" }
            });

            return $"{keyframes}\n.pulse {{\n  animation: pulse {duration}ms {easing} infinite;\n}}";
        }

        /// <summary>
        /// 生成摇摆动画CSS
        /// </summary>
        private string GenerateShakeCss(int duration, string easing)
        {
            var keyframes = GenerateKeyframeAnimation("shake", new Dictionary<string, Dictionary<string, string>>
            {
                ["0%, 100%"] = new Dictionary<string, string> { ["transform"] = "translateX(0)" },
                ["10%, 30%, 50%, 70%, 90%"] = new Dictionary<string, string> { ["transform"] = "translateX(-10px)" },
                ["20%, 40%, 60%, 80%"] = new Dictionary<string, string> { ["transform"] = "translateX(10px)" }
            });

            return $"{keyframes}\n.shake {{\n  animation: shake {duration}ms {easing};\n}}";
        }

        /// <summary>
        /// 生成默认动画CSS
        /// </summary>
        private string GenerateDefaultAnimationCss(int duration, string easing)
        {
            return $"transition: all {duration}ms {easing};";
        }
    }
}