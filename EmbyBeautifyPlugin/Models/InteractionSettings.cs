using System.Collections.Generic;

namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// 交互设置模型
    /// </summary>
    public class InteractionSettings
    {
        /// <summary>
        /// 是否启用交互增强
        /// </summary>
        public bool EnableInteractionEnhancements { get; set; } = true;

        /// <summary>
        /// 悬停效果配置
        /// </summary>
        public Dictionary<string, HoverEffectConfig> HoverEffects { get; set; } = new Dictionary<string, HoverEffectConfig>();

        /// <summary>
        /// 点击反馈配置
        /// </summary>
        public Dictionary<string, ClickFeedbackConfig> ClickFeedbacks { get; set; } = new Dictionary<string, ClickFeedbackConfig>();

        /// <summary>
        /// 焦点效果配置
        /// </summary>
        public Dictionary<string, FocusEffectConfig> FocusEffects { get; set; } = new Dictionary<string, FocusEffectConfig>();

        /// <summary>
        /// 滚动优化配置
        /// </summary>
        public ScrollOptimizationConfig ScrollOptimization { get; set; } = new ScrollOptimizationConfig();

        /// <summary>
        /// 全局交互设置
        /// </summary>
        public GlobalInteractionConfig GlobalSettings { get; set; } = new GlobalInteractionConfig();
    }

    /// <summary>
    /// 悬停效果配置
    /// </summary>
    public class HoverEffectConfig
    {
        /// <summary>
        /// 过渡持续时间（毫秒）
        /// </summary>
        public int Duration { get; set; } = 200;

        /// <summary>
        /// 缓动函数
        /// </summary>
        public string Easing { get; set; } = "ease";

        /// <summary>
        /// 变换效果
        /// </summary>
        public TransformEffect Transform { get; set; } = new TransformEffect();

        /// <summary>
        /// 颜色变化
        /// </summary>
        public ColorEffect Color { get; set; } = new ColorEffect();

        /// <summary>
        /// 阴影效果
        /// </summary>
        public ShadowEffect Shadow { get; set; } = new ShadowEffect();

        /// <summary>
        /// 透明度变化
        /// </summary>
        public OpacityEffect Opacity { get; set; } = new OpacityEffect();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// 点击反馈配置
    /// </summary>
    public class ClickFeedbackConfig
    {
        /// <summary>
        /// 反馈类型
        /// </summary>
        public ClickFeedbackType Type { get; set; } = ClickFeedbackType.Ripple;

        /// <summary>
        /// 持续时间（毫秒）
        /// </summary>
        public int Duration { get; set; } = 150;

        /// <summary>
        /// 缓动函数
        /// </summary>
        public string Easing { get; set; } = "ease-out";

        /// <summary>
        /// 反馈颜色
        /// </summary>
        public string Color { get; set; } = "rgba(255, 255, 255, 0.3)";

        /// <summary>
        /// 缩放效果
        /// </summary>
        public ScaleEffect Scale { get; set; } = new ScaleEffect();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// 焦点效果配置
    /// </summary>
    public class FocusEffectConfig
    {
        /// <summary>
        /// 过渡持续时间（毫秒）
        /// </summary>
        public int Duration { get; set; } = 150;

        /// <summary>
        /// 缓动函数
        /// </summary>
        public string Easing { get; set; } = "ease";

        /// <summary>
        /// 轮廓样式
        /// </summary>
        public OutlineEffect Outline { get; set; } = new OutlineEffect();

        /// <summary>
        /// 阴影效果
        /// </summary>
        public ShadowEffect Shadow { get; set; } = new ShadowEffect();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// 滚动优化配置
    /// </summary>
    public class ScrollOptimizationConfig
    {
        /// <summary>
        /// 是否启用平滑滚动
        /// </summary>
        public bool EnableSmoothScrolling { get; set; } = true;

        /// <summary>
        /// 滚动行为
        /// </summary>
        public string ScrollBehavior { get; set; } = "smooth";

        /// <summary>
        /// 是否启用滚动条美化
        /// </summary>
        public bool EnableScrollbarStyling { get; set; } = true;

        /// <summary>
        /// 滚动条样式
        /// </summary>
        public ScrollbarStyle ScrollbarStyle { get; set; } = new ScrollbarStyle();

        /// <summary>
        /// 是否启用滚动性能优化
        /// </summary>
        public bool EnableScrollPerformanceOptimization { get; set; } = true;
    }

    /// <summary>
    /// 全局交互配置
    /// </summary>
    public class GlobalInteractionConfig
    {
        /// <summary>
        /// 是否启用硬件加速
        /// </summary>
        public bool EnableHardwareAcceleration { get; set; } = true;

        /// <summary>
        /// 是否启用减少动画模式
        /// </summary>
        public bool RespectReducedMotion { get; set; } = true;

        /// <summary>
        /// 全局过渡持续时间倍数
        /// </summary>
        public double GlobalDurationMultiplier { get; set; } = 1.0;

        /// <summary>
        /// 是否启用触摸优化
        /// </summary>
        public bool EnableTouchOptimization { get; set; } = true;
    }

    /// <summary>
    /// 变换效果
    /// </summary>
    public class TransformEffect
    {
        /// <summary>
        /// 缩放值
        /// </summary>
        public double Scale { get; set; } = 1.05;

        /// <summary>
        /// X轴平移（像素）
        /// </summary>
        public int TranslateX { get; set; } = 0;

        /// <summary>
        /// Y轴平移（像素）
        /// </summary>
        public int TranslateY { get; set; } = -2;

        /// <summary>
        /// 旋转角度（度）
        /// </summary>
        public double Rotate { get; set; } = 0;
    }

    /// <summary>
    /// 颜色效果
    /// </summary>
    public class ColorEffect
    {
        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BackgroundColor { get; set; } = string.Empty;

        /// <summary>
        /// 文字颜色
        /// </summary>
        public string TextColor { get; set; } = string.Empty;

        /// <summary>
        /// 边框颜色
        /// </summary>
        public string BorderColor { get; set; } = string.Empty;
    }

    /// <summary>
    /// 阴影效果
    /// </summary>
    public class ShadowEffect
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 阴影样式
        /// </summary>
        public string BoxShadow { get; set; } = "0 4px 8px rgba(0, 0, 0, 0.1)";

        /// <summary>
        /// 文字阴影
        /// </summary>
        public string TextShadow { get; set; } = string.Empty;
    }

    /// <summary>
    /// 透明度效果
    /// </summary>
    public class OpacityEffect
    {
        /// <summary>
        /// 透明度值（0-1）
        /// </summary>
        public double Value { get; set; } = 0.8;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = false;
    }

    /// <summary>
    /// 缩放效果
    /// </summary>
    public class ScaleEffect
    {
        /// <summary>
        /// 缩放值
        /// </summary>
        public double Value { get; set; } = 0.95;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// 轮廓效果
    /// </summary>
    public class OutlineEffect
    {
        /// <summary>
        /// 轮廓颜色
        /// </summary>
        public string Color { get; set; } = "#007acc";

        /// <summary>
        /// 轮廓宽度（像素）
        /// </summary>
        public int Width { get; set; } = 2;

        /// <summary>
        /// 轮廓样式
        /// </summary>
        public string Style { get; set; } = "solid";

        /// <summary>
        /// 轮廓偏移（像素）
        /// </summary>
        public int Offset { get; set; } = 2;
    }

    /// <summary>
    /// 滚动条样式
    /// </summary>
    public class ScrollbarStyle
    {
        /// <summary>
        /// 滚动条宽度（像素）
        /// </summary>
        public int Width { get; set; } = 8;

        /// <summary>
        /// 滚动条轨道颜色
        /// </summary>
        public string TrackColor { get; set; } = "rgba(0, 0, 0, 0.1)";

        /// <summary>
        /// 滚动条滑块颜色
        /// </summary>
        public string ThumbColor { get; set; } = "rgba(0, 0, 0, 0.3)";

        /// <summary>
        /// 滚动条滑块悬停颜色
        /// </summary>
        public string ThumbHoverColor { get; set; } = "rgba(0, 0, 0, 0.5)";

        /// <summary>
        /// 滚动条圆角（像素）
        /// </summary>
        public int BorderRadius { get; set; } = 4;
    }

    /// <summary>
    /// 点击反馈类型枚举
    /// </summary>
    public enum ClickFeedbackType
    {
        /// <summary>
        /// 无反馈
        /// </summary>
        None,

        /// <summary>
        /// 涟漪效果
        /// </summary>
        Ripple,

        /// <summary>
        /// 缩放效果
        /// </summary>
        Scale,

        /// <summary>
        /// 脉冲效果
        /// </summary>
        Pulse,

        /// <summary>
        /// 闪烁效果
        /// </summary>
        Flash
    }
}