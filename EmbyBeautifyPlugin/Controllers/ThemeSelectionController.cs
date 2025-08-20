using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Services;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Controllers
{
    /// <summary>
    /// 主题选择界面控制器，提供主题选择和自定义功能
    /// </summary>
    [Route("/emby-beautify/theme-selection", "GET", Summary = "获取主题选择界面")]
    [Route("/emby-beautify/theme-customizer", "GET", Summary = "获取主题自定义界面")]
    [Route("/emby-beautify/theme-preview/{ThemeId}", "GET", Summary = "获取主题预览数据")]
    public class ThemeSelectionController : IService
    {
        private readonly IThemeManager _themeManager;
        private readonly IConfigurationManager _configurationManager;
        private readonly ILogger _logger;

        public ThemeSelectionController(
            IThemeManager themeManager,
            IConfigurationManager configurationManager,
            ILogManager logManager)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            _logger = logManager?.GetLogger(GetType().Name) ?? throw new ArgumentNullException(nameof(logManager));
        }

        /// <summary>
        /// 获取主题选择界面
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>HTML页面内容</returns>
        public async Task<object> Get(GetThemeSelectionRequest request)
        {
            try
            {
                _logger.Debug("开始获取主题选择界面");

                var htmlContent = await GetEmbeddedResourceAsync("EmbyBeautifyPlugin.Views.ThemeSelection.html");
                
                // 获取主题数据
                var themes = await _themeManager.GetAvailableThemesAsync();
                var activeTheme = await _themeManager.GetActiveThemeAsync();
                var configuration = await _configurationManager.LoadConfigurationAsync();
                
                // 替换页面中的占位符
                htmlContent = htmlContent.Replace("{{AVAILABLE_THEMES}}", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(themes, Newtonsoft.Json.Formatting.Indented));
                htmlContent = htmlContent.Replace("{{ACTIVE_THEME}}", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(activeTheme, Newtonsoft.Json.Formatting.Indented));
                htmlContent = htmlContent.Replace("{{CURRENT_CONFIG}}", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(configuration, Newtonsoft.Json.Formatting.Indented));

                _logger.Debug("成功获取主题选择界面，可用主题数量: {0}", themes.Count);
                
                return htmlContent;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("获取主题选择界面失败", ex);
                return $"<html><body><h1>错误</h1><p>加载主题选择界面失败: {ex.Message}</p></body></html>";
            }
        }

        /// <summary>
        /// 获取主题自定义界面
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>HTML页面内容</returns>
        public async Task<object> Get(GetThemeCustomizerRequest request)
        {
            try
            {
                _logger.Debug("开始获取主题自定义界面");

                var htmlContent = await GetEmbeddedResourceAsync("EmbyBeautifyPlugin.Views.ThemeCustomizer.html");
                
                // 获取当前活动主题
                var activeTheme = await _themeManager.GetActiveThemeAsync();
                var configuration = await _configurationManager.LoadConfigurationAsync();
                
                // 替换页面中的占位符
                htmlContent = htmlContent.Replace("{{ACTIVE_THEME}}", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(activeTheme, Newtonsoft.Json.Formatting.Indented));
                htmlContent = htmlContent.Replace("{{CURRENT_CONFIG}}", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(configuration, Newtonsoft.Json.Formatting.Indented));

                _logger.Debug("成功获取主题自定义界面");
                
                return htmlContent;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("获取主题自定义界面失败", ex);
                return $"<html><body><h1>错误</h1><p>加载主题自定义界面失败: {ex.Message}</p></body></html>";
            }
        }

        /// <summary>
        /// 获取主题预览数据
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>主题预览数据</returns>
        public async Task<object> Get(GetThemePreviewRequest request)
        {
            try
            {
                _logger.Debug("开始获取主题预览数据，主题ID: {0}", request.ThemeId);

                if (string.IsNullOrWhiteSpace(request.ThemeId))
                {
                    throw new ArgumentException("主题ID不能为空", nameof(request.ThemeId));
                }

                var themes = await _themeManager.GetAvailableThemesAsync();
                var theme = themes.Find(t => t.Id == request.ThemeId);

                if (theme == null)
                {
                    throw new ArgumentException($"未找到ID为 '{request.ThemeId}' 的主题");
                }

                // 生成主题CSS
                var css = await _themeManager.GenerateThemeCssAsync(theme);

                var response = new GetThemePreviewResponse
                {
                    Theme = theme,
                    Css = css,
                    PreviewData = GeneratePreviewData(theme),
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.Debug("成功获取主题预览数据: {0}", request.ThemeId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"获取主题预览数据 '{request.ThemeId}' 失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 从嵌入资源中获取文件内容
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <returns>文件内容</returns>
        private async Task<string> GetEmbeddedResourceAsync(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"未找到嵌入资源: {resourceName}");
                }

                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        /// <summary>
        /// 生成主题预览数据
        /// </summary>
        /// <param name="theme">主题对象</param>
        /// <returns>预览数据</returns>
        private object GeneratePreviewData(Theme theme)
        {
            return new
            {
                ColorScheme = new
                {
                    Primary = theme.Colors?.Primary ?? "#4facfe",
                    Secondary = theme.Colors?.Secondary ?? "#00f2fe",
                    Background = theme.Colors?.Background ?? "#f8f9fa",
                    Surface = theme.Colors?.Surface ?? "#ffffff",
                    Text = theme.Colors?.Text ?? "#333333",
                    Accent = theme.Colors?.Accent ?? "#4facfe"
                },
                Typography = new
                {
                    FontFamily = theme.Typography?.FontFamily ?? "Segoe UI, sans-serif",
                    FontSize = theme.Typography?.FontSize ?? "14px",
                    LineHeight = theme.Typography?.LineHeight ?? "1.5",
                    FontWeight = theme.Typography?.FontWeight ?? "400"
                },
                Layout = new
                {
                    BorderRadius = theme.Layout?.BorderRadius ?? "8px",
                    Spacing = theme.Layout?.Spacing ?? "16px",
                    MaxWidth = theme.Layout?.MaxWidth ?? "1200px"
                },
                SampleElements = new[]
                {
                    new { Type = "Button", Label = "主要按钮", Style = "primary" },
                    new { Type = "Button", Label = "次要按钮", Style = "secondary" },
                    new { Type = "Card", Label = "媒体卡片", Style = "media" },
                    new { Type = "Input", Label = "输入框", Style = "form" },
                    new { Type = "Navigation", Label = "导航菜单", Style = "nav" }
                }
            };
        }
    }

    #region Request/Response Models

    /// <summary>
    /// 获取主题选择界面请求
    /// </summary>
    public class GetThemeSelectionRequest
    {
        // 空请求类，用于路由匹配
    }

    /// <summary>
    /// 获取主题自定义界面请求
    /// </summary>
    public class GetThemeCustomizerRequest
    {
        // 空请求类，用于路由匹配
    }

    /// <summary>
    /// 获取主题预览请求
    /// </summary>
    public class GetThemePreviewRequest
    {
        public string ThemeId { get; set; }
    }

    /// <summary>
    /// 获取主题预览响应
    /// </summary>
    public class GetThemePreviewResponse
    {
        public Theme Theme { get; set; }
        public string Css { get; set; }
        public object PreviewData { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    #endregion
}