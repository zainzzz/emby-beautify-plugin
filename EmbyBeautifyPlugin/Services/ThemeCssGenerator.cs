using EmbyBeautifyPlugin.Models;
using System;
using System.Text;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// Generates CSS from theme configurations
    /// </summary>
    public static class ThemeCssGenerator
    {
        /// <summary>
        /// Generate comprehensive CSS for a theme
        /// </summary>
        /// <param name="theme">Theme to generate CSS for</param>
        /// <returns>Generated CSS string</returns>
        public static string GenerateThemeCss(Theme theme)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            var css = new StringBuilder();
            
            // Add theme header comment
            css.AppendLine($"/* Theme: {theme.Name} v{theme.Version} */");
            css.AppendLine($"/* Author: {theme.Author} */");
            css.AppendLine($"/* Description: {theme.Description} */");
            css.AppendLine();
            
            // Generate CSS custom properties
            GenerateRootVariables(css, theme);
            
            // Generate base styles
            GenerateBaseStyles(css, theme);
            
            // Generate component styles
            GenerateComponentStyles(css, theme);
            
            return css.ToString();
        }

        /// <summary>
        /// Generate CSS custom properties (CSS variables)
        /// </summary>
        private static void GenerateRootVariables(StringBuilder css, Theme theme)
        {
            css.AppendLine(":root {");
            css.AppendLine("  /* Theme Variables */");
            
            // Colors
            if (theme.Colors != null)
            {
                css.AppendLine("  /* Colors */");
                if (!string.IsNullOrEmpty(theme.Colors.Primary))
                    css.AppendLine($"  --primary-color: {theme.Colors.Primary};");
                if (!string.IsNullOrEmpty(theme.Colors.Secondary))
                    css.AppendLine($"  --secondary-color: {theme.Colors.Secondary};");
                if (!string.IsNullOrEmpty(theme.Colors.Background))
                    css.AppendLine($"  --background-color: {theme.Colors.Background};");
                if (!string.IsNullOrEmpty(theme.Colors.Surface))
                    css.AppendLine($"  --surface-color: {theme.Colors.Surface};");
                if (!string.IsNullOrEmpty(theme.Colors.Text))
                    css.AppendLine($"  --text-color: {theme.Colors.Text};");
                if (!string.IsNullOrEmpty(theme.Colors.Accent))
                    css.AppendLine($"  --accent-color: {theme.Colors.Accent};");
                css.AppendLine();
            }
            
            // Typography
            if (theme.Typography != null)
            {
                css.AppendLine("  /* Typography */");
                if (!string.IsNullOrEmpty(theme.Typography.FontFamily))
                    css.AppendLine($"  --font-family: {theme.Typography.FontFamily};");
                if (!string.IsNullOrEmpty(theme.Typography.FontSize))
                    css.AppendLine($"  --font-size: {theme.Typography.FontSize};");
                if (!string.IsNullOrEmpty(theme.Typography.HeadingWeight))
                    css.AppendLine($"  --heading-weight: {theme.Typography.HeadingWeight};");
                if (!string.IsNullOrEmpty(theme.Typography.BodyWeight))
                    css.AppendLine($"  --body-weight: {theme.Typography.BodyWeight};");
                if (!string.IsNullOrEmpty(theme.Typography.LineHeight))
                    css.AppendLine($"  --line-height: {theme.Typography.LineHeight};");
                css.AppendLine();
            }
            
            // Layout
            if (theme.Layout != null)
            {
                css.AppendLine("  /* Layout */");
                if (!string.IsNullOrEmpty(theme.Layout.BorderRadius))
                    css.AppendLine($"  --border-radius: {theme.Layout.BorderRadius};");
                if (!string.IsNullOrEmpty(theme.Layout.SpacingUnit))
                    css.AppendLine($"  --spacing-unit: {theme.Layout.SpacingUnit};");
                if (!string.IsNullOrEmpty(theme.Layout.BoxShadow))
                    css.AppendLine($"  --box-shadow: {theme.Layout.BoxShadow};");
                if (!string.IsNullOrEmpty(theme.Layout.MaxWidth))
                    css.AppendLine($"  --max-width: {theme.Layout.MaxWidth};");
                css.AppendLine();
            }
            
            // Custom properties
            if (theme.CustomProperties != null && theme.CustomProperties.Count > 0)
            {
                css.AppendLine("  /* Custom Properties */");
                foreach (var prop in theme.CustomProperties)
                {
                    css.AppendLine($"  --{prop.Key}: {prop.Value};");
                }
                css.AppendLine();
            }
            
            css.AppendLine("}");
            css.AppendLine();
        }

        /// <summary>
        /// Generate base styles for common elements
        /// </summary>
        private static void GenerateBaseStyles(StringBuilder css, Theme theme)
        {
            css.AppendLine("/* Base Styles */");
            
            // Body styles
            css.AppendLine("body {");
            css.AppendLine("  background-color: var(--background-color);");
            css.AppendLine("  color: var(--text-color);");
            css.AppendLine("  font-family: var(--font-family);");
            css.AppendLine("  font-size: var(--font-size);");
            css.AppendLine("  font-weight: var(--body-weight);");
            css.AppendLine("  line-height: var(--line-height);");
            css.AppendLine("  margin: 0;");
            css.AppendLine("  padding: 0;");
            css.AppendLine("}");
            css.AppendLine();
            
            // Heading styles
            css.AppendLine("h1, h2, h3, h4, h5, h6 {");
            css.AppendLine("  color: var(--text-color);");
            css.AppendLine("  font-weight: var(--heading-weight);");
            css.AppendLine("  margin: 0 0 var(--spacing-unit) 0;");
            css.AppendLine("}");
            css.AppendLine();
            
            // Link styles
            css.AppendLine("a {");
            css.AppendLine("  color: var(--primary-color);");
            css.AppendLine("  text-decoration: none;");
            css.AppendLine("  transition: color var(--transition-duration, 0.2s) ease;");
            css.AppendLine("}");
            css.AppendLine();
            
            css.AppendLine("a:hover {");
            css.AppendLine("  color: var(--accent-color);");
            css.AppendLine("}");
            css.AppendLine();
        }

        /// <summary>
        /// Generate component-specific styles
        /// </summary>
        private static void GenerateComponentStyles(StringBuilder css, Theme theme)
        {
            css.AppendLine("/* Component Styles */");
            
            // Container styles
            css.AppendLine(".container, .main-container {");
            css.AppendLine("  max-width: var(--max-width);");
            css.AppendLine("  margin: 0 auto;");
            css.AppendLine("  padding: 0 var(--spacing-unit);");
            css.AppendLine("}");
            css.AppendLine();
            
            // Card styles
            css.AppendLine(".card, .media-card {");
            css.AppendLine("  background-color: var(--surface-color);");
            css.AppendLine("  border-radius: var(--border-radius);");
            css.AppendLine("  box-shadow: var(--box-shadow);");
            css.AppendLine("  padding: var(--spacing-unit);");
            css.AppendLine("  margin-bottom: var(--spacing-unit);");
            css.AppendLine("  transition: transform var(--transition-duration, 0.2s) ease, box-shadow var(--transition-duration, 0.2s) ease;");
            css.AppendLine("}");
            css.AppendLine();
            
            css.AppendLine(".card:hover, .media-card:hover {");
            css.AppendLine("  transform: var(--hover-transform, translateY(-2px));");
            if (theme.CustomProperties?.ContainsKey("glow-effect") == true)
            {
                css.AppendLine("  box-shadow: var(--glow-effect);");
            }
            css.AppendLine("}");
            css.AppendLine();
            
            // Button styles
            css.AppendLine(".button, .btn, button {");
            css.AppendLine("  background: var(--gradient-primary, var(--primary-color));");
            css.AppendLine("  color: white;");
            css.AppendLine("  border: none;");
            css.AppendLine("  border-radius: var(--border-radius);");
            css.AppendLine("  padding: calc(var(--spacing-unit) * 0.5) var(--spacing-unit);");
            css.AppendLine("  font-family: var(--font-family);");
            css.AppendLine("  font-size: var(--font-size);");
            css.AppendLine("  font-weight: var(--body-weight);");
            css.AppendLine("  cursor: pointer;");
            css.AppendLine("  transition: all var(--transition-duration, 0.2s) ease;");
            css.AppendLine("}");
            css.AppendLine();
            
            css.AppendLine(".button:hover, .btn:hover, button:hover {");
            css.AppendLine("  transform: var(--hover-transform, translateY(-1px));");
            css.AppendLine("  opacity: 0.9;");
            css.AppendLine("}");
            css.AppendLine();
            
            // Navigation styles
            css.AppendLine(".navigation, .nav, .navbar {");
            css.AppendLine("  background-color: var(--surface-color);");
            css.AppendLine("  border-bottom: 1px solid var(--primary-color);");
            css.AppendLine("  padding: var(--spacing-unit);");
            css.AppendLine("}");
            css.AppendLine();
            
            // Form styles
            css.AppendLine(".form-control, input, textarea, select {");
            css.AppendLine("  background-color: var(--surface-color);");
            css.AppendLine("  color: var(--text-color);");
            css.AppendLine("  border: 1px solid var(--primary-color);");
            css.AppendLine("  border-radius: var(--border-radius);");
            css.AppendLine("  padding: calc(var(--spacing-unit) * 0.5);");
            css.AppendLine("  font-family: var(--font-family);");
            css.AppendLine("  font-size: var(--font-size);");
            css.AppendLine("  transition: border-color var(--transition-duration, 0.2s) ease;");
            css.AppendLine("}");
            css.AppendLine();
            
            css.AppendLine(".form-control:focus, input:focus, textarea:focus, select:focus {");
            css.AppendLine("  outline: none;");
            css.AppendLine("  border-color: var(--accent-color);");
            css.AppendLine("}");
            css.AppendLine();
        }
    }
}