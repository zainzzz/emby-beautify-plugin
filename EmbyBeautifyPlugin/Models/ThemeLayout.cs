using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// Layout configuration for a theme
    /// </summary>
    public class ThemeLayout
    {
        /// <summary>
        /// Border radius for elements
        /// </summary>
        public string BorderRadius { get; set; }

        /// <summary>
        /// Spacing unit for margins and padding
        /// </summary>
        public string SpacingUnit { get; set; }

        /// <summary>
        /// General spacing value
        /// </summary>
        public string Spacing { get; set; }

        /// <summary>
        /// Shadow configuration for elements
        /// </summary>
        public string BoxShadow { get; set; }

        /// <summary>
        /// Maximum width for content containers
        /// </summary>
        public string MaxWidth { get; set; }

        /// <summary>
        /// Validates layout values
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (!string.IsNullOrEmpty(BorderRadius) && !IsValidCssLength(BorderRadius))
                errors.Add("BorderRadius is not a valid CSS length value");

            if (!string.IsNullOrEmpty(SpacingUnit) && !IsValidCssLength(SpacingUnit))
                errors.Add("SpacingUnit is not a valid CSS length value");

            if (!string.IsNullOrEmpty(MaxWidth) && !IsValidCssLength(MaxWidth))
                errors.Add("MaxWidth is not a valid CSS length value");

            if (!string.IsNullOrEmpty(BoxShadow) && !IsValidBoxShadow(BoxShadow))
                errors.Add("BoxShadow is not a valid CSS box-shadow value");

            return errors;
        }

        /// <summary>
        /// Validates CSS length value
        /// </summary>
        private static bool IsValidCssLength(string length)
        {
            if (string.IsNullOrWhiteSpace(length))
                return false;

            // Check for valid CSS length units
            return Regex.IsMatch(length, @"^\d+(\.\d+)?(px|em|rem|%|pt|pc|in|cm|mm|ex|ch|vw|vh|vmin|vmax)$") ||
                   length.ToLowerInvariant() == "auto" ||
                   length == "0";
        }

        /// <summary>
        /// Validates CSS box-shadow value (basic validation)
        /// </summary>
        private static bool IsValidBoxShadow(string boxShadow)
        {
            if (string.IsNullOrWhiteSpace(boxShadow))
                return false;

            var trimmed = boxShadow.Trim();
            if (trimmed.Length == 0)
                return false;

            // Allow "none" or basic shadow patterns
            if (trimmed.ToLowerInvariant() == "none")
                return true;

            // Basic validation for shadow format (offset-x offset-y blur-radius spread-radius color)
            // Must contain at least some numbers and valid characters
            return Regex.IsMatch(trimmed, @"^[\d\s\w#(),.-]+$") &&
                   Regex.IsMatch(trimmed, @"\d");
        }
    }
}