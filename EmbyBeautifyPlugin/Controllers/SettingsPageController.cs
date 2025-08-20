using EmbyBeautifyPlugin.Interfaces;
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
    /// 设置页面控制器，用于提供插件管理界面
    /// </summary>
    [Route("/emby-beautify/settings", "GET", Summary = "获取插件设置页面")]
    [Route("/emby-beautify/settings/preview", "GET", Summary = "获取主题预览页面")]
    public class SettingsPageController : IService
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IThemeManager _themeManager;
        private readonly ILogger _logger;

        public SettingsPageController(
            IConfigurationManager configurationManager,
            IThemeManager themeManager,
            ILogManager logManager)
        {
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logManager?.GetLogger(GetType().Name) ?? throw new ArgumentNullException(nameof(logManager));
        }

        /// <summary>
        /// 获取插件设置页面
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>HTML页面内容</returns>
        public async Task<object> Get(GetSettingsPageRequest request)
        {
            try
            {
                _logger.Debug("开始获取设置页面");

                var htmlContent = await GetEmbeddedResourceAsync("EmbyBeautifyPlugin.Views.SettingsPage.html");
                
                // 替换页面中的占位符
                var configuration = await _configurationManager.LoadConfigurationAsync();
                var themes = await _themeManager.GetAvailableThemesAsync();
                
                htmlContent = htmlContent.Replace("{{CURRENT_CONFIG}}", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(configuration, Newtonsoft.Json.Formatting.Indented));
                htmlContent = htmlContent.Replace("{{AVAILABLE_THEMES}}", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(themes, Newtonsoft.Json.Formatting.Indented));

                _logger.Debug("成功获取设置页面");
                
                return htmlContent;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("获取设置页面失败", ex);
                return $"<html><body><h1>错误</h1><p>加载设置页面失败: {ex.Message}</p></body></html>";
            }
        }

        /// <summary>
        /// 获取主题预览页面
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>HTML页面内容</returns>
        public async Task<object> Get(GetPreviewPageRequest request)
        {
            try
            {
                _logger.Debug("开始获取预览页面");

                var htmlContent = await GetEmbeddedResourceAsync("EmbyBeautifyPlugin.Views.PreviewPage.html");
                
                // 获取主题列表用于预览
                var themes = await _themeManager.GetAvailableThemesAsync();
                htmlContent = htmlContent.Replace("{{AVAILABLE_THEMES}}", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(themes, Newtonsoft.Json.Formatting.Indented));

                _logger.Debug("成功获取预览页面");
                
                return htmlContent;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("获取预览页面失败", ex);
                return $"<html><body><h1>错误</h1><p>加载预览页面失败: {ex.Message}</p></body></html>";
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
    }

    #region Request Models

    /// <summary>
    /// 获取设置页面请求
    /// </summary>
    public class GetSettingsPageRequest
    {
        // 空请求类，用于路由匹配
    }

    /// <summary>
    /// 获取预览页面请求
    /// </summary>
    public class GetPreviewPageRequest
    {
        // 空请求类，用于路由匹配
    }

    #endregion
}