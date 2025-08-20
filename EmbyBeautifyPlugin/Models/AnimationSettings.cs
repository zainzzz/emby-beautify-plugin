using System.Collections.Generic;

namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// 动画配置模型
    /// </summary>
    public class AnimationConfig
    {
        /// <summary>
        /// 动画类型
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 持续时间（毫秒）
        /// </summary>
        public int Duration { get; set; } = 300;

        /// <summary>
        /// 缓动函数
        /// </summary>
        public string Easing { get; set; } = "ease";

        /// <summary>
        /// 延迟时间（毫秒）
        /// </summary>
        public int Delay { get; set; } = 0;

        /// <summary>
        /// 是否启用硬件加速
        /// </summary>
        public bool EnableHardwareAcceleration { get; set; } = true;

        /// <summary>
        /// 自定义CSS属性
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
    }
    /// <summary>
    /// 动画设置模型
    /// </summary>
    public class AnimationSettings
    {
        /// <summary>
        /// 是否启用动画
        /// </summary>
        public bool EnableAnimations { get; set; } = true;

        /// <summary>
        /// 全局动画持续时间（毫秒）
        /// </summary>
        public int GlobalDuration { get; set; } = 300;

        /// <summary>
        /// 全局缓动函数
        /// </summary>
        public string GlobalEasing { get; set; } = "ease";

        /// <summary>
        /// 是否启用硬件加速
        /// </summary>
        public bool EnableHardwareAcceleration { get; set; } = true;

        /// <summary>
        /// 是否启用减少动画模式（用于性能优化）
        /// </summary>
        public bool ReducedMotion { get; set; } = false;

        /// <summary>
        /// 特定动画配置
        /// </summary>
        public Dictionary<string, AnimationConfig> SpecificAnimations { get; set; } = new Dictionary<string, AnimationConfig>();

        /// <summary>
        /// 过渡动画配置
        /// </summary>
        public TransitionSettings Transitions { get; set; } = new TransitionSettings();
    }

    /// <summary>
    /// 过渡动画设置
    /// </summary>
    public class TransitionSettings
    {
        /// <summary>
        /// 悬停效果过渡时间
        /// </summary>
        public int HoverDuration { get; set; } = 200;

        /// <summary>
        /// 点击效果过渡时间
        /// </summary>
        public int ClickDuration { get; set; } = 150;

        /// <summary>
        /// 页面切换过渡时间
        /// </summary>
        public int PageTransitionDuration { get; set; } = 400;

        /// <summary>
        /// 模态框动画时间
        /// </summary>
        public int ModalDuration { get; set; } = 250;

        /// <summary>
        /// 滚动动画时间
        /// </summary>
        public int ScrollDuration { get; set; } = 300;
    }

    /// <summary>
    /// 动画类型枚举
    /// </summary>
    public enum AnimationType
    {
        /// <summary>
        /// 淡入淡出
        /// </summary>
        FadeIn,
        FadeOut,

        /// <summary>
        /// 滑动
        /// </summary>
        SlideIn,
        SlideOut,
        SlideUp,
        SlideDown,

        /// <summary>
        /// 缩放
        /// </summary>
        ScaleIn,
        ScaleOut,

        /// <summary>
        /// 弹跳
        /// </summary>
        BounceIn,
        BounceOut,

        /// <summary>
        /// 旋转
        /// </summary>
        RotateIn,
        RotateOut,

        /// <summary>
        /// 脉冲
        /// </summary>
        Pulse,

        /// <summary>
        /// 摇摆
        /// </summary>
        Shake,

        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }

    /// <summary>
    /// 缓动函数类型
    /// </summary>
    public static class EasingFunctions
    {
        public const string Linear = "linear";
        public const string Ease = "ease";
        public const string EaseIn = "ease-in";
        public const string EaseOut = "ease-out";
        public const string EaseInOut = "ease-in-out";
        public const string EaseInQuad = "cubic-bezier(0.55, 0.085, 0.68, 0.53)";
        public const string EaseOutQuad = "cubic-bezier(0.25, 0.46, 0.45, 0.94)";
        public const string EaseInOutQuad = "cubic-bezier(0.455, 0.03, 0.515, 0.955)";
        public const string EaseInCubic = "cubic-bezier(0.55, 0.055, 0.675, 0.19)";
        public const string EaseOutCubic = "cubic-bezier(0.215, 0.61, 0.355, 1)";
        public const string EaseInOutCubic = "cubic-bezier(0.645, 0.045, 0.355, 1)";
    }
}