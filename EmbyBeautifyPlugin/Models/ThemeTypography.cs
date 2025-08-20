using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Linq;

namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// Typography configuration for a theme
    /// </summary>
    public class ThemeTypography
    {
        /// <summary>
        /// Primary font family
        /// </summary>
        [StringLength(200)]
        public string FontFamily { get; set; }

        /// <summary>
        /// Base font size
        /// </summary>
        public string FontSize { get; set; }

        /// <summary>
        /// Font weight for headings
        /// </summary>
        public string HeadingWeight { get; set; }

        /// <summary>
        /// Font weight for body text
        /// </summary>
        public string BodyWeight { get; set; }

        /// <summary>
        /// General font weight
        /// </summary>
        public string FontWeight { get; set; }

        /// <summary>
        /// Line height for text
        /// </summary>
        public string LineHeight { get; set; }

        /// <summary>
        /// Validates typography values
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (!string.IsNullOrEmpty(FontSize) && !IsValidFontSize(FontSize))
                errors.Add("FontSize is not a valid CSS font-size value");

            if (!string.IsNullOrEmpty(HeadingWeight) && !IsValidFontWeight(HeadingWeight))
                errors.Add("HeadingWeight is not a valid CSS font-weight value");

            if (!string.IsNullOrEmpty(BodyWeight) && !IsValidFontWeight(BodyWeight))
                errors.Add("BodyWeight is not a valid CSS font-weight value");

            if (!string.IsNullOrEmpty(LineHeight) && !IsValidLineHeight(LineHeight))
                errors.Add("LineHeight is not a valid CSS line-height value");

            return errors;
        }

        /// <summary>
        /// Validates CSS font-size value
        /// </summary>
        private static bool IsValidFontSize(string fontSize)
        {
            if (string.IsNullOrWhiteSpace(fontSize))
                return false;

            // Check for valid CSS units (px, em, rem, %, pt, etc.)
            return Regex.IsMatch(fontSize, @"^\d+(\.\d+)?(px|em|rem|%|pt|pc|in|cm|mm|ex|ch|vw|vh|vmin|vmax)$");
        }

        /// <summary>
        /// Validates CSS font-weight value
        /// </summary>
        private static bool IsValidFontWeight(string fontWeight)
        {
            if (string.IsNullOrWhiteSpace(fontWeight))
                return false;

            // Check for numeric values (100-900)
            if (Regex.IsMatch(fontWeight, @"^[1-9]00$"))
                return true;

            // Check for named values
            var namedWeights = new[] { "normal", "bold", "bolder", "lighter" };
            return namedWeights.Contains(fontWeight.ToLowerInvariant());
        }

        /// <summary>
        /// Validates CSS line-height value
        /// </summary>
        private static bool IsValidLineHeight(string lineHeight)
        {
            if (string.IsNullOrWhiteSpace(lineHeight))
                return false;

            // Check for unitless numbers
            if (Regex.IsMatch(lineHeight, @"^\d+(\.\d+)?$"))
                return true;

            // Check for values with units
            if (Regex.IsMatch(lineHeight, @"^\d+(\.\d+)?(px|em|rem|%|pt|pc|in|cm|mm|ex|ch|vw|vh|vmin|vmax)$"))
                return true;

            // Check for named values
            return lineHeight.ToLowerInvariant() == "normal";
        }
    }
}