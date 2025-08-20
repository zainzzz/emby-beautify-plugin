using EmbyBeautifyPlugin.Abstracts;
using EmbyBeautifyPlugin.Exceptions;
using EmbyBeautifyPlugin.Extensions;
using EmbyBeautifyPlugin.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 样式注入器实现，负责动态CSS注入和样式优先级管理
    /// </summary>
    public class StyleInjector : BaseStyleInjector
    {
        private readonly IThemeManager _themeManager;
        private readonly IConfigurationManager _configurationManager;
        private readonly ErrorHandlingService _errorHandlingService;
        private readonly StyleCacheService _cacheService;
        private readonly ConcurrentDictionary<string, StyleEntry> _styleRegistry;
        private readonly SemaphoreSlim _injectionSemaphore;
        private readonly Timer _cleanupTimer;
        
        // 样式优先级常量
        private const int PRIORITY_SYSTEM = 1000;
        private const int PRIORITY_THEME = 800;
        private const int PRIORITY_USER = 600;
        private const int PRIORITY_CUSTOM = 400;
        private const int PRIORITY_OVERRIDE = 200;

        public StyleInjector(
            ILogger<StyleInjector> logger,
            IThemeManager themeManager,
            IConfigurationManager configurationManager,
            ErrorHandlingService errorHandlingService,
            StyleCacheService cacheService) : base(logger)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            
            _styleRegistry = new ConcurrentDictionary<string, StyleEntry>();
            _injectionSemaphore = new SemaphoreSlim(1, 1);
            
            // 设置定期清理定时器（每30分钟）
            _cleanupTimer = new Timer(CleanupExpiredStyles, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
        }

        /// <summary>
        /// 注入CSS样式到Web客户端
        /// </summary>
        /// <param name="css">要注入的CSS内容</param>
        /// <returns>样式ID，用于后续移除</returns>
        public override async Task InjectStylesAsync(string css)
        {
            await InjectStylesAsync(css, $"style_{Guid.NewGuid():N}", PRIORITY_CUSTOM);
        }

        /// <summary>
        /// 注入CSS样式到Web客户端（带样式ID和优先级）
        /// </summary>
        /// <param name="css">要注入的CSS内容</param>
        /// <param name="styleId">样式ID</param>
        /// <param name="priority">样式优先级</param>
        /// <returns>注入操作的任务</returns>
        public async Task InjectStylesAsync(string css, string styleId, int priority = PRIORITY_CUSTOM)
        {
            if (string.IsNullOrWhiteSpace(css))
            {
                _logger.LogWarning("尝试注入空的CSS内容");
                return;
            }

            if (string.IsNullOrWhiteSpace(styleId))
            {
                styleId = $"style_{Guid.NewGuid():N}";
            }

            using var timer = _logger.BeginOperation("InjectStyles", new Dictionary<string, object>
            {
                ["StyleId"] = styleId,
                ["Priority"] = priority,
                ["CssLength"] = css.Length
            });

            await _injectionSemaphore.WaitAsync();
            try
            {
                // 安全性检查：清理潜在危险的CSS内容
                var sanitizedCss = SanitizeCssContent(css);
                
                // 创建样式条目
                var styleEntry = new StyleEntry
                {
                    Id = styleId,
                    Content = sanitizedCss,
                    Priority = priority,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessed = DateTime.UtcNow
                };

                // 注册样式
                _styleRegistry.AddOrUpdate(styleId, styleEntry, (key, existing) =>
                {
                    existing.Content = sanitizedCss;
                    existing.Priority = priority;
                    existing.LastAccessed = DateTime.UtcNow;
                    return existing;
                });

                // 执行实际的样式注入
                await PerformStyleInjectionAsync(styleEntry);
                
                // 跟踪注入的样式
                TrackInjectedStyle(styleId, sanitizedCss);
                
                _logger.LogDebug("成功注入样式: {StyleId}, 优先级: {Priority}", styleId, priority);
                timer.Complete(true);
            }
            catch (Exception ex)
            {
                timer.Complete(false);
                await _errorHandlingService.HandleExceptionAsync(ex, BeautifyErrorType.StyleInjectionError, 
                    "StyleInjector", "样式注入失败", new Dictionary<string, object>
                    {
                        ["StyleId"] = styleId,
                        ["CssLength"] = css?.Length ?? 0
                    });
                throw;
            }
            finally
            {
                _injectionSemaphore.Release();
            }
        }

        /// <summary>
        /// 移除之前注入的样式
        /// </summary>
        /// <param name="styleId">要移除的样式ID</param>
        public override async Task RemoveStylesAsync(string styleId)
        {
            if (string.IsNullOrWhiteSpace(styleId))
            {
                _logger.LogWarning("尝试移除空的样式ID");
                return;
            }

            using var timer = _logger.BeginOperation("RemoveStyles", new Dictionary<string, object>
            {
                ["StyleId"] = styleId
            });

            await _injectionSemaphore.WaitAsync();
            try
            {
                if (_styleRegistry.TryRemove(styleId, out var styleEntry))
                {
                    // 执行实际的样式移除
                    await PerformStyleRemovalAsync(styleEntry);
                    
                    _logger.LogDebug("成功移除样式: {StyleId}", styleId);
                    timer.Complete(true);
                }
                else
                {
                    _logger.LogWarning("尝试移除不存在的样式: {StyleId}", styleId);
                }

                // 调用基类方法更新跟踪
                await base.RemoveStylesAsync(styleId);
            }
            catch (Exception ex)
            {
                timer.Complete(false);
                await _errorHandlingService.HandleExceptionAsync(ex, BeautifyErrorType.StyleInjectionError,
                    "StyleInjector", "样式移除失败", new Dictionary<string, object>
                    {
                        ["StyleId"] = styleId
                    });
                throw;
            }
            finally
            {
                _injectionSemaphore.Release();
            }
        }

        /// <summary>
        /// 更新所有全局样式
        /// </summary>
        public override async Task UpdateGlobalStylesAsync()
        {
            using var timer = _logger.BeginOperation("UpdateGlobalStyles");

            try
            {
                // 获取当前配置
                var config = await _configurationManager.LoadConfigurationAsync();
                
                // 获取当前主题
                var activeTheme = await _themeManager.GetActiveThemeAsync();
                
                if (activeTheme == null)
                {
                    _logger.LogWarning("没有活动主题，跳过全局样式更新");
                    return;
                }

                // 生成主题CSS
                var themeCss = await _themeManager.GenerateThemeCssAsync(activeTheme);
                
                // 注入主题样式
                await InjectStylesAsync(themeCss, "global_theme", PRIORITY_THEME);
                
                // 注入系统样式
                await InjectSystemStylesAsync();
                
                // 如果启用了动画，注入动画样式
                if (config.EnableAnimations)
                {
                    await InjectAnimationStylesAsync(config.AnimationDuration);
                }

                _logger.LogInformation("全局样式更新完成");
                timer.Complete(true);
            }
            catch (Exception ex)
            {
                timer.Complete(false);
                await _errorHandlingService.HandleExceptionAsync(ex, BeautifyErrorType.StyleInjectionError,
                    "StyleInjector", "全局样式更新失败");
                throw;
            }
        }

        /// <summary>
        /// 获取所有已注入的样式信息
        /// </summary>
        /// <returns>样式信息列表</returns>
        public async Task<List<StyleInfo>> GetInjectedStylesAsync()
        {
            await Task.CompletedTask; // 保持异步签名一致性
            
            return _styleRegistry.Values
                .OrderByDescending(s => s.Priority)
                .ThenBy(s => s.CreatedAt)
                .Select(s => new StyleInfo
                {
                    Id = s.Id,
                    Priority = s.Priority,
                    ContentLength = s.Content.Length,
                    CreatedAt = s.CreatedAt,
                    LastAccessed = s.LastAccessed
                })
                .ToList();
        }

        /// <summary>
        /// 清理过期的样式
        /// </summary>
        /// <param name="maxAge">最大存活时间</param>
        public async Task CleanupStylesAsync(TimeSpan? maxAge = null)
        {
            var cutoffTime = DateTime.UtcNow - (maxAge ?? TimeSpan.FromHours(24));
            var expiredStyles = _styleRegistry.Values
                .Where(s => s.LastAccessed < cutoffTime)
                .ToList();

            foreach (var style in expiredStyles)
            {
                await RemoveStylesAsync(style.Id);
            }

            if (expiredStyles.Any())
            {
                _logger.LogInformation("清理了 {Count} 个过期样式", expiredStyles.Count);
            }
        }

        /// <summary>
        /// 执行实际的样式注入操作
        /// </summary>
        /// <param name="styleEntry">样式条目</param>
        private async Task PerformStyleInjectionAsync(StyleEntry styleEntry)
        {
            // 这里实现实际的样式注入逻辑
            // 在真实的Emby插件中，这会涉及到与Emby Server API的交互
            // 目前作为占位符实现
            
            await Task.Delay(10); // 模拟异步操作
            
            _logger.LogDebug("执行样式注入: {StyleId}, 内容长度: {Length}", 
                styleEntry.Id, styleEntry.Content.Length);
        }

        /// <summary>
        /// 执行实际的样式移除操作
        /// </summary>
        /// <param name="styleEntry">样式条目</param>
        private async Task PerformStyleRemovalAsync(StyleEntry styleEntry)
        {
            // 这里实现实际的样式移除逻辑
            // 在真实的Emby插件中，这会涉及到与Emby Server API的交互
            // 目前作为占位符实现
            
            await Task.Delay(5); // 模拟异步操作
            
            _logger.LogDebug("执行样式移除: {StyleId}", styleEntry.Id);
        }

        /// <summary>
        /// 注入系统级样式
        /// </summary>
        private async Task InjectSystemStylesAsync()
        {
            var systemCss = @"
/* Emby Beautify Plugin - System Styles */
.emby-beautify-enhanced {
    transition: all var(--transition-duration, 0.3s) ease;
}

.emby-beautify-card {
    border-radius: var(--border-radius, 8px);
    box-shadow: var(--box-shadow, 0 2px 8px rgba(0,0,0,0.1));
    overflow: hidden;
}

.emby-beautify-button {
    background: var(--gradient-primary, linear-gradient(135deg, var(--primary-color), var(--accent-color)));
    border: none;
    border-radius: var(--border-radius, 8px);
    color: white;
    cursor: pointer;
    font-weight: 500;
    padding: 12px 24px;
    transition: all var(--transition-duration, 0.3s) ease;
}

.emby-beautify-button:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0,0,0,0.2);
}
";

            await InjectStylesAsync(systemCss, "system_styles", PRIORITY_SYSTEM);
        }

        /// <summary>
        /// 注入动画样式
        /// </summary>
        /// <param name="duration">动画持续时间（毫秒）</param>
        private async Task InjectAnimationStylesAsync(int duration)
        {
            var animationCss = $@"
/* Emby Beautify Plugin - Animation Styles */
:root {{
    --transition-duration: {duration}ms;
    --hover-transform: translateY(-2px);
    --glow-effect: 0 0 20px rgba(var(--primary-color-rgb, 33, 150, 243), 0.3);
}}

.emby-beautify-fade-in {{
    animation: embyBeautifyFadeIn {duration}ms ease-out;
}}

.emby-beautify-slide-up {{
    animation: embyBeautifySlideUp {duration}ms ease-out;
}}

@keyframes embyBeautifyFadeIn {{
    from {{ opacity: 0; }}
    to {{ opacity: 1; }}
}}

@keyframes embyBeautifySlideUp {{
    from {{ 
        opacity: 0; 
        transform: translateY(20px); 
    }}
    to {{ 
        opacity: 1; 
        transform: translateY(0); 
    }}
}}

.emby-beautify-hover-glow:hover {{
    box-shadow: var(--glow-effect);
}}
";

            await InjectStylesAsync(animationCss, "animation_styles", PRIORITY_SYSTEM);
        }

        /// <summary>
        /// 清理CSS内容，移除潜在危险的代码
        /// </summary>
        /// <param name="css">原始CSS内容</param>
        /// <returns>清理后的CSS内容</returns>
        private string SanitizeCssContent(string css)
        {
            if (string.IsNullOrWhiteSpace(css))
                return string.Empty;

            // 移除潜在危险的CSS属性和值
            var dangerousPatterns = new[]
            {
                @"javascript\s*:",
                @"expression\s*\(",
                @"@import\s+url\s*\(",
                @"url\s*\(\s*[""']?(?!data:)[^""']*[""']?\s*\)",
                @"<script[^>]*>.*?</script>",
                @"<iframe[^>]*>.*?</iframe>",
                @"<object[^>]*>.*?</object>",
                @"<embed[^>]*>.*?</embed>"
            };

            var sanitized = css;
            foreach (var pattern in dangerousPatterns)
            {
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            return sanitized;
        }

        /// <summary>
        /// 生成客户端样式注入脚本
        /// </summary>
        /// <returns>完整的客户端脚本内容</returns>
        public async Task<string> GenerateClientScriptAsync()
        {
            using var timer = _logger.BeginOperation("GenerateClientScript");

            try
            {
                var scriptBuilder = new StringBuilder();
                
                // 添加基础脚本
                scriptBuilder.AppendLine(await LoadBaseScriptAsync());
                
                // 添加配置数据
                var config = await _configurationManager.LoadConfigurationAsync();
                var configJson = System.Text.Json.JsonSerializer.Serialize(config);
                scriptBuilder.AppendLine($"window.EmbyBeautifyConfig = {configJson};");
                
                // 添加主题数据
                var activeTheme = await _themeManager.GetActiveThemeAsync();
                if (activeTheme != null)
                {
                    var themeJson = System.Text.Json.JsonSerializer.Serialize(activeTheme);
                    scriptBuilder.AppendLine($"window.EmbyBeautifyActiveTheme = {themeJson};");
                }
                
                // 添加可用主题列表
                var availableThemes = await _themeManager.GetAvailableThemesAsync();
                var themesJson = System.Text.Json.JsonSerializer.Serialize(availableThemes);
                scriptBuilder.AppendLine($"window.EmbyBeautifyAvailableThemes = {themesJson};");
                
                // 添加初始化脚本
                scriptBuilder.AppendLine(@"
// 自动初始化样式注入器
(function() {
    if (typeof window.EmbyBeautifyStyleInjector !== 'undefined') {
        window.EmbyBeautifyStyleInjector.config.debugMode = " + (config.CustomSettings?.ContainsKey("DebugMode") == true ? "true" : "false") + @";
        
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                window.EmbyBeautifyStyleInjector.init();
            });
        } else {
            window.EmbyBeautifyStyleInjector.init();
        }
    }
})();");

                var script = scriptBuilder.ToString();
                _logger.LogDebug("生成客户端脚本完成，长度: {Length}", script.Length);
                timer.Complete(true);
                
                return script;
            }
            catch (Exception ex)
            {
                timer.Complete(false);
                await _errorHandlingService.HandleExceptionAsync(ex, BeautifyErrorType.StyleInjectionError,
                    "StyleInjector", "生成客户端脚本失败");
                
                // 返回基础脚本作为回退
                return await GenerateBasicClientScriptAsync();
            }
        }

        /// <summary>
        /// 生成主题CSS
        /// </summary>
        /// <param name="theme">主题对象</param>
        /// <returns>生成的CSS内容</returns>
        public async Task<string> GenerateThemeCssAsync(Models.Theme theme)
        {
            if (theme == null)
                return string.Empty;

            // 生成缓存键
            var config = await _configurationManager.LoadConfigurationAsync();
            var cacheKey = _cacheService.GenerateCacheKey(theme, config, "theme-css");

            // 尝试从缓存获取
            var cachedCss = await _cacheService.GetCachedStyleAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedCss))
            {
                _logger.LogDebug("从缓存获取主题CSS: {ThemeId}", theme.Id);
                return cachedCss;
            }

            // 生成新的CSS
            var cssBuilder = new StringBuilder();
            
            // 生成CSS自定义属性
            if (theme.Colors != null)
            {
                cssBuilder.AppendLine(":root {");
                
                if (!string.IsNullOrEmpty(theme.Colors.Primary))
                    cssBuilder.AppendLine($"  --emby-beautify-primary: {theme.Colors.Primary};");
                if (!string.IsNullOrEmpty(theme.Colors.Secondary))
                    cssBuilder.AppendLine($"  --emby-beautify-secondary: {theme.Colors.Secondary};");
                if (!string.IsNullOrEmpty(theme.Colors.Background))
                    cssBuilder.AppendLine($"  --emby-beautify-background: {theme.Colors.Background};");
                if (!string.IsNullOrEmpty(theme.Colors.Surface))
                    cssBuilder.AppendLine($"  --emby-beautify-surface: {theme.Colors.Surface};");
                if (!string.IsNullOrEmpty(theme.Colors.Text))
                    cssBuilder.AppendLine($"  --emby-beautify-text: {theme.Colors.Text};");
                if (!string.IsNullOrEmpty(theme.Colors.Accent))
                    cssBuilder.AppendLine($"  --emby-beautify-accent: {theme.Colors.Accent};");
                
                cssBuilder.AppendLine("}");
            }
            
            // 生成字体样式
            if (theme.Typography != null)
            {
                if (!string.IsNullOrEmpty(theme.Typography.FontFamily))
                {
                    cssBuilder.AppendLine($"body, .page {{ font-family: {theme.Typography.FontFamily}; }}");
                }
                if (!string.IsNullOrEmpty(theme.Typography.FontSize))
                {
                    cssBuilder.AppendLine($"body {{ font-size: {theme.Typography.FontSize}; }}");
                }
                if (!string.IsNullOrEmpty(theme.Typography.LineHeight))
                {
                    cssBuilder.AppendLine($"body {{ line-height: {theme.Typography.LineHeight}; }}");
                }
            }
            
            var css = cssBuilder.ToString();

            // 缓存生成的CSS
            await _cacheService.SetCachedStyleAsync(cacheKey, css, TimeSpan.FromHours(24));
            
            _logger.LogDebug("生成并缓存主题CSS: {ThemeId}", theme.Id);
            return css;
        }

        /// <summary>
        /// 生成响应式CSS
        /// </summary>
        /// <param name="theme">主题对象</param>
        /// <returns>响应式CSS内容</returns>
        public async Task<string> GenerateResponsiveCssAsync(Models.Theme theme)
        {
            var config = await _configurationManager.LoadConfigurationAsync();
            
            // 生成缓存键
            var cacheKey = _cacheService.GenerateCacheKey(theme, config, "responsive-css");

            // 尝试从缓存获取
            var cachedCss = await _cacheService.GetCachedStyleAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedCss))
            {
                _logger.LogDebug("从缓存获取响应式CSS");
                return cachedCss;
            }

            var cssBuilder = new StringBuilder();
            
            if (config.ResponsiveSettings != null)
            {
                // 移动端样式
                if (config.ResponsiveSettings.Mobile != null)
                {
                    cssBuilder.AppendLine($"@media (max-width: {config.ResponsiveSettings.Mobile.MaxWidth}px) {{");
                    cssBuilder.AppendLine("  .emby-beautify-card { margin: 4px; border-radius: 4px; }");
                    cssBuilder.AppendLine("  .page { padding: 12px; }");
                    cssBuilder.AppendLine("}");
                }
                
                // 平板端样式
                if (config.ResponsiveSettings.Tablet != null)
                {
                    cssBuilder.AppendLine($"@media (max-width: {config.ResponsiveSettings.Tablet.MaxWidth}px) {{");
                    cssBuilder.AppendLine("  .emby-beautify-card { margin: 8px; border-radius: 6px; }");
                    cssBuilder.AppendLine("  .emby-beautify-button { padding: 8px 16px; font-size: 14px; }");
                    cssBuilder.AppendLine("}");
                }
            }
            
            var css = cssBuilder.ToString();

            // 缓存生成的CSS
            await _cacheService.SetCachedStyleAsync(cacheKey, css, TimeSpan.FromHours(12));
            
            return css;
        }

        /// <summary>
        /// 生成动画CSS
        /// </summary>
        /// <returns>动画CSS内容</returns>
        public async Task<string> GenerateAnimationCssAsync()
        {
            var config = await _configurationManager.LoadConfigurationAsync();
            
            // 生成缓存键
            var cacheKey = _cacheService.GenerateCacheKey(null, config, "animation-css");

            // 尝试从缓存获取
            var cachedCss = await _cacheService.GetCachedStyleAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedCss))
            {
                _logger.LogDebug("从缓存获取动画CSS");
                return cachedCss;
            }

            string css;
            
            if (!config.EnableAnimations)
            {
                css = @"
/* 禁用动画 */
* {
    transition: none !important;
    animation: none !important;
}";
            }
            else
            {
                var duration = config.AnimationDuration / 1000.0; // 转换为秒
                
                css = $@"
/* 动画样式 */
@keyframes emby-beautify-fadeIn {{
    from {{ opacity: 0; transform: translateY(10px); }}
    to {{ opacity: 1; transform: translateY(0); }}
}}

@keyframes emby-beautify-slideIn {{
    from {{ transform: translateX(-20px); opacity: 0; }}
    to {{ transform: translateX(0); opacity: 1; }}
}}

.emby-beautify-animate-in {{
    animation: emby-beautify-fadeIn {duration}s ease-out;
}}

.emby-beautify-slide-in {{
    animation: emby-beautify-slideIn {duration * 1.2}s ease-out;
}}

.emby-beautify-enhanced {{
    transition: all {duration}s ease;
}}";
            }

            // 缓存生成的CSS
            await _cacheService.SetCachedStyleAsync(cacheKey, css, TimeSpan.FromHours(6));
            
            return css;
        }

        /// <summary>
        /// 生成断点CSS
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        /// <param name="breakpointSettings">断点设置</param>
        /// <returns>断点CSS内容</returns>
        public async Task<string> GenerateBreakpointCssAsync(string deviceType, Models.BreakpointSettings breakpointSettings)
        {
            await Task.CompletedTask; // 保持异步签名
            
            if (breakpointSettings == null)
                return string.Empty;
                
            return $@"
@media (max-width: {breakpointSettings.MaxWidth}px) {{
    /* {deviceType} 样式 */
    .emby-beautify-{deviceType} {{
        display: block;
    }}
    
    .emby-beautify-hide-{deviceType} {{
        display: none;
    }}
}}";
        }

        /// <summary>
        /// 优化CSS内容
        /// </summary>
        /// <param name="css">原始CSS</param>
        /// <returns>优化后的CSS</returns>
        public async Task<string> OptimizeCssAsync(string css)
        {
            await Task.CompletedTask; // 保持异步签名
            
            if (string.IsNullOrWhiteSpace(css))
                return string.Empty;
            
            // 移除多余的空白字符
            css = Regex.Replace(css, @"\s+", " ");
            
            // 移除注释
            css = Regex.Replace(css, @"/\*.*?\*/", "", RegexOptions.Singleline);
            
            // 移除多余的分号
            css = Regex.Replace(css, @";\s*}", "}");
            
            // 移除空行
            css = Regex.Replace(css, @"\n\s*\n", "\n");
            
            return css.Trim();
        }

        /// <summary>
        /// 加载基础脚本内容
        /// </summary>
        /// <returns>基础脚本内容</returns>
        private async Task<string> LoadBaseScriptAsync()
        {
            try
            {
                var scriptPath = Path.Combine("Views", "js", "style-injector.js");
                if (File.Exists(scriptPath))
                {
                    return await File.ReadAllTextAsync(scriptPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "无法加载基础脚本文件");
            }
            
            // 返回内联的基础脚本
            return @"
// Emby 美化插件 - 基础样式注入器
(function(window, document) {
    'use strict';
    
    window.EmbyBeautifyStyleInjector = {
        config: { debugMode: false },
        init: function() {
            console.log('Emby Beautify Style Injector initialized');
            return Promise.resolve();
        },
        injectStyle: function(id, css) {
            var styleElement = document.getElementById('emby-beautify-' + id);
            if (!styleElement) {
                styleElement = document.createElement('style');
                styleElement.id = 'emby-beautify-' + id;
                styleElement.type = 'text/css';
                document.head.appendChild(styleElement);
            }
            styleElement.textContent = css;
        },
        updateStyles: function() {
            return Promise.resolve();
        }
    };
})(window, document);";
        }

        /// <summary>
        /// 生成基础客户端脚本（回退方案）
        /// </summary>
        /// <returns>基础脚本内容</returns>
        private async Task<string> GenerateBasicClientScriptAsync()
        {
            return await LoadBaseScriptAsync();
        }

        /// <summary>
        /// 清理主题相关的缓存
        /// </summary>
        /// <param name="themeId">主题ID，如果为null则清理所有主题缓存</param>
        public async Task InvalidateThemeCacheAsync(string themeId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(themeId))
                {
                    // 清理所有缓存
                    await _cacheService.ClearAllCacheAsync();
                    _logger.LogInformation("已清理所有主题缓存");
                }
                else
                {
                    // 清理特定主题的缓存
                    var config = await _configurationManager.LoadConfigurationAsync();
                    var theme = await _themeManager.GetThemeByIdAsync(themeId);
                    
                    if (theme != null)
                    {
                        var cacheKeys = new[]
                        {
                            _cacheService.GenerateCacheKey(theme, config, "theme-css"),
                            _cacheService.GenerateCacheKey(theme, config, "responsive-css")
                        };

                        foreach (var cacheKey in cacheKeys)
                        {
                            await _cacheService.RemoveCachedStyleAsync(cacheKey);
                        }
                        
                        _logger.LogInformation("已清理主题 {ThemeId} 的缓存", themeId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理主题缓存时发生错误: {ThemeId}", themeId);
            }
        }

        /// <summary>
        /// 清理配置相关的缓存
        /// </summary>
        public async Task InvalidateConfigurationCacheAsync()
        {
            try
            {
                // 配置更改会影响所有缓存，因此清理所有缓存
                await _cacheService.ClearAllCacheAsync();
                _logger.LogInformation("已清理配置相关的缓存");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理配置缓存时发生错误");
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns>缓存统计信息</returns>
        public async Task<CacheStatistics> GetCacheStatisticsAsync()
        {
            return await _cacheService.GetCacheStatisticsAsync();
        }

        /// <summary>
        /// 预热缓存
        /// </summary>
        /// <returns>预热任务</returns>
        public async Task WarmupCacheAsync()
        {
            try
            {
                _logger.LogInformation("开始预热样式缓存...");

                var config = await _configurationManager.LoadConfigurationAsync();
                var themes = await _themeManager.GetAvailableThemesAsync();

                var warmupTasks = new List<Task>();

                foreach (var theme in themes)
                {
                    warmupTasks.Add(GenerateThemeCssAsync(theme));
                    warmupTasks.Add(GenerateResponsiveCssAsync(theme));
                }

                // 预热动画CSS
                warmupTasks.Add(GenerateAnimationCssAsync());

                await Task.WhenAll(warmupTasks);

                _logger.LogInformation("样式缓存预热完成，预热了 {ThemeCount} 个主题", themes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预热缓存时发生错误");
            }
        }

        /// <summary>
        /// 定期清理过期样式的回调方法
        /// </summary>
        /// <param name="state">定时器状态</param>
        private async void CleanupExpiredStyles(object state)
        {
            try
            {
                await CleanupStylesAsync();
                await _cacheService.CleanupExpiredCacheAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定期清理过期样式时发生错误");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cleanupTimer?.Dispose();
                _injectionSemaphore?.Dispose();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 样式条目类
    /// </summary>
    internal class StyleEntry
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
    }

    /// <summary>
    /// 样式信息类
    /// </summary>
    public class StyleInfo
    {
        public string Id { get; set; }
        public int Priority { get; set; }
        public int ContentLength { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}