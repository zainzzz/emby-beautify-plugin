using System.Collections.Generic;

namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// Main configuration class for the beautify plugin
    /// </summary>
    public class BeautifyConfig
    {
        /// <summary>
        /// ID of the currently active theme
        /// </summary>
        public string ActiveThemeId { get; set; }

        /// <summary>
        /// Whether animations are enabled
        /// </summary>
        public bool EnableAnimations { get; set; }

        /// <summary>
        /// Whether custom fonts are enabled
        /// </summary>
        public bool EnableCustomFonts { get; set; }

        /// <summary>
        /// Duration of animations in milliseconds
        /// </summary>
        public int AnimationDuration { get; set; }

        /// <summary>
        /// Responsive settings for different screen sizes
        /// </summary>
        public ResponsiveSettings ResponsiveSettings { get; set; }

        /// <summary>
        /// Custom settings dictionary for extensibility
        /// </summary>
        public Dictionary<string, object> CustomSettings { get; set; }

        public BeautifyConfig()
        {
            ActiveThemeId = "default";
            EnableAnimations = true;
            EnableCustomFonts = true;
            AnimationDuration = 300;
            ResponsiveSettings = new ResponsiveSettings();
            CustomSettings = new Dictionary<string, object>();
        }
    }
}