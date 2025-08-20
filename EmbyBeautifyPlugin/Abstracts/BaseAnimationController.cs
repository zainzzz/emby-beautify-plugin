using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Extensions;

namespace EmbyBeautifyPlugin.Abstracts
{
    /// <summary>
    /// 动画控制器抽象基类
    /// </summary>
    public abstract class BaseAnimationController : IAnimationController
    {
        protected readonly ILogger<BaseAnimationController> _logger;
        protected readonly Dictionary<string, AnimationConfig> _defaultConfigs;

        protected BaseAnimationController(ILogger<BaseAnimationController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultConfigs = InitializeDefaultConfigs();
        }

        /// <summary>
        /// 获取可用的动画效果列表
        /// </summary>
        public virtual async Task<IEnumerable<string>> GetAvailableAnimationsAsync()
        {
            using var timer = _logger.BeginOperation("GetAvailableAnimations");

            try
            {
                var animations = Enum.GetNames(typeof(AnimationType))
                    .Where(name => name != nameof(AnimationType.Custom))
                    .ToList();

                _logger.LogDebug("获取到 {Count} 个可用动画", animations.Count);
                return await Task.FromResult(animations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取可用动画列表时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 生成指定动画的CSS代码
        /// </summary>
        public abstract Task<string> GenerateAnimationCssAsync(string animationType, int duration = 300, string easing = "ease");

        /// <summary>
        /// 生成过渡动画CSS
        /// </summary>
        public virtual async Task<string> GenerateTransitionCssAsync(IEnumerable<string> properties, int duration = 300, string easing = "ease")
        {
            using var timer = _logger.BeginOperation("GenerateTransitionCss", new Dictionary<string, object>
            {
                ["Properties"] = properties,
                ["Duration"] = duration,
                ["Easing"] = easing
            });

            try
            {
                if (properties == null || !properties.Any())
                {
                    _logger.LogWarning("过渡属性列表为空，返回默认过渡CSS");
                    return "transition: all 0.3s ease;";
                }

                var transitionProperties = properties.Select(prop => $"{prop} {duration}ms {easing}");
                var css = $"transition: {string.Join(", ", transitionProperties)};";

                _logger.LogDebug("生成过渡CSS: {Css}", css);
                return await Task.FromResult(css);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成过渡CSS时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 获取性能优化的动画CSS
        /// </summary>
        public virtual async Task<string> GetOptimizedAnimationCssAsync(string animationType, bool enableHardwareAcceleration = true)
        {
            using var timer = _logger.BeginOperation("GetOptimizedAnimationCss", new Dictionary<string, object>
            {
                ["AnimationType"] = animationType,
                ["EnableHardwareAcceleration"] = enableHardwareAcceleration
            });

            try
            {
                var baseCss = await GenerateAnimationCssAsync(animationType);
                
                if (enableHardwareAcceleration)
                {
                    // 添加硬件加速属性
                    baseCss += "\ntransform: translateZ(0);";
                    baseCss += "\nwill-change: transform, opacity;";
                    baseCss += "\nbackface-visibility: hidden;";
                }

                // 添加性能优化属性
                baseCss += "\npointer-events: none;"; // 动画期间禁用指针事件

                _logger.LogDebug("生成优化后的动画CSS，长度: {Length}", baseCss.Length);
                return baseCss;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成优化动画CSS时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 验证动画配置是否有效
        /// </summary>
        public virtual async Task<bool> ValidateAnimationConfigAsync(string animationType, int duration)
        {
            using var timer = _logger.BeginOperation("ValidateAnimationConfig", new Dictionary<string, object>
            {
                ["AnimationType"] = animationType,
                ["Duration"] = duration
            });

            try
            {
                // 验证动画类型
                if (string.IsNullOrWhiteSpace(animationType))
                {
                    _logger.LogWarning("动画类型不能为空");
                    return false;
                }

                // 验证持续时间
                if (duration < 0 || duration > 10000) // 最大10秒
                {
                    _logger.LogWarning("动画持续时间无效: {Duration}ms", duration);
                    return false;
                }

                // 验证动画类型是否存在
                var availableAnimations = await GetAvailableAnimationsAsync();
                if (!availableAnimations.Contains(animationType, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("不支持的动画类型: {AnimationType}", animationType);
                    return false;
                }

                _logger.LogDebug("动画配置验证通过: {AnimationType}, {Duration}ms", animationType, duration);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证动画配置时发生错误");
                return false;
            }
        }

        /// <summary>
        /// 获取动画的默认配置
        /// </summary>
        public virtual async Task<AnimationConfig> GetDefaultAnimationConfigAsync(string animationType)
        {
            using var timer = _logger.BeginOperation("GetDefaultAnimationConfig", new Dictionary<string, object>
            {
                ["AnimationType"] = animationType
            });

            try
            {
                if (_defaultConfigs.TryGetValue(animationType, out var config))
                {
                    _logger.LogDebug("获取到默认配置: {AnimationType}", animationType);
                    return await Task.FromResult(config);
                }

                // 返回通用默认配置
                var defaultConfig = new AnimationConfig
                {
                    Type = animationType,
                    Duration = 300,
                    Easing = EasingFunctions.Ease,
                    EnableHardwareAcceleration = true
                };

                _logger.LogDebug("使用通用默认配置: {AnimationType}", animationType);
                return await Task.FromResult(defaultConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取默认动画配置时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 初始化默认配置
        /// </summary>
        protected virtual Dictionary<string, AnimationConfig> InitializeDefaultConfigs()
        {
            return new Dictionary<string, AnimationConfig>
            {
                [nameof(AnimationType.FadeIn)] = new AnimationConfig
                {
                    Type = nameof(AnimationType.FadeIn),
                    Duration = 300,
                    Easing = EasingFunctions.EaseOut
                },
                [nameof(AnimationType.FadeOut)] = new AnimationConfig
                {
                    Type = nameof(AnimationType.FadeOut),
                    Duration = 200,
                    Easing = EasingFunctions.EaseIn
                },
                [nameof(AnimationType.SlideIn)] = new AnimationConfig
                {
                    Type = nameof(AnimationType.SlideIn),
                    Duration = 400,
                    Easing = EasingFunctions.EaseOutCubic
                },
                [nameof(AnimationType.SlideOut)] = new AnimationConfig
                {
                    Type = nameof(AnimationType.SlideOut),
                    Duration = 300,
                    Easing = EasingFunctions.EaseInCubic
                },
                [nameof(AnimationType.ScaleIn)] = new AnimationConfig
                {
                    Type = nameof(AnimationType.ScaleIn),
                    Duration = 250,
                    Easing = EasingFunctions.EaseOutQuad
                },
                [nameof(AnimationType.ScaleOut)] = new AnimationConfig
                {
                    Type = nameof(AnimationType.ScaleOut),
                    Duration = 200,
                    Easing = EasingFunctions.EaseInQuad
                },
                [nameof(AnimationType.Pulse)] = new AnimationConfig
                {
                    Type = nameof(AnimationType.Pulse),
                    Duration = 1000,
                    Easing = EasingFunctions.EaseInOut
                }
            };
        }

        /// <summary>
        /// 生成CSS关键帧动画
        /// </summary>
        protected virtual string GenerateKeyframeAnimation(string name, Dictionary<string, Dictionary<string, string>> keyframes)
        {
            var css = $"@keyframes {name} {{\n";
            
            foreach (var keyframe in keyframes)
            {
                css += $"  {keyframe.Key} {{\n";
                foreach (var property in keyframe.Value)
                {
                    css += $"    {property.Key}: {property.Value};\n";
                }
                css += "  }\n";
            }
            
            css += "}";
            return css;
        }

        /// <summary>
        /// 应用性能优化
        /// </summary>
        protected virtual string ApplyPerformanceOptimizations(string css, bool enableHardwareAcceleration)
        {
            if (enableHardwareAcceleration)
            {
                // 添加硬件加速相关的CSS属性
                css += "\ntransform: translateZ(0);";
                css += "\nwill-change: transform, opacity;";
                css += "\nbackface-visibility: hidden;";
            }

            return css;
        }
    }
}