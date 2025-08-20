namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// Responsive settings for different screen sizes
    /// </summary>
    public class ResponsiveSettings
    {
        /// <summary>
        /// Settings for desktop devices
        /// </summary>
        public BreakpointSettings Desktop { get; set; }

        /// <summary>
        /// Settings for tablet devices
        /// </summary>
        public BreakpointSettings Tablet { get; set; }

        /// <summary>
        /// Settings for mobile devices
        /// </summary>
        public BreakpointSettings Mobile { get; set; }

        public ResponsiveSettings()
        {
            // 默认不初始化断点设置，让它们为null
            // 这样可以选择性地生成CSS
        }
    }
}