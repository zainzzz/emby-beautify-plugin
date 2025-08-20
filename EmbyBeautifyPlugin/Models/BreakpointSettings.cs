namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// Settings for a specific breakpoint/screen size
    /// </summary>
    public class BreakpointSettings
    {
        /// <summary>
        /// Minimum width for this breakpoint
        /// </summary>
        public int MinWidth { get; set; }

        /// <summary>
        /// Maximum width for this breakpoint
        /// </summary>
        public int MaxWidth { get; set; }

        /// <summary>
        /// Number of columns in the grid layout
        /// </summary>
        public int GridColumns { get; set; }

        /// <summary>
        /// Gap between grid items
        /// </summary>
        public string GridGap { get; set; }

        /// <summary>
        /// Font size scaling factor
        /// </summary>
        public double FontScale { get; set; }

        public BreakpointSettings()
        {
            GridColumns = 4;
            GridGap = "1rem";
            FontScale = 1.0;
        }
    }
}