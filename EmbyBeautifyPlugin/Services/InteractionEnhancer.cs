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
    /// 用户交互增强实现类
    /// </summary>
    public class InteractionEnhancer : BaseInteractionEnhancer
    {
        public InteractionEnhancer(ILogger<InteractionEnhancer> logger) : base(logger)
        {
        }

        /// <summary>
        /// 生成悬停效果CSS
        /// </summary>
        public override async Task<string> GenerateHoverEffectCssAsync(string selector, HoverEffectConfig hoverConfig)
        {
            using var timer = _logger.BeginOperation("GenerateHoverEffectCss", new Dictionary<string, object>
            {
                ["Selector"] = selector
            });

            try
            {
                if (!hoverConfig.Enabled)
                {
                    return await Task.FromResult(string.Empty);
                }

                var css = new StringBuilder();

                // 基础过渡效果
                css.AppendLine($"{selector} {{");
                css.AppendLine($"  transition: all {hoverConfig.Duration}ms {hoverConfig.Easing};");
                css.AppendLine("}");

                // 悬停状态
                css.AppendLine($"{selector}:hover {{");

                // 变换效果
                if (hoverConfig.Transform != null)
                {
                    var transforms = new List<string>();
                    
                    if (hoverConfig.Transform.Scale != 1.0)
                    {
                        transforms.Add($"scale({hoverConfig.Transform.Scale})");
                    }
                    
                    if (hoverConfig.Transform.TranslateX != 0 || hoverConfig.Transform.TranslateY != 0)
                    {
                        transforms.Add($"translate({hoverConfig.Transform.TranslateX}px, {hoverConfig.Transform.TranslateY}px)");
                    }
                    
                    if (hoverConfig.Transform.Rotate != 0)
                    {
                        transforms.Add($"rotate({hoverConfig.Transform.Rotate}deg)");
                    }

                    if (transforms.Count > 0)
                    {
                        css.AppendLine($"  transform: {string.Join(" ", transforms)};");
                    }
                }

                // 颜色效果
                if (hoverConfig.Color != null)
                {
                    if (!string.IsNullOrEmpty(hoverConfig.Color.BackgroundColor))
                    {
                        css.AppendLine($"  background-color: {hoverConfig.Color.BackgroundColor};");
                    }
                    
                    if (!string.IsNullOrEmpty(hoverConfig.Color.TextColor))
                    {
                        css.AppendLine($"  color: {hoverConfig.Color.TextColor};");
                    }
                    
                    if (!string.IsNullOrEmpty(hoverConfig.Color.BorderColor))
                    {
                        css.AppendLine($"  border-color: {hoverConfig.Color.BorderColor};");
                    }
                }

                // 阴影效果
                if (hoverConfig.Shadow?.Enabled == true && !string.IsNullOrEmpty(hoverConfig.Shadow.BoxShadow))
                {
                    css.AppendLine($"  box-shadow: {hoverConfig.Shadow.BoxShadow};");
                }

                // 透明度效果
                if (hoverConfig.Opacity?.Enabled == true)
                {
                    css.AppendLine($"  opacity: {hoverConfig.Opacity.Value};");
                }

                css.AppendLine("}");

                var result = css.ToString();
                _logger.LogDebug("生成悬停效果CSS: {Selector}, 长度: {Length}", selector, result.Length);
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成悬停效果CSS时发生错误: {Selector}", selector);
                throw;
            }
        }

        /// <summary>
        /// 生成点击反馈动画CSS
        /// </summary>
        public override async Task<string> GenerateClickFeedbackCssAsync(string selector, ClickFeedbackConfig clickConfig)
        {
            using var timer = _logger.BeginOperation("GenerateClickFeedbackCss", new Dictionary<string, object>
            {
                ["Selector"] = selector,
                ["Type"] = clickConfig.Type.ToString()
            });

            try
            {
                if (!clickConfig.Enabled)
                {
                    return await Task.FromResult(string.Empty);
                }

                var css = new StringBuilder();

                switch (clickConfig.Type)
                {
                    case ClickFeedbackType.Scale:
                        css.AppendLine(GenerateScaleClickFeedback(selector, clickConfig));
                        break;
                    case ClickFeedbackType.Ripple:
                        css.AppendLine(GenerateRippleClickFeedback(selector, clickConfig));
                        break;
                    case ClickFeedbackType.Pulse:
                        css.AppendLine(GeneratePulseClickFeedback(selector, clickConfig));
                        break;
                    case ClickFeedbackType.Flash:
                        css.AppendLine(GenerateFlashClickFeedback(selector, clickConfig));
                        break;
                    case ClickFeedbackType.None:
                    default:
                        return await Task.FromResult(string.Empty);
                }

                var result = css.ToString();
                _logger.LogDebug("生成点击反馈CSS: {Selector}, 类型: {Type}, 长度: {Length}", 
                    selector, clickConfig.Type, result.Length);
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成点击反馈CSS时发生错误: {Selector}", selector);
                throw;
            }
        }

        /// <summary>
        /// 生成缩放点击反馈
        /// </summary>
        private string GenerateScaleClickFeedback(string selector, ClickFeedbackConfig config)
        {
            var css = new StringBuilder();

            css.AppendLine($"{selector} {{");
            css.AppendLine($"  transition: transform {config.Duration}ms {config.Easing};");
            css.AppendLine("}");

            css.AppendLine($"{selector}:active {{");
            if (config.Scale?.Enabled == true)
            {
                css.AppendLine($"  transform: scale({config.Scale.Value});");
            }
            css.AppendLine("}");

            return css.ToString();
        }

        /// <summary>
        /// 生成涟漪点击反馈
        /// </summary>
        private string GenerateRippleClickFeedback(string selector, ClickFeedbackConfig config)
        {
            var css = new StringBuilder();

            // 涟漪容器
            css.AppendLine($"{selector} {{");
            css.AppendLine("  position: relative;");
            css.AppendLine("  overflow: hidden;");
            css.AppendLine("}");

            // 涟漪效果关键帧
            css.AppendLine("@keyframes ripple {");
            css.AppendLine("  0% {");
            css.AppendLine("    transform: scale(0);");
            css.AppendLine("    opacity: 1;");
            css.AppendLine("  }");
            css.AppendLine("  100% {");
            css.AppendLine("    transform: scale(4);");
            css.AppendLine("    opacity: 0;");
            css.AppendLine("  }");
            css.AppendLine("}");

            // 涟漪元素
            css.AppendLine($"{selector}::before {{");
            css.AppendLine("  content: '';");
            css.AppendLine("  position: absolute;");
            css.AppendLine("  top: 50%;");
            css.AppendLine("  left: 50%;");
            css.AppendLine("  width: 0;");
            css.AppendLine("  height: 0;");
            css.AppendLine($"  background: {config.Color};");
            css.AppendLine("  border-radius: 50%;");
            css.AppendLine("  transform: translate(-50%, -50%);");
            css.AppendLine("  transition: width 0.6s, height 0.6s;");
            css.AppendLine("}");

            css.AppendLine($"{selector}:active::before {{");
            css.AppendLine("  width: 300px;");
            css.AppendLine("  height: 300px;");
            css.AppendLine($"  animation: ripple {config.Duration}ms {config.Easing};");
            css.AppendLine("}");

            return css.ToString();
        }

        /// <summary>
        /// 生成脉冲点击反馈
        /// </summary>
        private string GeneratePulseClickFeedback(string selector, ClickFeedbackConfig config)
        {
            var css = new StringBuilder();

            // 脉冲关键帧
            css.AppendLine("@keyframes pulse {");
            css.AppendLine("  0% {");
            css.AppendLine("    box-shadow: 0 0 0 0 rgba(255, 255, 255, 0.7);");
            css.AppendLine("  }");
            css.AppendLine("  70% {");
            css.AppendLine("    box-shadow: 0 0 0 10px rgba(255, 255, 255, 0);");
            css.AppendLine("  }");
            css.AppendLine("  100% {");
            css.AppendLine("    box-shadow: 0 0 0 0 rgba(255, 255, 255, 0);");
            css.AppendLine("  }");
            css.AppendLine("}");

            css.AppendLine($"{selector}:active {{");
            css.AppendLine($"  animation: pulse {config.Duration}ms {config.Easing};");
            css.AppendLine("}");

            return css.ToString();
        }

        /// <summary>
        /// 生成闪烁点击反馈
        /// </summary>
        private string GenerateFlashClickFeedback(string selector, ClickFeedbackConfig config)
        {
            var css = new StringBuilder();

            // 闪烁关键帧
            css.AppendLine("@keyframes flash {");
            css.AppendLine("  0%, 50%, 100% {");
            css.AppendLine("    opacity: 1;");
            css.AppendLine("  }");
            css.AppendLine("  25%, 75% {");
            css.AppendLine("    opacity: 0.5;");
            css.AppendLine("  }");
            css.AppendLine("}");

            css.AppendLine($"{selector}:active {{");
            css.AppendLine($"  animation: flash {config.Duration}ms {config.Easing};");
            css.AppendLine("}");

            return css.ToString();
        }
    }
}