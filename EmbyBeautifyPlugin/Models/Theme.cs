using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Linq;

namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// Represents a theme configuration
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Unique identifier for the theme
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Id { get; set; }

        /// <summary>
        /// Display name of the theme
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Description of the theme
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Version of the theme
        /// </summary>
        [Required]
        [RegularExpression(@"^\d+\.\d+\.\d+$", ErrorMessage = "Version must be in format x.y.z")]
        public string Version { get; set; }

        /// <summary>
        /// Author of the theme
        /// </summary>
        [StringLength(100)]
        public string Author { get; set; }

        /// <summary>
        /// Color configuration for the theme
        /// </summary>
        [Required]
        public ThemeColors Colors { get; set; }

        /// <summary>
        /// Typography configuration for the theme
        /// </summary>
        public ThemeTypography Typography { get; set; }

        /// <summary>
        /// Layout configuration for the theme
        /// </summary>
        public ThemeLayout Layout { get; set; }

        /// <summary>
        /// Custom CSS properties for the theme
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; }

        public Theme()
        {
            CustomProperties = new Dictionary<string, string>();
        }

        /// <summary>
        /// Validates the theme configuration
        /// </summary>
        /// <returns>List of validation errors, empty if valid</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(this, context, results, true))
            {
                errors.AddRange(results.Select(r => r.ErrorMessage));
            }

            // Additional custom validation
            if (Colors != null)
            {
                errors.AddRange(Colors.Validate());
            }

            if (Typography != null)
            {
                errors.AddRange(Typography.Validate());
            }

            if (Layout != null)
            {
                errors.AddRange(Layout.Validate());
            }

            return errors;
        }

        /// <summary>
        /// Checks if the theme is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        [JsonIgnore]
        public bool IsValid => Validate().Count == 0;
    }
}