using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using EmbyBeautifyPlugin.Services;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using System;
using System.Collections.Generic;
using System.IO;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using EmbyBeautifyPlugin.Extensions;

namespace EmbyBeautifyPlugin
{
    /// <summary>
    /// Emby美化插件主类
    /// 提供主题定制、样式注入、动画效果等功能
    /// </summary>
    public class Plugin : BasePlugin<BeautifyConfig>, IHasWebPages
    {
        private readonly IThemeManager _themeManager;
        private readonly IStyleInjector _styleInjector;
        private readonly IAnimationController _animationController;
        private readonly IInteractionEnhancer _interactionEnhancer;
        private readonly IConfigurationManager _configManager;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly EmbyLoggerProvider _loggerProvider;

        /// <summary>
        /// 插件实例（单例）
        /// </summary>
        public static Plugin Instance { get; private set; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public override string Name => "EmbyBeautifyPlugin";

        /// <summary>
        /// 插件描述
        /// </summary>
        public override string Description => "一个功能强大的Emby主题美化插件，提供丰富的界面定制选项";

        /// <summary>
        /// 插件ID
        /// </summary>
        public override Guid Id => Guid.Parse("12345678-1234-5678-9012-123456789012");

        /// <summary>
        /// 插件版本
        /// </summary>
        public override string Version => "1.0.0";

        /// <summary>
        /// 构造函数
        /// </summary>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            
            // 创建日志提供程序
            _loggerProvider = new EmbyLoggerProvider();
            _logger = _loggerProvider.CreateLogger<Plugin>();
            
            try
            {
                _logger.LogInformation("正在初始化EmbyBeautifyPlugin...");
                
                // 手动实例化核心服务（避免依赖注入问题）
                _configManager = new ConfigurationManager(this, _logger);
                _themeManager = new ThemeManager(_configManager, _logger);
                _styleInjector = new StyleInjector(_themeManager, _logger);
                _animationController = new AnimationController(_configManager, _logger);
                _interactionEnhancer = new InteractionEnhancer(_configManager, _logger);
                
                _logger.LogInformation("EmbyBeautifyPlugin核心服务初始化完成");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "EmbyBeautifyPlugin初始化失败");
                throw;
            }
        }

        /// <summary>
        /// 插件启用时调用
        /// </summary>
        public override void OnEnable()
        {
            try
            {
                _logger.LogInformation("正在启用EmbyBeautifyPlugin...");
                
                // 初始化主题管理器
                _themeManager?.InitializeAsync().GetAwaiter().GetResult();
                
                // 启用样式注入
                _styleInjector?.Enable();
                
                // 启用动画控制器
                _animationController?.Enable();
                
                // 启用交互增强器
                _interactionEnhancer?.Enable();
                
                _logger.LogInformation("EmbyBeautifyPlugin启用成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmbyBeautifyPlugin启用失败");
                throw;
            }
        }

        /// <summary>
        /// 插件禁用时调用
        /// </summary>
        public override void OnDisable()
        {
            try
            {
                _logger.LogInformation("正在禁用EmbyBeautifyPlugin...");
                
                // 禁用交互增强器
                _interactionEnhancer?.Disable();
                
                // 禁用动画控制器
                _animationController?.Disable();
                
                // 禁用样式注入
                _styleInjector?.Disable();
                
                _logger.LogInformation("EmbyBeautifyPlugin禁用成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmbyBeautifyPlugin禁用失败");
            }
        }

        /// <summary>
        /// 获取Web页面
        /// </summary>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "EmbyBeautifyPlugin",
                    EmbeddedResourcePath = GetType().Namespace + ".Views.SettingsPage.html",
                    MenuSection = "server",
                    MenuIcon = "palette"
                },
                new PluginPageInfo
                {
                    Name = "ThemeSelection",
                    EmbeddedResourcePath = GetType().Namespace + ".Views.ThemeSelection.html"
                },
                new PluginPageInfo
                {
                    Name = "ThemeCustomizer",
                    EmbeddedResourcePath = GetType().Namespace + ".Views.ThemeCustomizer.html"
                },
                new PluginPageInfo
                {
                    Name = "PreviewPage",
                    EmbeddedResourcePath = GetType().Namespace + ".Views.PreviewPage.html"
                }
            };
        }

        /// <summary>
        /// 获取主题管理器
        /// </summary>
        public IThemeManager GetThemeManager() => _themeManager;

        /// <summary>
        /// 获取样式注入器
        /// </summary>
        public IStyleInjector GetStyleInjector() => _styleInjector;

        /// <summary>
        /// 获取动画控制器
        /// </summary>
        public IAnimationController GetAnimationController() => _animationController;

        /// <summary>
        /// 获取交互增强器
        /// </summary>
        public IInteractionEnhancer GetInteractionEnhancer() => _interactionEnhancer;

        /// <summary>
        /// 获取配置管理器
        /// </summary>
        public IConfigurationManager GetConfigurationManager() => _configManager;

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _logger?.LogInformation("正在释放EmbyBeautifyPlugin资源...");
                    
                    _interactionEnhancer?.Dispose();
                    _animationController?.Dispose();
                    _styleInjector?.Dispose();
                    _themeManager?.Dispose();
                    _configManager?.Dispose();
                    _loggerProvider?.Dispose();
                    
                    _logger?.LogInformation("EmbyBeautifyPlugin资源释放完成");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "EmbyBeautifyPlugin资源释放失败");
                }
            }
            
            base.Dispose(disposing);
        }
    }
}