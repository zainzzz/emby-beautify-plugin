using EmbyBeautifyPlugin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 响应式样式管理器，负责管理不同设备断点的样式
    /// </summary>
    public class ResponsiveStyleManager
    {
        private readonly ILogger<ResponsiveStyleManager> _logger;
        private readonly Dictionary<string, BreakpointDefinition> _breakpoints;

        public ResponsiveStyleManager(ILogger<ResponsiveStyleManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _breakpoints = InitializeDefaultBreakpoints();
        }

        /// <summary>
        /// 生成响应式CSS
        /// </summary>
        /// <param name="responsiveSettings">响应式设置</param>
        /// <param name="theme">主题</param>
        /// <returns>响应式CSS字符串</returns>
        public async Task<string> GenerateResponsiveCssAsync(ResponsiveSettings responsiveSettings, Theme theme = null)
        {
            if (responsiveSettings == null)
                throw new ArgumentNullException(nameof(responsiveSettings));

            await Task.CompletedTask; // 保持异步签名一致性

            var cssBuilder = new StringBuilder();

            // 生成桌面端样式
            if (responsiveSettings.Desktop != null)
            {
                var desktopCss = GenerateBreakpointCss("desktop", responsiveSettings.Desktop, _breakpoints["desktop"]);
                cssBuilder.AppendLine(desktopCss);
            }

            // 生成平板端样式
            if (responsiveSettings.Tablet != null)
            {
                var tabletCss = GenerateBreakpointCss("tablet", responsiveSettings.Tablet, _breakpoints["tablet"]);
                cssBuilder.AppendLine(tabletCss);
            }

            // 生成移动端样式
            if (responsiveSettings.Mobile != null)
            {
                var mobileCss = GenerateBreakpointCss("mobile", responsiveSettings.Mobile, _breakpoints["mobile"]);
                cssBuilder.AppendLine(mobileCss);
            }

            // 生成通用响应式工具类
            cssBuilder.AppendLine(GenerateResponsiveUtilities());

            _logger.LogDebug("响应式CSS生成完成，长度: {Length}", cssBuilder.Length);

            return cssBuilder.ToString();
        }

        /// <summary>
        /// 为特定断点生成CSS
        /// </summary>
        /// <param name="breakpointName">断点名称</param>
        /// <param name="settings">断点设置</param>
        /// <param name="definition">断点定义</param>
        /// <returns>CSS字符串</returns>
        private string GenerateBreakpointCss(string breakpointName, BreakpointSettings settings, BreakpointDefinition definition)
        {
            var css = new StringBuilder();

            // 生成媒体查询
            var mediaQuery = GenerateMediaQuery(definition);
            css.AppendLine($"@media {mediaQuery} {{");

            // 根变量
            css.AppendLine("  :root {");
            css.AppendLine($"    --responsive-columns: {settings.GridColumns};");
            css.AppendLine($"    --responsive-gap: {settings.GridGap};");
            css.AppendLine($"    --responsive-font-scale: {settings.FontScale};");
            
            // 计算响应式字体大小
            if (settings.FontScale != 1.0)
            {
                css.AppendLine($"    --font-size-responsive: calc(var(--font-size) * {settings.FontScale});");
            }
            
            css.AppendLine("  }");

            // 容器样式
            css.AppendLine("  .container, .main-container {");
            if (breakpointName == "mobile")
            {
                css.AppendLine("    padding: 0 calc(var(--spacing-unit) * 0.5);");
            }
            else if (breakpointName == "tablet")
            {
                css.AppendLine("    padding: 0 calc(var(--spacing-unit) * 0.75);");
            }
            css.AppendLine("  }");

            // 网格系统
            css.AppendLine("  .responsive-grid {");
            css.AppendLine("    display: grid;");
            css.AppendLine("    grid-template-columns: repeat(var(--responsive-columns), 1fr);");
            css.AppendLine("    gap: var(--responsive-gap);");
            css.AppendLine("  }");

            // 卡片样式调整
            css.AppendLine("  .card, .media-card {");
            if (breakpointName == "mobile")
            {
                css.AppendLine("    margin-bottom: calc(var(--spacing-unit) * 0.5);");
                css.AppendLine("    padding: calc(var(--spacing-unit) * 0.75);");
            }
            else if (breakpointName == "tablet")
            {
                css.AppendLine("    margin-bottom: calc(var(--spacing-unit) * 0.75);");
            }
            css.AppendLine("  }");

            // 字体大小调整
            if (settings.FontScale != 1.0)
            {
                css.AppendLine("  body {");
                css.AppendLine("    font-size: var(--font-size-responsive);");
                css.AppendLine("  }");
            }

            // 特定断点的样式
            switch (breakpointName)
            {
                case "mobile":
                    css.AppendLine(GenerateMobileSpecificStyles());
                    break;
                case "tablet":
                    css.AppendLine(GenerateTabletSpecificStyles());
                    break;
                case "desktop":
                    css.AppendLine(GenerateDesktopSpecificStyles());
                    break;
            }

            css.AppendLine("}");

            return css.ToString();
        }

        /// <summary>
        /// 生成媒体查询字符串
        /// </summary>
        /// <param name="definition">断点定义</param>
        /// <returns>媒体查询字符串</returns>
        private string GenerateMediaQuery(BreakpointDefinition definition)
        {
            var conditions = new List<string>();

            if (definition.MinWidth > 0)
            {
                conditions.Add($"(min-width: {definition.MinWidth}px)");
            }

            if (definition.MaxWidth < int.MaxValue)
            {
                conditions.Add($"(max-width: {definition.MaxWidth}px)");
            }

            return string.Join(" and ", conditions);
        }

        /// <summary>
        /// 生成移动端特定样式
        /// </summary>
        /// <returns>CSS字符串</returns>
        private string GenerateMobileSpecificStyles()
        {
            return @"
  /* 移动端特定样式 */
  .hide-mobile {
    display: none !important;
  }
  
  .show-mobile {
    display: block !important;
  }
  
  .mobile-full-width {
    width: 100% !important;
  }
  
  .mobile-center {
    text-align: center !important;
  }
  
  .mobile-stack {
    flex-direction: column !important;
  }
  
  .button, .btn, button {
    padding: calc(var(--spacing-unit) * 0.75) var(--spacing-unit);
    font-size: calc(var(--font-size) * 1.1);
    min-height: 44px; /* 触摸友好的最小高度 */
  }
  
  .navigation, .nav, .navbar {
    padding: calc(var(--spacing-unit) * 0.5);
  }";
        }

        /// <summary>
        /// 生成平板端特定样式
        /// </summary>
        /// <returns>CSS字符串</returns>
        private string GenerateTabletSpecificStyles()
        {
            return @"
  /* 平板端特定样式 */
  .hide-tablet {
    display: none !important;
  }
  
  .show-tablet {
    display: block !important;
  }
  
  .tablet-two-column {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--responsive-gap);
  }
  
  .tablet-center {
    text-align: center !important;
  }";
        }

        /// <summary>
        /// 生成桌面端特定样式
        /// </summary>
        /// <returns>CSS字符串</returns>
        private string GenerateDesktopSpecificStyles()
        {
            return @"
  /* 桌面端特定样式 */
  .hide-desktop {
    display: none !important;
  }
  
  .show-desktop {
    display: block !important;
  }
  
  .desktop-multi-column {
    column-count: var(--responsive-columns);
    column-gap: var(--responsive-gap);
  }";
        }

        /// <summary>
        /// 生成响应式工具类
        /// </summary>
        /// <returns>CSS字符串</returns>
        private string GenerateResponsiveUtilities()
        {
            return @"
/* 响应式工具类 */
.responsive-text {
  font-size: calc(var(--font-size) * var(--responsive-font-scale, 1));
}

.responsive-spacing {
  margin: calc(var(--spacing-unit) * var(--responsive-font-scale, 1));
}

.responsive-padding {
  padding: calc(var(--spacing-unit) * var(--responsive-font-scale, 1));
}

/* 显示/隐藏工具类 */
.show-mobile, .show-tablet, .show-desktop {
  display: none;
}

/* 弹性布局工具类 */
.flex-responsive {
  display: flex;
  flex-wrap: wrap;
  gap: var(--responsive-gap);
}

.flex-responsive > * {
  flex: 1 1 calc((100% / var(--responsive-columns)) - var(--responsive-gap));
}

/* 图片响应式 */
.responsive-image {
  max-width: 100%;
  height: auto;
  display: block;
}

/* 视频响应式 */
.responsive-video {
  position: relative;
  padding-bottom: 56.25%; /* 16:9 宽高比 */
  height: 0;
  overflow: hidden;
}

.responsive-video iframe,
.responsive-video object,
.responsive-video embed {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
}";
        }

        /// <summary>
        /// 初始化默认断点
        /// </summary>
        /// <returns>断点定义字典</returns>
        private Dictionary<string, BreakpointDefinition> InitializeDefaultBreakpoints()
        {
            return new Dictionary<string, BreakpointDefinition>
            {
                ["mobile"] = new BreakpointDefinition
                {
                    Name = "mobile",
                    MinWidth = 0,
                    MaxWidth = 767,
                    Description = "移动设备"
                },
                ["tablet"] = new BreakpointDefinition
                {
                    Name = "tablet",
                    MinWidth = 768,
                    MaxWidth = 1199,
                    Description = "平板设备"
                },
                ["desktop"] = new BreakpointDefinition
                {
                    Name = "desktop",
                    MinWidth = 1200,
                    MaxWidth = int.MaxValue,
                    Description = "桌面设备"
                }
            };
        }

        /// <summary>
        /// 获取断点定义
        /// </summary>
        /// <param name="name">断点名称</param>
        /// <returns>断点定义</returns>
        public BreakpointDefinition GetBreakpoint(string name)
        {
            return _breakpoints.TryGetValue(name, out var breakpoint) ? breakpoint : null;
        }

        /// <summary>
        /// 获取所有断点定义
        /// </summary>
        /// <returns>断点定义列表</returns>
        public List<BreakpointDefinition> GetAllBreakpoints()
        {
            return _breakpoints.Values.ToList();
        }

        /// <summary>
        /// 添加自定义断点
        /// </summary>
        /// <param name="definition">断点定义</param>
        public void AddBreakpoint(BreakpointDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            _breakpoints[definition.Name] = definition;
            _logger.LogDebug("添加自定义断点: {Name} ({MinWidth}px - {MaxWidth}px)", 
                definition.Name, definition.MinWidth, definition.MaxWidth);
        }

        /// <summary>
        /// 验证响应式设置
        /// </summary>
        /// <param name="settings">响应式设置</param>
        /// <returns>验证结果</returns>
        public ResponsiveValidationResult ValidateResponsiveSettings(ResponsiveSettings settings)
        {
            var result = new ResponsiveValidationResult { IsValid = true };

            if (settings == null)
            {
                result.IsValid = false;
                result.Errors.Add("响应式设置不能为空");
                return result;
            }

            // 验证桌面端设置
            if (settings.Desktop != null)
            {
                ValidateBreakpointSettings("Desktop", settings.Desktop, result);
            }

            // 验证平板端设置
            if (settings.Tablet != null)
            {
                ValidateBreakpointSettings("Tablet", settings.Tablet, result);
            }

            // 验证移动端设置
            if (settings.Mobile != null)
            {
                ValidateBreakpointSettings("Mobile", settings.Mobile, result);
            }

            return result;
        }

        /// <summary>
        /// 验证断点设置
        /// </summary>
        /// <param name="name">断点名称</param>
        /// <param name="settings">断点设置</param>
        /// <param name="result">验证结果</param>
        private void ValidateBreakpointSettings(string name, BreakpointSettings settings, ResponsiveValidationResult result)
        {
            if (settings.GridColumns <= 0)
            {
                result.IsValid = false;
                result.Errors.Add($"{name}: 网格列数必须大于0");
            }

            if (settings.FontScale <= 0)
            {
                result.IsValid = false;
                result.Errors.Add($"{name}: 字体缩放比例必须大于0");
            }

            if (string.IsNullOrEmpty(settings.GridGap))
            {
                result.Warnings.Add($"{name}: 网格间距为空，将使用默认值");
            }
        }
    }

    /// <summary>
    /// 断点定义
    /// </summary>
    public class BreakpointDefinition
    {
        public string Name { get; set; }
        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// 响应式验证结果
    /// </summary>
    public class ResponsiveValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}