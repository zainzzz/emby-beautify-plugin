using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Models
{
    /// <summary>
    /// Handles serialization and deserialization of theme objects
    /// </summary>
    public static class ThemeSerializer
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a theme to JSON string
        /// </summary>
        /// <param name="theme">Theme to serialize</param>
        /// <returns>JSON string representation</returns>
        public static string ToJson(Theme theme)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            return JsonSerializer.Serialize(theme, JsonOptions);
        }

        /// <summary>
        /// Deserializes a theme from JSON string
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Deserialized theme</returns>
        public static Theme FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON string cannot be null or empty", nameof(json));

            try
            {
                return JsonSerializer.Deserialize<Theme>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize theme from JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves a theme to a JSON file
        /// </summary>
        /// <param name="theme">Theme to save</param>
        /// <param name="filePath">File path to save to</param>
        public static async Task SaveToFileAsync(Theme theme, string filePath)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var json = ToJson(theme);
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// Loads a theme from a JSON file
        /// </summary>
        /// <param name="filePath">File path to load from</param>
        /// <returns>Loaded theme</returns>
        public static async Task<Theme> LoadFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Theme file not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);
            return FromJson(json);
        }

        /// <summary>
        /// Validates and deserializes a theme from JSON
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Tuple containing the theme and validation errors</returns>
        public static (Theme Theme, string[] ValidationErrors) FromJsonWithValidation(string json)
        {
            try
            {
                var theme = FromJson(json);
                var validationErrors = theme.Validate();
                return (theme, validationErrors.ToArray());
            }
            catch (Exception ex)
            {
                return (null, new[] { ex.Message });
            }
        }
    }
}