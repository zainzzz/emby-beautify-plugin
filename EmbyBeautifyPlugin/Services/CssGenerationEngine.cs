using EmbyBeautifyPlugin.Exceptions;
using EmbyBeautifyPlugin.Extensions;
using EmbyBeautifyPlugin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 高级CSS生成引擎，支持主题转换、CSS变量、压缩和优化
    /// </summary>
    public class CssGenerationEngine
    {
        private readonly ILogger<CssGenerationEngine> _logger;
        private readonly Dictionary<string, string> _cssCache;
        private readonly CssOptimizer _optimizer;

        public CssGenerationEngine(ILogger<CssGenerationEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cssCache = new Dictionary<string, string>();
            _optimizer = new CssOptimizer();
        }

        /// <summary>
        /// 从主题生成完整的CSS
        /// </summary>
        /// <param name="theme">主题对象</param>
        /// <param name="options">生成选项</param>
        /// <returns>生成的CSS字符串</returns>
        public async Task<string> GenerateThemeCssAsync(Theme theme, CssGenerationOptions options = null)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            options ??= new CssGenerationOptions();

            using var timer = _logger.BeginOperation("GenerateThemeCss", new Dictionary<string, object>
            {
                ["ThemeId"] = theme.Id,
                ["ThemeName"] = theme.Name,
                ["Minify"] = options.Minify,
                ["IncludeVariables"] = options.IncludeVariables
            });

            try
            {
                // 检查缓存
                var cacheKey = GenerateCacheKey(theme, options);
                if (options.UseCache && _cssCache.TryGetValue(cacheKey, out var cachedCss))
                {
                    _logger.LogDebug("从缓存返回CSS: {ThemeId}", theme.Id);
                    return cachedCss;
                }

                var cssBuilder = new CssBuilder();

                // 添加主题头部注释
                if (options.IncludeComments)
                {
                    cssBuilder.AddComment($"Theme: {theme.Name} v{theme.Version}");
                    cssBuilder.AddComment($"Author: {theme.Author}");
                    cssBuilder.AddComment($"Description: {theme.Description}");
                    cssBuilder.AddComment($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                    cssBuilder.AddNewLine();
                }

                // 生成CSS变量
                if (options.IncludeVariables)
                {
                    await GenerateRootVariablesAsync(cssBuilder, theme, options);
                }

                // 生成基础样式
                if (options.IncludeBaseStyles)
                {
                    await GenerateBaseStylesAsync(cssBuilder, theme, options);
                }

                // 生成组件样式
                if (options.IncludeComponentStyles)
                {
                    await GenerateComponentStylesAsync(cssBuilder, theme, options);
                }

                // 生成响应式样式
                if (options.IncludeResponsiveStyles)
                {
                    await GenerateResponsiveStylesAsync(cssBuilder, theme, options);
                }

                // 生成动画样式
                if (options.IncludeAnimations)
                {
                    await GenerateAnimationStylesAsync(cssBuilder, theme, options);
                }

                var css = cssBuilder.Build();

                // 优化和压缩CSS
                if (options.Optimize)
                {
                    css = await _optimizer.OptimizeCssAsync(css, options);
                }

                // 缓存结果
                if (options.UseCache)
                {
                    _cssCache[cacheKey] = css;
                }

                _logger.LogDebug("CSS生成完成: {ThemeId}, 长度: {Length}", theme.Id, css.Length);
                timer.Complete(true);

                return css;
            }
            catch (Exception ex)
            {
                timer.Complete(false);
                _logger.LogBeautifyException(new BeautifyException(
                    BeautifyErrorType.StyleInjectionError,
                    "CSS生成失败",
                    $"无法为主题 '{theme.Name}' 生成CSS",
                    ex));
                throw;
            }
        }

        /// <summary>
        /// 生成CSS根变量
        /// </summary>
        private async Task GenerateRootVariablesAsync(CssBuilder cssBuilder, Theme theme, CssGenerationOptions options)
        {
            await Task.CompletedTask; // 保持异步签名一致性

            cssBuilder.AddComment("CSS Custom Properties (Variables)");
            cssBuilder.StartRule(":root");

            // 颜色变量
            if (theme.Colors != null)
            {
                cssBuilder.AddComment("Colors");
                AddColorVariables(cssBuilder, theme.Colors);
            }

            // 字体变量
            if (theme.Typography != null)
            {
                cssBuilder.AddComment("Typography");
                AddTypographyVariables(cssBuilder, theme.Typography);
            }

            // 布局变量
            if (theme.Layout != null)
            {
                cssBuilder.AddComment("Layout");
                AddLayoutVariables(cssBuilder, theme.Layout);
            }

            // 自定义属性
            if (theme.CustomProperties?.Any() == true)
            {
                cssBuilder.AddComment("Custom Properties");
                foreach (var prop in theme.CustomProperties)
                {
                    var propertyName = prop.Key.StartsWith("--") ? prop.Key : $"--{prop.Key}";
                    cssBuilder.AddProperty(propertyName, prop.Value);
                }
            }

            // 计算的颜色变量（RGB值用于透明度）
            if (theme.Colors != null && options.IncludeComputedVariables)
            {
                cssBuilder.AddComment("Computed Color Variables");
                AddComputedColorVariables(cssBuilder, theme.Colors);
            }

            cssBuilder.EndRule();
            cssBuilder.AddNewLine();
        }

        /// <summary>
        /// 添加颜色变量
        /// </summary>
        private void AddColorVariables(CssBuilder cssBuilder, ThemeColors colors)
        {
            if (!string.IsNullOrEmpty(colors.Primary))
                cssBuilder.AddProperty("--primary-color", colors.Primary);
            if (!string.IsNullOrEmpty(colors.Secondary))
                cssBuilder.AddProperty("--secondary-color", colors.Secondary);
            if (!string.IsNullOrEmpty(colors.Background))
                cssBuilder.AddProperty("--background-color", colors.Background);
            if (!string.IsNullOrEmpty(colors.Surface))
                cssBuilder.AddProperty("--surface-color", colors.Surface);
            if (!string.IsNullOrEmpty(colors.Text))
                cssBuilder.AddProperty("--text-color", colors.Text);
            if (!string.IsNullOrEmpty(colors.Accent))
                cssBuilder.AddProperty("--accent-color", colors.Accent);
        }

        /// <summary>
        /// 添加计算的颜色变量（RGB值）
        /// </summary>
        private void AddComputedColorVariables(CssBuilder cssBuilder, ThemeColors colors)
        {
            if (!string.IsNullOrEmpty(colors.Primary))
            {
                var rgb = HexToRgb(colors.Primary);
                if (rgb.HasValue)
                    cssBuilder.AddProperty("--primary-color-rgb", $"{rgb.Value.R}, {rgb.Value.G}, {rgb.Value.B}");
            }

            if (!string.IsNullOrEmpty(colors.Secondary))
            {
                var rgb = HexToRgb(colors.Secondary);
                if (rgb.HasValue)
                    cssBuilder.AddProperty("--secondary-color-rgb", $"{rgb.Value.R}, {rgb.Value.G}, {rgb.Value.B}");
            }

            if (!string.IsNullOrEmpty(colors.Accent))
            {
                var rgb = HexToRgb(colors.Accent);
                if (rgb.HasValue)
                    cssBuilder.AddProperty("--accent-color-rgb", $"{rgb.Value.R}, {rgb.Value.G}, {rgb.Value.B}");
            }
        }

        /// <summary>
        /// 添加字体变量
        /// </summary>
        private void AddTypographyVariables(CssBuilder cssBuilder, ThemeTypography typography)
        {
            if (!string.IsNullOrEmpty(typography.FontFamily))
                cssBuilder.AddProperty("--font-family", typography.FontFamily);
            if (!string.IsNullOrEmpty(typography.FontSize))
                cssBuilder.AddProperty("--font-size", typography.FontSize);
            if (!string.IsNullOrEmpty(typography.HeadingWeight))
                cssBuilder.AddProperty("--heading-weight", typography.HeadingWeight);
            if (!string.IsNullOrEmpty(typography.BodyWeight))
                cssBuilder.AddProperty("--body-weight", typography.BodyWeight);
            if (!string.IsNullOrEmpty(typography.LineHeight))
                cssBuilder.AddProperty("--line-height", typography.LineHeight);
        }

        /// <summary>
        /// 添加布局变量
        /// </summary>
        private void AddLayoutVariables(CssBuilder cssBuilder, ThemeLayout layout)
        {
            if (!string.IsNullOrEmpty(layout.BorderRadius))
                cssBuilder.AddProperty("--border-radius", layout.BorderRadius);
            if (!string.IsNullOrEmpty(layout.SpacingUnit))
                cssBuilder.AddProperty("--spacing-unit", layout.SpacingUnit);
            if (!string.IsNullOrEmpty(layout.BoxShadow))
                cssBuilder.AddProperty("--box-shadow", layout.BoxShadow);
            if (!string.IsNullOrEmpty(layout.MaxWidth))
                cssBuilder.AddProperty("--max-width", layout.MaxWidth);
        }

        /// <summary>
        /// 生成基础样式
        /// </summary>
        private async Task GenerateBaseStylesAsync(CssBuilder cssBuilder, Theme theme, CssGenerationOptions options)
        {
            await Task.CompletedTask;

            cssBuilder.AddComment("Base Styles");

            // Body样式
            cssBuilder.StartRule("body");
            cssBuilder.AddProperty("background-color", "var(--background-color)");
            cssBuilder.AddProperty("color", "var(--text-color)");
            cssBuilder.AddProperty("font-family", "var(--font-family)");
            cssBuilder.AddProperty("font-size", "var(--font-size)");
            cssBuilder.AddProperty("font-weight", "var(--body-weight)");
            cssBuilder.AddProperty("line-height", "var(--line-height)");
            cssBuilder.AddProperty("margin", "0");
            cssBuilder.AddProperty("padding", "0");
            cssBuilder.EndRule();

            // 标题样式
            cssBuilder.StartRule("h1, h2, h3, h4, h5, h6");
            cssBuilder.AddProperty("color", "var(--text-color)");
            cssBuilder.AddProperty("font-weight", "var(--heading-weight)");
            cssBuilder.AddProperty("margin", "0 0 var(--spacing-unit) 0");
            cssBuilder.EndRule();

            // 链接样式
            cssBuilder.StartRule("a");
            cssBuilder.AddProperty("color", "var(--primary-color)");
            cssBuilder.AddProperty("text-decoration", "none");
            cssBuilder.AddProperty("transition", "color var(--transition-duration, 0.2s) ease");
            cssBuilder.EndRule();

            cssBuilder.StartRule("a:hover");
            cssBuilder.AddProperty("color", "var(--accent-color)");
            cssBuilder.EndRule();

            cssBuilder.AddNewLine();
        }

        /// <summary>
        /// 生成组件样式
        /// </summary>
        private async Task GenerateComponentStylesAsync(CssBuilder cssBuilder, Theme theme, CssGenerationOptions options)
        {
            await Task.CompletedTask;

            cssBuilder.AddComment("Component Styles");

            // 容器样式
            cssBuilder.StartRule(".container, .main-container");
            cssBuilder.AddProperty("max-width", "var(--max-width)");
            cssBuilder.AddProperty("margin", "0 auto");
            cssBuilder.AddProperty("padding", "0 var(--spacing-unit)");
            cssBuilder.EndRule();

            // 卡片样式
            cssBuilder.StartRule(".card, .media-card");
            cssBuilder.AddProperty("background-color", "var(--surface-color)");
            cssBuilder.AddProperty("border-radius", "var(--border-radius)");
            cssBuilder.AddProperty("box-shadow", "var(--box-shadow)");
            cssBuilder.AddProperty("padding", "var(--spacing-unit)");
            cssBuilder.AddProperty("margin-bottom", "var(--spacing-unit)");
            cssBuilder.AddProperty("transition", "transform var(--transition-duration, 0.2s) ease, box-shadow var(--transition-duration, 0.2s) ease");
            cssBuilder.EndRule();

            cssBuilder.StartRule(".card:hover, .media-card:hover");
            cssBuilder.AddProperty("transform", "var(--hover-transform, translateY(-2px))");
            if (theme.CustomProperties?.ContainsKey("glow-effect") == true)
            {
                cssBuilder.AddProperty("box-shadow", "var(--glow-effect)");
            }
            cssBuilder.EndRule();

            // 按钮样式
            cssBuilder.StartRule(".button, .btn, button");
            cssBuilder.AddProperty("background", "var(--gradient-primary, var(--primary-color))");
            cssBuilder.AddProperty("color", "white");
            cssBuilder.AddProperty("border", "none");
            cssBuilder.AddProperty("border-radius", "var(--border-radius)");
            cssBuilder.AddProperty("padding", "calc(var(--spacing-unit) * 0.5) var(--spacing-unit)");
            cssBuilder.AddProperty("font-family", "var(--font-family)");
            cssBuilder.AddProperty("font-size", "var(--font-size)");
            cssBuilder.AddProperty("font-weight", "var(--body-weight)");
            cssBuilder.AddProperty("cursor", "pointer");
            cssBuilder.AddProperty("transition", "all var(--transition-duration, 0.2s) ease");
            cssBuilder.EndRule();

            cssBuilder.StartRule(".button:hover, .btn:hover, button:hover");
            cssBuilder.AddProperty("transform", "var(--hover-transform, translateY(-1px))");
            cssBuilder.AddProperty("opacity", "0.9");
            cssBuilder.EndRule();

            cssBuilder.AddNewLine();
        }

        /// <summary>
        /// 生成响应式样式
        /// </summary>
        private async Task GenerateResponsiveStylesAsync(CssBuilder cssBuilder, Theme theme, CssGenerationOptions options)
        {
            await Task.CompletedTask;

            cssBuilder.AddComment("Responsive Styles");

            // 使用原始CSS字符串来避免CSS Builder的复杂性
            var responsiveCss = @"
@media (min-width: 1200px) {
  :root {
    --responsive-columns: 6;
    --responsive-gap: 1.5rem;
    --responsive-font-scale: 1.0;
  }
}

@media (min-width: 768px) and (max-width: 1199px) {
  :root {
    --responsive-columns: 4;
    --responsive-gap: 1rem;
    --responsive-font-scale: 0.9;
  }
}

@media (max-width: 767px) {
  :root {
    --responsive-columns: 2;
    --responsive-gap: 0.5rem;
    --responsive-font-scale: 0.8;
  }
  
  .container, .main-container {
    padding: 0 calc(var(--spacing-unit) * 0.5);
  }
  
  .card, .media-card {
    margin-bottom: calc(var(--spacing-unit) * 0.5);
  }
}
";

            cssBuilder.AddRawCss(responsiveCss);
            cssBuilder.AddNewLine();
        }

        /// <summary>
        /// 生成动画样式
        /// </summary>
        private async Task GenerateAnimationStylesAsync(CssBuilder cssBuilder, Theme theme, CssGenerationOptions options)
        {
            await Task.CompletedTask;

            cssBuilder.AddComment("Animation Styles");

            // 使用原始CSS字符串来避免CSS Builder的复杂性
            var animationCss = @"
:root {
  --transition-duration: 0.3s;
  --hover-transform: translateY(-2px);
  --glow-effect: 0 0 20px rgba(var(--primary-color-rgb, 33, 150, 243), 0.3);
}

.emby-beautify-fade-in {
  animation: embyBeautifyFadeIn var(--transition-duration) ease-out;
}

.emby-beautify-slide-up {
  animation: embyBeautifySlideUp var(--transition-duration) ease-out;
}

@keyframes embyBeautifyFadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes embyBeautifySlideUp {
  from { 
    opacity: 0; 
    transform: translateY(20px); 
  }
  to { 
    opacity: 1; 
    transform: translateY(0); 
  }
}
";

            cssBuilder.AddRawCss(animationCss);
            cssBuilder.AddNewLine();
        }

        /// <summary>
        /// 生成缓存键
        /// </summary>
        private string GenerateCacheKey(Theme theme, CssGenerationOptions options)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(theme.Id);
            keyBuilder.Append("_");
            keyBuilder.Append(theme.Version);
            keyBuilder.Append("_");
            keyBuilder.Append(options.GetHashCode());
            return keyBuilder.ToString();
        }

        /// <summary>
        /// 将十六进制颜色转换为RGB
        /// </summary>
        private (int R, int G, int B)? HexToRgb(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return null;

            hex = hex.TrimStart('#');
            if (hex.Length != 6)
                return null;

            try
            {
                var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                var b = Convert.ToInt32(hex.Substring(4, 2), 16);
                return (r, g, b);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 清理CSS缓存
        /// </summary>
        public void ClearCache()
        {
            _cssCache.Clear();
            _logger.LogDebug("CSS缓存已清理");
        }
    }

    /// <summary>
    /// CSS生成选项
    /// </summary>
    public class CssGenerationOptions
    {
        public bool Minify { get; set; } = false;
        public bool Optimize { get; set; } = true;
        public bool UseCache { get; set; } = true;
        public bool IncludeComments { get; set; } = true;
        public bool IncludeVariables { get; set; } = true;
        public bool IncludeBaseStyles { get; set; } = true;
        public bool IncludeComponentStyles { get; set; } = true;
        public bool IncludeResponsiveStyles { get; set; } = true;
        public bool IncludeAnimations { get; set; } = false;
        public bool IncludeComputedVariables { get; set; } = true;

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 23 + Minify.GetHashCode();
            hash = hash * 23 + Optimize.GetHashCode();
            hash = hash * 23 + IncludeComments.GetHashCode();
            hash = hash * 23 + IncludeVariables.GetHashCode();
            hash = hash * 23 + IncludeBaseStyles.GetHashCode();
            hash = hash * 23 + IncludeComponentStyles.GetHashCode();
            hash = hash * 23 + IncludeResponsiveStyles.GetHashCode();
            hash = hash * 23 + IncludeAnimations.GetHashCode();
            hash = hash * 23 + IncludeComputedVariables.GetHashCode();
            return hash;
        }
    }
}