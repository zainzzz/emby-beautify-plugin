using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Extensions;

namespace EmbyBeautifyPlugin.Abstracts
{
    /// <summary>
    /// 用户交互增强抽象基类
    /// </summary>
    public abstract class BaseInteractionEnhancer : IInteractionEnhancer
    {
        protected readonly ILogger<BaseInteractionEnhancer> _logger;

        protected BaseInteractionEnhancer(ILogger<BaseInteractionEnhancer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 生成悬停效果CSS
        /// </summary>
        public abstract Task<string> GenerateHoverEffectCssAsync(string selector, HoverEffectConfig hoverConfig);

        /// <summary>
        /// 生成点击反馈动画CSS
        /// </summary>
        public abstract Task<string> GenerateClickFeedbackCssAsync(string selector, ClickFeedbackConfig clickConfig);

        /// <summary>
        /// 生成滚动优化效果CSS
        /// </summary>
        public virtual async Task<string> GenerateScrollOptimizationCssAsync(ScrollOptimizationConfig scrollConfig)
        {
            using var timer = _logger.BeginOperation("GenerateScrollOptimizationCss");

            try
            {
                var css = new StringBuilder();

                if (scrollConfig.EnableSmoothScrolling)
                {
                    css.AppendLine("html { scroll-behavior: smooth; }");
                }

                if (scrollConfig.EnableScrollbarStyling)
                {
                    css.AppendLine(GenerateScrollbarCss(scrollConfig.ScrollbarStyle));
                }

                if (scrollConfig.EnableScrollPerformanceOptimization)
                {
                    css.AppendLine(GenerateScrollPerformanceCss());
                }

                var result = css.ToString();
                _logger.LogDebug("生成滚动优化CSS，长度: {Length}", result.Length);
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成滚动优化CSS时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 生成焦点效果CSS
        /// </summary>
        public virtual async Task<string> GenerateFocusEffectCssAsync(string selector, FocusEffectConfig focusConfig)
        {
            using var timer = _logger.BeginOperation("GenerateFocusEffectCss", new Dictionary<string, object>
            {
                ["Selector"] = selector
            });

            try
            {
                if (!focusConfig.Enabled)
                {
                    return await Task.FromResult(string.Empty);
                }

                var css = new StringBuilder();
                css.AppendLine($"{selector}:focus {{");
                css.AppendLine($"  transition: all {focusConfig.Duration}ms {focusConfig.Easing};");

                if (focusConfig.Outline.Width > 0)
                {
                    css.AppendLine($"  outline: {focusConfig.Outline.Width}px {focusConfig.Outline.Style} {focusConfig.Outline.Color};");
                    css.AppendLine($"  outline-offset: {focusConfig.Outline.Offset}px;");
                }

                if (focusConfig.Shadow.Enabled && !string.IsNullOrEmpty(focusConfig.Shadow.BoxShadow))
                {
                    css.AppendLine($"  box-shadow: {focusConfig.Shadow.BoxShadow};");
                }

                css.AppendLine("}");

                var result = css.ToString();
                _logger.LogDebug("生成焦点效果CSS: {Selector}", selector);
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成焦点效果CSS时发生错误: {Selector}", selector);
                throw;
            }
        }

        /// <summary>
        /// 生成完整的交互增强CSS
        /// </summary>
        public virtual async Task<string> GenerateInteractionCssAsync(InteractionSettings interactionSettings)
        {
            using var timer = _logger.BeginOperation("GenerateInteractionCss");

            try
            {
                if (!interactionSettings.EnableInteractionEnhancements)
                {
                    _logger.LogDebug("交互增强已禁用，返回空CSS");
                    return await Task.FromResult(string.Empty);
                }

                var css = new StringBuilder();

                // 添加全局设置
                css.AppendLine(GenerateGlobalInteractionCss(interactionSettings.GlobalSettings));

                // 生成悬停效果
                foreach (var hoverEffect in interactionSettings.HoverEffects)
                {
                    var hoverCss = await GenerateHoverEffectCssAsync(hoverEffect.Key, hoverEffect.Value);
                    css.AppendLine(hoverCss);
                }

                // 生成点击反馈
                foreach (var clickFeedback in interactionSettings.ClickFeedbacks)
                {
                    var clickCss = await GenerateClickFeedbackCssAsync(clickFeedback.Key, clickFeedback.Value);
                    css.AppendLine(clickCss);
                }

                // 生成焦点效果
                foreach (var focusEffect in interactionSettings.FocusEffects)
                {
                    var focusCss = await GenerateFocusEffectCssAsync(focusEffect.Key, focusEffect.Value);
                    css.AppendLine(focusCss);
                }

                // 生成滚动优化
                var scrollCss = await GenerateScrollOptimizationCssAsync(interactionSettings.ScrollOptimization);
                css.AppendLine(scrollCss);

                var result = css.ToString();
                _logger.LogDebug("生成完整交互CSS，长度: {Length}", result.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成交互CSS时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 验证交互配置
        /// </summary>
        public virtual async Task<bool> ValidateInteractionSettingsAsync(InteractionSettings interactionSettings)
        {
            using var timer = _logger.BeginOperation("ValidateInteractionSettings");

            try
            {
                if (interactionSettings == null)
                {
                    _logger.LogWarning("交互设置为空");
                    return false;
                }

                // 验证悬停效果配置
                foreach (var hoverEffect in interactionSettings.HoverEffects)
                {
                    if (!ValidateHoverEffectConfig(hoverEffect.Value))
                    {
                        _logger.LogWarning("无效的悬停效果配置: {Selector}", hoverEffect.Key);
                        return false;
                    }
                }

                // 验证点击反馈配置
                foreach (var clickFeedback in interactionSettings.ClickFeedbacks)
                {
                    if (!ValidateClickFeedbackConfig(clickFeedback.Value))
                    {
                        _logger.LogWarning("无效的点击反馈配置: {Selector}", clickFeedback.Key);
                        return false;
                    }
                }

                // 验证全局设置
                if (!ValidateGlobalInteractionConfig(interactionSettings.GlobalSettings))
                {
                    _logger.LogWarning("无效的全局交互配置");
                    return false;
                }

                _logger.LogDebug("交互设置验证通过");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证交互设置时发生错误");
                return false;
            }
        }

        /// <summary>
        /// 获取默认交互设置
        /// </summary>
        public virtual async Task<InteractionSettings> GetDefaultInteractionSettingsAsync()
        {
            using var timer = _logger.BeginOperation("GetDefaultInteractionSettings");

            try
            {
                var settings = new InteractionSettings
                {
                    EnableInteractionEnhancements = true,
                    HoverEffects = GetDefaultHoverEffects(),
                    ClickFeedbacks = GetDefaultClickFeedbacks(),
                    FocusEffects = GetDefaultFocusEffects(),
                    ScrollOptimization = GetDefaultScrollOptimization(),
                    GlobalSettings = GetDefaultGlobalSettings()
                };

                _logger.LogDebug("获取默认交互设置");
                return await Task.FromResult(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取默认交互设置时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 生成滚动条CSS
        /// </summary>
        protected virtual string GenerateScrollbarCss(ScrollbarStyle style)
        {
            var css = new StringBuilder();

            // Webkit滚动条样式
            css.AppendLine("::-webkit-scrollbar {");
            css.AppendLine($"  width: {style.Width}px;");
            css.AppendLine($"  height: {style.Width}px;");
            css.AppendLine("}");

            css.AppendLine("::-webkit-scrollbar-track {");
            css.AppendLine($"  background: {style.TrackColor};");
            css.AppendLine($"  border-radius: {style.BorderRadius}px;");
            css.AppendLine("}");

            css.AppendLine("::-webkit-scrollbar-thumb {");
            css.AppendLine($"  background: {style.ThumbColor};");
            css.AppendLine($"  border-radius: {style.BorderRadius}px;");
            css.AppendLine("}");

            css.AppendLine("::-webkit-scrollbar-thumb:hover {");
            css.AppendLine($"  background: {style.ThumbHoverColor};");
            css.AppendLine("}");

            // Firefox滚动条样式
            css.AppendLine("* {");
            css.AppendLine("  scrollbar-width: thin;");
            css.AppendLine($"  scrollbar-color: {style.ThumbColor} {style.TrackColor};");
            css.AppendLine("}");

            return css.ToString();
        }

        /// <summary>
        /// 生成滚动性能优化CSS
        /// </summary>
        protected virtual string GenerateScrollPerformanceCss()
        {
            return @"
* {
  -webkit-overflow-scrolling: touch;
  scroll-behavior: smooth;
}

.scroll-container {
  will-change: scroll-position;
  transform: translateZ(0);
}";
        }

        /// <summary>
        /// 生成全局交互CSS
        /// </summary>
        protected virtual string GenerateGlobalInteractionCss(GlobalInteractionConfig globalConfig)
        {
            var css = new StringBuilder();

            if (globalConfig.RespectReducedMotion)
            {
                css.AppendLine("@media (prefers-reduced-motion: reduce) {");
                css.AppendLine("  *, *::before, *::after {");
                css.AppendLine("    animation-duration: 0.01ms !important;");
                css.AppendLine("    animation-iteration-count: 1 !important;");
                css.AppendLine("    transition-duration: 0.01ms !important;");
                css.AppendLine("  }");
                css.AppendLine("}");
            }

            if (globalConfig.EnableHardwareAcceleration)
            {
                css.AppendLine(".interactive-element {");
                css.AppendLine("  transform: translateZ(0);");
                css.AppendLine("  will-change: transform, opacity;");
                css.AppendLine("  backface-visibility: hidden;");
                css.AppendLine("}");
            }

            if (globalConfig.EnableTouchOptimization)
            {
                css.AppendLine(".touch-target {");
                css.AppendLine("  min-height: 44px;");
                css.AppendLine("  min-width: 44px;");
                css.AppendLine("  touch-action: manipulation;");
                css.AppendLine("}");
            }

            return css.ToString();
        }

        /// <summary>
        /// 验证悬停效果配置
        /// </summary>
        protected virtual bool ValidateHoverEffectConfig(HoverEffectConfig config)
        {
            return config != null && 
                   config.Duration >= 0 && config.Duration <= 5000 &&
                   !string.IsNullOrEmpty(config.Easing);
        }

        /// <summary>
        /// 验证点击反馈配置
        /// </summary>
        protected virtual bool ValidateClickFeedbackConfig(ClickFeedbackConfig config)
        {
            return config != null && 
                   config.Duration >= 0 && config.Duration <= 2000 &&
                   !string.IsNullOrEmpty(config.Easing);
        }

        /// <summary>
        /// 验证全局交互配置
        /// </summary>
        protected virtual bool ValidateGlobalInteractionConfig(GlobalInteractionConfig config)
        {
            return config != null && 
                   config.GlobalDurationMultiplier > 0 && config.GlobalDurationMultiplier <= 5.0;
        }

        /// <summary>
        /// 获取默认悬停效果
        /// </summary>
        protected virtual Dictionary<string, HoverEffectConfig> GetDefaultHoverEffects()
        {
            return new Dictionary<string, HoverEffectConfig>
            {
                [".card, .media-item"] = new HoverEffectConfig
                {
                    Duration = 200,
                    Transform = new TransformEffect { Scale = 1.02, TranslateY = -2 },
                    Shadow = new ShadowEffect { BoxShadow = "0 8px 16px rgba(0, 0, 0, 0.15)" }
                },
                ["button, .button"] = new HoverEffectConfig
                {
                    Duration = 150,
                    Transform = new TransformEffect { Scale = 1.05 },
                    Color = new ColorEffect { BackgroundColor = "rgba(255, 255, 255, 0.1)" }
                }
            };
        }

        /// <summary>
        /// 获取默认点击反馈
        /// </summary>
        protected virtual Dictionary<string, ClickFeedbackConfig> GetDefaultClickFeedbacks()
        {
            return new Dictionary<string, ClickFeedbackConfig>
            {
                ["button, .button"] = new ClickFeedbackConfig
                {
                    Type = ClickFeedbackType.Scale,
                    Duration = 150,
                    Scale = new ScaleEffect { Value = 0.95 }
                },
                [".card, .media-item"] = new ClickFeedbackConfig
                {
                    Type = ClickFeedbackType.Ripple,
                    Duration = 300,
                    Color = "rgba(255, 255, 255, 0.3)"
                }
            };
        }

        /// <summary>
        /// 获取默认焦点效果
        /// </summary>
        protected virtual Dictionary<string, FocusEffectConfig> GetDefaultFocusEffects()
        {
            return new Dictionary<string, FocusEffectConfig>
            {
                ["button, input, select, textarea, [tabindex]"] = new FocusEffectConfig
                {
                    Duration = 150,
                    Outline = new OutlineEffect
                    {
                        Color = "#007acc",
                        Width = 2,
                        Offset = 2
                    },
                    Shadow = new ShadowEffect
                    {
                        BoxShadow = "0 0 0 3px rgba(0, 122, 204, 0.3)"
                    }
                }
            };
        }

        /// <summary>
        /// 获取默认滚动优化
        /// </summary>
        protected virtual ScrollOptimizationConfig GetDefaultScrollOptimization()
        {
            return new ScrollOptimizationConfig
            {
                EnableSmoothScrolling = true,
                EnableScrollbarStyling = true,
                EnableScrollPerformanceOptimization = true,
                ScrollbarStyle = new ScrollbarStyle
                {
                    Width = 8,
                    TrackColor = "rgba(0, 0, 0, 0.1)",
                    ThumbColor = "rgba(0, 0, 0, 0.3)",
                    ThumbHoverColor = "rgba(0, 0, 0, 0.5)",
                    BorderRadius = 4
                }
            };
        }

        /// <summary>
        /// 获取默认全局设置
        /// </summary>
        protected virtual GlobalInteractionConfig GetDefaultGlobalSettings()
        {
            return new GlobalInteractionConfig
            {
                EnableHardwareAcceleration = true,
                RespectReducedMotion = true,
                GlobalDurationMultiplier = 1.0,
                EnableTouchOptimization = true
            };
        }
    }
}