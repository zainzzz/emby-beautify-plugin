using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// CSS优化器，用于压缩和优化CSS代码
    /// </summary>
    public class CssOptimizer
    {
        private readonly Dictionary<string, string> _colorMap;
        private readonly HashSet<string> _shorthandProperties;

        public CssOptimizer()
        {
            _colorMap = InitializeColorMap();
            _shorthandProperties = InitializeShorthandProperties();
        }

        /// <summary>
        /// 优化CSS代码
        /// </summary>
        /// <param name="css">原始CSS</param>
        /// <param name="options">优化选项</param>
        /// <returns>优化后的CSS</returns>
        public async Task<string> OptimizeCssAsync(string css, CssGenerationOptions options)
        {
            if (string.IsNullOrEmpty(css))
                return string.Empty;

            await Task.CompletedTask; // 保持异步签名一致性

            var optimizedCss = css;

            // 移除注释（如果需要）
            if (!options.IncludeComments)
            {
                optimizedCss = RemoveComments(optimizedCss);
            }

            // 压缩空白字符
            if (options.Minify)
            {
                optimizedCss = MinifyWhitespace(optimizedCss);
            }

            // 优化颜色值
            optimizedCss = OptimizeColors(optimizedCss);

            // 优化数值
            optimizedCss = OptimizeNumbers(optimizedCss);

            // 合并相同的选择器
            optimizedCss = MergeDuplicateSelectors(optimizedCss);

            // 移除空规则
            optimizedCss = RemoveEmptyRules(optimizedCss);

            // 优化简写属性
            optimizedCss = OptimizeShorthandProperties(optimizedCss);

            // 移除不必要的分号
            optimizedCss = RemoveUnnecessarySemicolons(optimizedCss);

            return optimizedCss;
        }

        /// <summary>
        /// 移除CSS注释
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>移除注释后的CSS</returns>
        private string RemoveComments(string css)
        {
            // 移除 /* ... */ 注释，但保留重要的注释（以 /*! 开头）
            return Regex.Replace(css, @"/\*(?!\!).*?\*/", "", RegexOptions.Singleline);
        }

        /// <summary>
        /// 压缩空白字符
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>压缩后的CSS</returns>
        private string MinifyWhitespace(string css)
        {
            // 移除多余的空白字符
            css = Regex.Replace(css, @"\s+", " ");
            
            // 移除大括号前后的空格
            css = Regex.Replace(css, @"\s*{\s*", "{");
            css = Regex.Replace(css, @"\s*}\s*", "}");
            
            // 移除分号前后的空格
            css = Regex.Replace(css, @"\s*;\s*", ";");
            
            // 移除冒号前后的空格
            css = Regex.Replace(css, @"\s*:\s*", ":");
            
            // 移除逗号后的空格
            css = Regex.Replace(css, @",\s+", ",");
            
            // 移除开头和结尾的空白
            css = css.Trim();
            
            return css;
        }

        /// <summary>
        /// 优化颜色值
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>优化后的CSS</returns>
        private string OptimizeColors(string css)
        {
            // 将长十六进制颜色转换为短格式
            css = Regex.Replace(css, @"#([0-9a-fA-F])\1([0-9a-fA-F])\2([0-9a-fA-F])\3\b", "#$1$2$3");
            
            // 将命名颜色转换为更短的十六进制格式
            foreach (var colorPair in _colorMap)
            {
                if (colorPair.Value.Length < colorPair.Key.Length)
                {
                    css = Regex.Replace(css, $@"\b{Regex.Escape(colorPair.Key)}\b", colorPair.Value, RegexOptions.IgnoreCase);
                }
            }
            
            // 优化RGB颜色为十六进制（如果更短）
            css = Regex.Replace(css, @"rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)", match =>
            {
                if (int.TryParse(match.Groups[1].Value, out int r) &&
                    int.TryParse(match.Groups[2].Value, out int g) &&
                    int.TryParse(match.Groups[3].Value, out int b))
                {
                    var hex = $"#{r:x2}{g:x2}{b:x2}";
                    return hex.Length <= match.Value.Length ? hex : match.Value;
                }
                return match.Value;
            });
            
            return css;
        }

        /// <summary>
        /// 优化数值
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>优化后的CSS</returns>
        private string OptimizeNumbers(string css)
        {
            // 移除小数点前的0
            css = Regex.Replace(css, @"\b0+\.(\d+)", ".$1");
            
            // 移除小数点后多余的0
            css = Regex.Replace(css, @"(\.\d*?)0+\b", "$1");
            css = Regex.Replace(css, @"\.0+\b", "");
            
            // 将0值的单位移除
            css = Regex.Replace(css, @"\b0+(px|em|rem|%|pt|pc|in|cm|mm|ex|ch|vw|vh|vmin|vmax|deg|rad|grad|turn|s|ms|Hz|kHz|dpi|dpcm|dppx)\b", "0");
            
            // 优化百分比
            css = Regex.Replace(css, @"\b0+%", "0%");
            
            return css;
        }

        /// <summary>
        /// 合并相同的选择器
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>优化后的CSS</returns>
        private string MergeDuplicateSelectors(string css)
        {
            var rules = new Dictionary<string, List<string>>();
            var rulePattern = @"([^{}]+)\{([^{}]*)\}";
            
            var matches = Regex.Matches(css, rulePattern);
            var otherContent = css;
            
            foreach (Match match in matches)
            {
                var selector = match.Groups[1].Value.Trim();
                var properties = match.Groups[2].Value.Trim();
                
                if (!string.IsNullOrEmpty(properties))
                {
                    if (!rules.ContainsKey(selector))
                    {
                        rules[selector] = new List<string>();
                    }
                    rules[selector].Add(properties);
                }
                
                otherContent = otherContent.Replace(match.Value, "");
            }
            
            var result = new StringBuilder();
            result.Append(otherContent);
            
            foreach (var rule in rules)
            {
                var mergedProperties = string.Join(";", rule.Value.Where(p => !string.IsNullOrEmpty(p)));
                if (!string.IsNullOrEmpty(mergedProperties))
                {
                    result.AppendLine($"{rule.Key}{{{mergedProperties}}}");
                }
            }
            
            return result.ToString();
        }

        /// <summary>
        /// 移除空规则
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>优化后的CSS</returns>
        private string RemoveEmptyRules(string css)
        {
            // 移除空的CSS规则
            return Regex.Replace(css, @"[^{}]+\{\s*\}", "");
        }

        /// <summary>
        /// 优化简写属性
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>优化后的CSS</returns>
        private string OptimizeShorthandProperties(string css)
        {
            // 优化margin和padding的简写
            css = OptimizeBoxModel(css, "margin");
            css = OptimizeBoxModel(css, "padding");
            
            // 优化border简写
            css = OptimizeBorder(css);
            
            // 优化background简写
            css = OptimizeBackground(css);
            
            return css;
        }

        /// <summary>
        /// 优化盒模型属性（margin, padding）
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <param name="property">属性名</param>
        /// <returns>优化后的CSS</returns>
        private string OptimizeBoxModel(string css, string property)
        {
            var pattern = $@"{property}-top\s*:\s*([^;]+);\s*{property}-right\s*:\s*([^;]+);\s*{property}-bottom\s*:\s*([^;]+);\s*{property}-left\s*:\s*([^;]+);";
            
            return Regex.Replace(css, pattern, match =>
            {
                var top = match.Groups[1].Value.Trim();
                var right = match.Groups[2].Value.Trim();
                var bottom = match.Groups[3].Value.Trim();
                var left = match.Groups[4].Value.Trim();
                
                // 如果所有值相同
                if (top == right && right == bottom && bottom == left)
                {
                    return $"{property}:{top};";
                }
                
                // 如果上下相同，左右相同
                if (top == bottom && right == left)
                {
                    return $"{property}:{top} {right};";
                }
                
                // 如果左右相同
                if (right == left)
                {
                    return $"{property}:{top} {right} {bottom};";
                }
                
                // 使用完整的四值简写
                return $"{property}:{top} {right} {bottom} {left};";
            }, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 优化边框属性
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>优化后的CSS</returns>
        private string OptimizeBorder(string css)
        {
            var pattern = @"border-width\s*:\s*([^;]+);\s*border-style\s*:\s*([^;]+);\s*border-color\s*:\s*([^;]+);";
            
            return Regex.Replace(css, pattern, match =>
            {
                var width = match.Groups[1].Value.Trim();
                var style = match.Groups[2].Value.Trim();
                var color = match.Groups[3].Value.Trim();
                
                return $"border:{width} {style} {color};";
            }, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 优化背景属性
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>优化后的CSS</returns>
        private string OptimizeBackground(string css)
        {
            // 这里可以实现背景属性的简写优化
            // 由于背景属性比较复杂，这里只做基本的优化
            return css;
        }

        /// <summary>
        /// 移除不必要的分号
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>优化后的CSS</returns>
        private string RemoveUnnecessarySemicolons(string css)
        {
            // 移除规则末尾的最后一个分号（在压缩模式下）
            return Regex.Replace(css, @";\s*}", "}");
        }

        /// <summary>
        /// 初始化颜色映射表
        /// </summary>
        /// <returns>颜色映射字典</returns>
        private Dictionary<string, string> InitializeColorMap()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "black", "#000" },
                { "white", "#fff" },
                { "red", "#f00" },
                { "green", "#008000" },
                { "blue", "#00f" },
                { "yellow", "#ff0" },
                { "cyan", "#0ff" },
                { "magenta", "#f0f" },
                { "silver", "#c0c0c0" },
                { "gray", "#808080" },
                { "maroon", "#800000" },
                { "olive", "#808000" },
                { "lime", "#0f0" },
                { "aqua", "#0ff" },
                { "teal", "#008080" },
                { "navy", "#000080" },
                { "fuchsia", "#f0f" }
            };
        }

        /// <summary>
        /// 初始化简写属性集合
        /// </summary>
        /// <returns>简写属性集合</returns>
        private HashSet<string> InitializeShorthandProperties()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "margin", "padding", "border", "border-width", "border-style", "border-color",
                "background", "font", "list-style", "outline", "animation", "transition"
            };
        }

        /// <summary>
        /// 获取CSS大小（字节）
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>大小（字节）</returns>
        public int GetCssSize(string css)
        {
            return string.IsNullOrEmpty(css) ? 0 : Encoding.UTF8.GetByteCount(css);
        }

        /// <summary>
        /// 计算压缩比率
        /// </summary>
        /// <param name="originalCss">原始CSS</param>
        /// <param name="optimizedCss">优化后的CSS</param>
        /// <returns>压缩比率（百分比）</returns>
        public double CalculateCompressionRatio(string originalCss, string optimizedCss)
        {
            var originalSize = GetCssSize(originalCss);
            var optimizedSize = GetCssSize(optimizedCss);
            
            if (originalSize == 0)
                return 0;
            
            return Math.Round((1.0 - (double)optimizedSize / originalSize) * 100, 2);
        }

        /// <summary>
        /// 验证CSS语法
        /// </summary>
        /// <param name="css">CSS内容</param>
        /// <returns>验证结果</returns>
        public CssValidationResult ValidateCss(string css)
        {
            var result = new CssValidationResult { IsValid = true };
            
            if (string.IsNullOrEmpty(css))
            {
                return result;
            }
            
            // 检查大括号匹配
            var openBraces = css.Count(c => c == '{');
            var closeBraces = css.Count(c => c == '}');
            
            if (openBraces != closeBraces)
            {
                result.IsValid = false;
                result.Errors.Add($"大括号不匹配: 开括号 {openBraces} 个, 闭括号 {closeBraces} 个");
            }
            
            // 检查基本的CSS语法错误
            var invalidPatterns = new[]
            {
                @"[^{}]*{[^{}]*{", // 嵌套的大括号（不在字符串中）
                @"}[^{}]*}", // 多余的闭括号
            };
            
            foreach (var pattern in invalidPatterns)
            {
                if (Regex.IsMatch(css, pattern))
                {
                    result.IsValid = false;
                    result.Errors.Add($"检测到无效的CSS语法模式: {pattern}");
                }
            }
            
            return result;
        }
    }

    /// <summary>
    /// CSS验证结果
    /// </summary>
    public class CssValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}