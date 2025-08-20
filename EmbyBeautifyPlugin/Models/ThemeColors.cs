using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// Color configuration for a theme
    /// </summary>
    public class ThemeColors
    {
        /// <summary>
        /// Primary color
        /// </summary>
        [Required]
        public string Primary { get; set; }

        /// <summary>
        /// Secondary color
        /// </summary>
        public string Secondary { get; set; }

        /// <summary>
        /// Background color
        /// </summary>
        [Required]
        public string Background { get; set; }

        /// <summary>
        /// Surface color
        /// </summary>
        public string Surface { get; set; }

        /// <summary>
        /// Text color
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// Accent color
        /// </summary>
        public string Accent { get; set; }

        /// <summary>
        /// Validates color values
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (!IsValidColor(Primary))
                errors.Add("Primary color is not a valid CSS color value");

            if (!string.IsNullOrEmpty(Secondary) && !IsValidColor(Secondary))
                errors.Add("Secondary color is not a valid CSS color value");

            if (!IsValidColor(Background))
                errors.Add("Background color is not a valid CSS color value");

            if (!string.IsNullOrEmpty(Surface) && !IsValidColor(Surface))
                errors.Add("Surface color is not a valid CSS color value");

            if (!IsValidColor(Text))
                errors.Add("Text color is not a valid CSS color value");

            if (!string.IsNullOrEmpty(Accent) && !IsValidColor(Accent))
                errors.Add("Accent color is not a valid CSS color value");

            return errors;
        }

        /// <summary>
        /// Validates if a string is a valid CSS color
        /// </summary>
        /// <param name="color">Color string to validate</param>
        /// <returns>True if valid CSS color</returns>
        private static bool IsValidColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            // Check for hex colors (#RGB, #RRGGBB, #RRGGBBAA)
            if (Regex.IsMatch(color, @"^#([A-Fa-f0-9]{3}|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$"))
                return true;

            // Check for rgb/rgba colors with value validation
            var rgbMatch = Regex.Match(color, @"^rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+)\s*)?\)$");
            if (rgbMatch.Success)
            {
                var r = int.Parse(rgbMatch.Groups[1].Value);
                var g = int.Parse(rgbMatch.Groups[2].Value);
                var b = int.Parse(rgbMatch.Groups[3].Value);
                
                if (r > 255 || g > 255 || b > 255)
                    return false;
                    
                if (rgbMatch.Groups[4].Success)
                {
                    var a = double.Parse(rgbMatch.Groups[4].Value);
                    if (a < 0 || a > 1)
                        return false;
                }
                
                return true;
            }

            // Check for hsl/hsla colors
            if (Regex.IsMatch(color, @"^hsla?\(\s*\d+\s*,\s*\d+%\s*,\s*\d+%\s*(,\s*[\d.]+\s*)?\)$"))
                return true;

            // Check for named colors (basic validation)
            var namedColors = new[] { "transparent", "black", "white", "red", "green", "blue", "yellow", "orange", "purple", "pink", "gray", "grey" };
            return System.Array.IndexOf(namedColors, color.ToLowerInvariant()) >= 0;
        }
    }
}