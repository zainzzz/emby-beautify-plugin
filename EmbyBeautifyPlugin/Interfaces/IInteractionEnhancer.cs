using System.Collections.Generic;
using System.Threading.Tasks;
using EmbyBeautifyPlugin.Models;

namespace EmbyBeautifyPlugin.Interfaces
{
    /// <summary>
    /// 用户交互增强接口，负责处理悬停、点击和滚动效果
    /// </summary>
    public interface IInteractionEnhancer
    {
        /// <summary>
        /// 生成悬停效果CSS
        /// </summary>
        /// <param name="selector">CSS选择器</param>
        /// <param name="hoverConfig">悬停配置</param>
        /// <returns>悬停效果CSS</returns>
        Task<string> GenerateHoverEffectCssAsync(string selector, HoverEffectConfig hoverConfig);

        /// <summary>
        /// 生成点击反馈动画CSS
        /// </summary>
        /// <param name="selector">CSS选择器</param>
        /// <param name="clickConfig">点击配置</param>
        /// <returns>点击反馈CSS</returns>
        Task<string> GenerateClickFeedbackCssAsync(string selector, ClickFeedbackConfig clickConfig);

        /// <summary>
        /// 生成滚动优化效果CSS
        /// </summary>
        /// <param name="scrollConfig">滚动配置</param>
        /// <returns>滚动优化CSS</returns>
        Task<string> GenerateScrollOptimizationCssAsync(ScrollOptimizationConfig scrollConfig);

        /// <summary>
        /// 生成焦点效果CSS
        /// </summary>
        /// <param name="selector">CSS选择器</param>
        /// <param name="focusConfig">焦点配置</param>
        /// <returns>焦点效果CSS</returns>
        Task<string> GenerateFocusEffectCssAsync(string selector, FocusEffectConfig focusConfig);

        /// <summary>
        /// 生成完整的交互增强CSS
        /// </summary>
        /// <param name="interactionSettings">交互设置</param>
        /// <returns>完整的交互CSS</returns>
        Task<string> GenerateInteractionCssAsync(InteractionSettings interactionSettings);

        /// <summary>
        /// 验证交互配置
        /// </summary>
        /// <param name="interactionSettings">交互设置</param>
        /// <returns>验证结果</returns>
        Task<bool> ValidateInteractionSettingsAsync(InteractionSettings interactionSettings);

        /// <summary>
        /// 获取默认交互设置
        /// </summary>
        /// <returns>默认交互设置</returns>
        Task<InteractionSettings> GetDefaultInteractionSettingsAsync();
    }
}