using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 客户端脚本加载器
    /// 负责管理和加载客户端JavaScript脚本
    /// </summary>
    public class ClientScriptLoader
    {
        private readonly ILogger<ClientScriptLoader> _logger;
        private readonly StyleInjector _styleInjector;
        private readonly IConfigurationManager _configurationManager;
        private readonly Dictionary<string, string> _scriptCache;
        private readonly object _cacheLock = new object();

        public ClientScriptLoader(
            ILogger<ClientScriptLoader> logger,
            StyleInjector styleInjector,
            IConfigurationManager configurationManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _styleInjector = styleInjector ?? throw new ArgumentNullException(nameof(styleInjector));
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            _scriptCache = new Dictionary<string, string>();
        }

        /// <summary>
        /// 获取完整的客户端脚本包
        /// </summary>
        /// <returns>包含所有必要脚本的字符串</returns>
        public async Task<string> GetClientScriptBundleAsync()
        {
            _logger.LogDebug("开始生成客户端脚本包");

            var scriptBuilder = new StringBuilder();

            try
            {
                // 添加浏览器兼容性脚本
                var compatibilityScript = await LoadScriptAsync("browser-compatibility.js");
                if (!string.IsNullOrEmpty(compatibilityScript))
                {
                    scriptBuilder.AppendLine("/* 浏览器兼容性脚本 */");
                    scriptBuilder.AppendLine(compatibilityScript);
                    scriptBuilder.AppendLine();
                }

                // 添加样式注入器脚本
                var styleInjectorScript = await LoadScriptAsync("style-injector.js");
                if (!string.IsNullOrEmpty(styleInjectorScript))
                {
                    scriptBuilder.AppendLine("/* 样式注入器脚本 */");
                    scriptBuilder.AppendLine(styleInjectorScript);
                    scriptBuilder.AppendLine();
                }

                // 添加配置和初始化脚本
                var initScript = await _styleInjector.GenerateClientScriptAsync();
                if (!string.IsNullOrEmpty(initScript))
                {
                    scriptBuilder.AppendLine("/* 配置和初始化脚本 */");
                    scriptBuilder.AppendLine(initScript);
                    scriptBuilder.AppendLine();
                }

                // 添加错误处理包装
                var wrappedScript = WrapScriptWithErrorHandling(scriptBuilder.ToString());

                _logger.LogDebug("客户端脚本包生成完成，总长度: {Length}", wrappedScript.Length);
                return wrappedScript;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成客户端脚本包时发生错误");
                return await GetFallbackScriptAsync();
            }
        }

        /// <summary>
        /// 获取特定的脚本文件内容
        /// </summary>
        /// <param name="scriptName">脚本文件名</param>
        /// <returns>脚本内容</returns>
        public async Task<string> GetScriptAsync(string scriptName)
        {
            if (string.IsNullOrWhiteSpace(scriptName))
            {
                _logger.LogWarning("请求的脚本名称为空");
                return string.Empty;
            }

            try
            {
                return await LoadScriptAsync(scriptName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载脚本 {ScriptName} 时发生错误", scriptName);
                return string.Empty;
            }
        }

        /// <summary>
        /// 生成内联脚本标签
        /// </summary>
        /// <param name="includeWrapper">是否包含script标签包装</param>
        /// <returns>完整的script标签内容</returns>
        public async Task<string> GenerateInlineScriptTagAsync(bool includeWrapper = true)
        {
            var scriptContent = await GetClientScriptBundleAsync();
            
            if (!includeWrapper)
            {
                return scriptContent;
            }

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("<script type=\"text/javascript\">");
            scriptBuilder.AppendLine("/* Emby 美化插件 - 客户端脚本 */");
            scriptBuilder.AppendLine("(function() {");
            scriptBuilder.AppendLine("'use strict';");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine(scriptContent);
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("})();");
            scriptBuilder.AppendLine("</script>");

            return scriptBuilder.ToString();
        }

        /// <summary>
        /// 生成外部脚本引用标签
        /// </summary>
        /// <param name="scriptUrl">脚本URL</param>
        /// <param name="async">是否异步加载</param>
        /// <param name="defer">是否延迟执行</param>
        /// <returns>script标签</returns>
        public string GenerateExternalScriptTag(string scriptUrl, bool async = true, bool defer = false)
        {
            var attributes = new List<string>
            {
                $"src=\"{scriptUrl}\"",
                "type=\"text/javascript\""
            };

            if (async)
                attributes.Add("async");
            
            if (defer)
                attributes.Add("defer");

            return $"<script {string.Join(" ", attributes)}></script>";
        }

        /// <summary>
        /// 检查脚本是否需要更新
        /// </summary>
        /// <param name="scriptName">脚本名称</param>
        /// <returns>是否需要更新</returns>
        public async Task<bool> IsScriptUpdateNeededAsync(string scriptName)
        {
            try
            {
                var scriptPath = GetScriptPath(scriptName);
                if (!File.Exists(scriptPath))
                {
                    return true; // 文件不存在，需要更新
                }

                var lastModified = File.GetLastWriteTimeUtc(scriptPath);
                var config = await _configurationManager.LoadConfigurationAsync();
                
                // 如果配置更新时间比脚本文件新，则需要更新
                return lastModified < DateTime.UtcNow.AddMinutes(-5); // 5分钟的缓存时间
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "检查脚本更新状态时发生错误: {ScriptName}", scriptName);
                return true; // 出错时默认需要更新
            }
        }

        /// <summary>
        /// 清理脚本缓存
        /// </summary>
        public void ClearScriptCache()
        {
            lock (_cacheLock)
            {
                _scriptCache.Clear();
                _logger.LogDebug("脚本缓存已清理");
            }
        }

        /// <summary>
        /// 预加载所有脚本到缓存
        /// </summary>
        /// <returns>预加载任务</returns>
        public async Task PreloadScriptsAsync()
        {
            _logger.LogDebug("开始预加载脚本");

            var scriptNames = new[]
            {
                "browser-compatibility.js",
                "style-injector.js"
            };

            var tasks = new List<Task>();
            foreach (var scriptName in scriptNames)
            {
                tasks.Add(LoadScriptAsync(scriptName));
            }

            await Task.WhenAll(tasks);
            _logger.LogDebug("脚本预加载完成");
        }

        /// <summary>
        /// 加载脚本文件
        /// </summary>
        /// <param name="scriptName">脚本文件名</param>
        /// <returns>脚本内容</returns>
        private async Task<string> LoadScriptAsync(string scriptName)
        {
            // 检查缓存
            lock (_cacheLock)
            {
                if (_scriptCache.TryGetValue(scriptName, out var cachedScript))
                {
                    return cachedScript;
                }
            }

            var scriptPath = GetScriptPath(scriptName);
            
            if (!File.Exists(scriptPath))
            {
                _logger.LogWarning("脚本文件不存在: {ScriptPath}", scriptPath);
                return string.Empty;
            }

            try
            {
                var scriptContent = await File.ReadAllTextAsync(scriptPath);
                
                // 添加到缓存
                lock (_cacheLock)
                {
                    _scriptCache[scriptName] = scriptContent;
                }

                _logger.LogDebug("成功加载脚本: {ScriptName}, 长度: {Length}", scriptName, scriptContent.Length);
                return scriptContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取脚本文件失败: {ScriptPath}", scriptPath);
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取脚本文件路径
        /// </summary>
        /// <param name="scriptName">脚本文件名</param>
        /// <returns>完整路径</returns>
        private string GetScriptPath(string scriptName)
        {
            return Path.Combine("Views", "js", scriptName);
        }

        /// <summary>
        /// 用错误处理包装脚本
        /// </summary>
        /// <param name="script">原始脚本</param>
        /// <returns>包装后的脚本</returns>
        private string WrapScriptWithErrorHandling(string script)
        {
            return $@"
try {{
    {script}
}} catch (error) {{
    console.error('[Emby Beautify Plugin] 脚本执行错误:', error);
    
    // 尝试基础功能
    if (typeof window.EmbyBeautifyStyleInjector === 'undefined') {{
        window.EmbyBeautifyStyleInjector = {{
            init: function() {{ 
                console.log('Emby Beautify Plugin - 基础模式'); 
                return Promise.resolve(); 
            }},
            injectStyle: function(id, css) {{
                var style = document.createElement('style');
                style.id = 'emby-beautify-' + id;
                style.textContent = css;
                document.head.appendChild(style);
            }}
        }};
    }}
}}";
        }

        /// <summary>
        /// 获取回退脚本
        /// </summary>
        /// <returns>基础回退脚本</returns>
        private async Task<string> GetFallbackScriptAsync()
        {
            return @"
// Emby 美化插件 - 回退脚本
(function() {
    'use strict';
    
    console.log('Emby Beautify Plugin - 回退模式');
    
    window.EmbyBeautifyStyleInjector = {
        config: { debugMode: false },
        init: function() {
            console.log('Emby Beautify Style Injector - 基础初始化');
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
            if (styleElement.styleSheet) {
                styleElement.styleSheet.cssText = css;
            } else {
                styleElement.textContent = css;
            }
        },
        removeStyle: function(id) {
            var styleElement = document.getElementById('emby-beautify-' + id);
            if (styleElement) {
                styleElement.remove();
            }
        },
        updateStyles: function() {
            return Promise.resolve();
        }
    };
    
    // 自动初始化
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            window.EmbyBeautifyStyleInjector.init();
        });
    } else {
        window.EmbyBeautifyStyleInjector.init();
    }
})();";
        }
    }
}