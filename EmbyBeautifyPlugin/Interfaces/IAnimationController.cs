using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmbyBeautifyPlugin.Models;

namespace EmbyBeautifyPlugin.Interfaces
{
    /// <summary>
    /// 动画控制器接口，负责管理动画效果和过渡动画
    /// </summary>
    public interface IAnimationController
    {
        /// <summary>
        /// 获取可用的动画效果列表
        /// </summary>
        /// <returns>动画效果名称列表</returns>
        Task<IEnumerable<string>> GetAvailableAnimationsAsync();

        /// <summary>
        /// 生成指定动画的CSS代码
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="duration">动画持续时间（毫秒）</param>
        /// <param name="easing">缓动函数</param>
        /// <returns>动画CSS代码</returns>
        Task<string> GenerateAnimationCssAsync(string animationType, int duration = 300, string easing = "ease");

        /// <summary>
        /// 生成过渡动画CSS
        /// </summary>
        /// <param name="properties">需要过渡的CSS属性</param>
        /// <param name="duration">过渡持续时间（毫秒）</param>
        /// <param name="easing">缓动函数</param>
        /// <returns>过渡CSS代码</returns>
        Task<string> GenerateTransitionCssAsync(IEnumerable<string> properties, int duration = 300, string easing = "ease");

        /// <summary>
        /// 获取性能优化的动画CSS
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="enableHardwareAcceleration">是否启用硬件加速</param>
        /// <returns>优化后的动画CSS</returns>
        Task<string> GetOptimizedAnimationCssAsync(string animationType, bool enableHardwareAcceleration = true);

        /// <summary>
        /// 验证动画配置是否有效
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="duration">持续时间</param>
        /// <returns>是否有效</returns>
        Task<bool> ValidateAnimationConfigAsync(string animationType, int duration);

        /// <summary>
        /// 获取动画的默认配置
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <returns>默认配置</returns>
        Task<AnimationConfig> GetDefaultAnimationConfigAsync(string animationType);
    }
}