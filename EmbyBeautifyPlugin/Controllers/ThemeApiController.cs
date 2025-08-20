using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Controllers
{
    /// <summary>
    /// API控制器，用于管理主题相关的操作
    /// </summary>
    [Route("/emby-beautify/themes", "GET", Summary = "获取所有可用主题")]
    [Route("/emby-beautify/themes/{ThemeId}", "GET", Summary = "获取指定主题")]
    [Route("/emby-beautify/themes/active", "GET", Summary = "获取当前活动主题")]
    [Route("/emby-beautify/themes/active", "POST", Summary = "设置活动主题")]
    [Route("/emby-beautify/themes/{ThemeId}/css", "GET", Summary = "获取主题CSS")]
    public class ThemeApiController : IService
    {
        private readonly IThemeManager _themeManager;
        private readonly ILogger _logger;

        public ThemeApiController(IThemeManager themeManager, ILogManager logManager)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logManager?.GetLogger(GetType().Name) ?? throw new ArgumentNullException(nameof(logManager));
        }

        /// <summary>
        /// 获取所有可用主题列表
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>主题列表</returns>
        public async Task<object> Get(GetThemesRequest request)
        {
            try
            {
                _logger.Debug("开始获取主题列表");

                var themes = await _themeManager.GetAvailableThemesAsync();
                
                var response = new GetThemesResponse
                {
                    Themes = themes,
                    Count = themes.Count
                };

                _logger.Debug("成功获取主题列表，共 {0} 个主题", themes.Count);
                return response;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("获取主题列表失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 获取指定主题详情
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>主题详情</returns>
        public async Task<object> Get(GetThemeRequest request)
        {
            try
            {
                _logger.Debug("开始获取主题详情，主题ID: {0}", request.ThemeId);

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

                _logger.Debug("成功获取主题详情: {0}", request.ThemeId);
                return theme;
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"获取主题 '{request.ThemeId}' 失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 获取当前活动主题
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>当前活动主题</returns>
        public async Task<object> Get(GetActiveThemeRequest request)
        {
            try
            {
                _logger.Debug("开始获取活动主题");

                var activeTheme = await _themeManager.GetActiveThemeAsync();
                
                if (activeTheme == null)
                {
                    throw new InvalidOperationException("未找到活动主题");
                }

                _logger.Debug("成功获取活动主题: {0}", activeTheme.Id);
                return activeTheme;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("获取活动主题失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 设置活动主题
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>操作结果</returns>
        public async Task<object> Post(SetActiveThemeRequest request)
        {
            try
            {
                _logger.Debug("开始设置活动主题: {0}", request.ThemeId);

                if (string.IsNullOrWhiteSpace(request.ThemeId))
                {
                    throw new ArgumentException("主题ID不能为空", nameof(request.ThemeId));
                }

                await _themeManager.SetActiveThemeAsync(request.ThemeId);
                var newActiveTheme = await _themeManager.GetActiveThemeAsync();

                var response = new SetActiveThemeResponse
                {
                    Success = true,
                    Message = $"主题已成功切换到 '{newActiveTheme.Name}'",
                    ActiveTheme = newActiveTheme
                };

                _logger.Info("成功设置活动主题: {0}", request.ThemeId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"设置活动主题 '{request.ThemeId}' 失败", ex);
                
                return new SetActiveThemeResponse
                {
                    Success = false,
                    Message = $"设置主题失败: {ex.Message}",
                    ActiveTheme = null
                };
            }
        }

        /// <summary>
        /// 获取主题的CSS样式
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>CSS样式字符串</returns>
        public async Task<object> Get(GetThemeCssRequest request)
        {
            try
            {
                _logger.Debug("开始获取主题CSS: {0}", request.ThemeId);

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

                var css = await _themeManager.GenerateThemeCssAsync(theme);

                var response = new GetThemeCssResponse
                {
                    ThemeId = request.ThemeId,
                    Css = css,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger.Debug("成功获取主题CSS: {0}，长度: {1}", request.ThemeId, css.Length);
                return response;
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"获取主题CSS '{request.ThemeId}' 失败", ex);
                throw;
            }
        }
    }

    #region Request/Response Models

    /// <summary>
    /// 获取主题列表请求
    /// </summary>
    public class GetThemesRequest
    {
        // 可以添加过滤参数，如分页、搜索等
    }

    /// <summary>
    /// 获取主题列表响应
    /// </summary>
    public class GetThemesResponse
    {
        public List<Theme> Themes { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// 获取指定主题请求
    /// </summary>
    public class GetThemeRequest
    {
        public string ThemeId { get; set; }
    }

    /// <summary>
    /// 获取活动主题请求
    /// </summary>
    public class GetActiveThemeRequest
    {
        // 空请求类，用于路由匹配
    }

    /// <summary>
    /// 设置活动主题请求
    /// </summary>
    public class SetActiveThemeRequest
    {
        public string ThemeId { get; set; }
    }

    /// <summary>
    /// 设置活动主题响应
    /// </summary>
    public class SetActiveThemeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Theme ActiveTheme { get; set; }
    }

    /// <summary>
    /// 获取主题CSS请求
    /// </summary>
    public class GetThemeCssRequest
    {
        public string ThemeId { get; set; }
    }

    /// <summary>
    /// 获取主题CSS响应
    /// </summary>
    public class GetThemeCssResponse
    {
        public string ThemeId { get; set; }
        public string Css { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    #endregion
}