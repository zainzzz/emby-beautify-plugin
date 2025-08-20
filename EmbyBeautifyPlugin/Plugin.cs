using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Logging;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace EmbyBeautifyPlugin
{
    /// <summary>
    /// Main plugin entry point for Emby Beautify Plugin
    /// Implements IServerEntryPoint for Emby plugin framework integration
    /// </summary>
    public class Plugin : IEmbyBeautifyPlugin, IServerEntryPoint
    {
        private readonly ILogger _logger;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private IThemeManager _themeManager;
        private IStyleInjector _styleInjector;
        private IConfigurationManager _configurationManager;
        private bool _isInitialized = false;
        private readonly object _initializationLock = new object();

        public Plugin(ILogManager logManager, IServerConfigurationManager serverConfigurationManager)
        {
            _logger = logManager?.GetLogger(GetType().Name) ?? throw new ArgumentNullException(nameof(logManager));
            _serverConfigurationManager = serverConfigurationManager ?? throw new ArgumentNullException(nameof(serverConfigurationManager));
        }

        /// <summary>
        /// Plugin name
        /// </summary>
        public string Name => "Emby Beautify Plugin";

        /// <summary>
        /// Plugin description
        /// </summary>
        public string Description => "A plugin to beautify Emby Server interface with custom themes and enhanced UI";

        /// <summary>
        /// Run the plugin - called by Emby Server on startup
        /// </summary>
        public void Run()
        {
            try
            {
                _logger.Info("Starting Emby Beautify Plugin v1.0.0");
                RunAsync().GetAwaiter().GetResult();
                _logger.Info("Emby Beautify Plugin started successfully");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Critical error starting Emby Beautify Plugin", ex);
                throw;
            }
        }

        /// <summary>
        /// Run the plugin asynchronously - internal implementation
        /// </summary>
        private async Task RunAsync()
        {
            await InitializeAsync();
        }

        /// <summary>
        /// Initialize the plugin asynchronously
        /// </summary>
        public async Task InitializeAsync()
        {
            lock (_initializationLock)
            {
                if (_isInitialized)
                {
                    _logger.Debug("Plugin already initialized, skipping");
                    return;
                }
            }

            try
            {
                _logger.Debug("Initializing Emby Beautify Plugin components");

                // Initialize dependency injection container and services
                await InitializeServicesAsync();

                // Load and validate configuration
                var config = await _configurationManager.LoadConfigurationAsync();
                _logger.Debug("Configuration loaded successfully. Active theme: {0}", config.ActiveThemeId);

                // Initialize and apply active theme
                await InitializeThemeSystemAsync(config);

                // Update global styles
                await _styleInjector.UpdateGlobalStylesAsync();

                lock (_initializationLock)
                {
                    _isInitialized = true;
                }

                _logger.Info("Plugin initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error during plugin initialization", ex);
                
                // Attempt graceful degradation
                await HandleInitializationFailureAsync(ex);
                throw;
            }
        }

        /// <summary>
        /// Initialize plugin services and dependencies
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            try
            {
                _logger.Debug("Initializing plugin services");

                // TODO: Initialize actual implementations when they are created
                // For now, we'll use placeholder implementations
                
                // This will be replaced with actual dependency injection container
                // _configurationManager = serviceProvider.GetService<IConfigurationManager>();
                // _themeManager = serviceProvider.GetService<IThemeManager>();
                // _styleInjector = serviceProvider.GetService<IStyleInjector>();

                _logger.Debug("Plugin services initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Failed to initialize plugin services", ex);
                throw;
            }
        }

        /// <summary>
        /// Initialize theme system and apply active theme
        /// </summary>
        private async Task InitializeThemeSystemAsync(BeautifyConfig config)
        {
            try
            {
                if (_themeManager == null)
                {
                    _logger.Warn("Theme manager not initialized, skipping theme application");
                    return;
                }

                _logger.Debug("Initializing theme system");

                // Get available themes
                var availableThemes = await _themeManager.GetAvailableThemesAsync();
                _logger.Debug("Found {0} available themes", availableThemes.Count);

                // Apply active theme
                var activeTheme = await _themeManager.GetActiveThemeAsync();
                if (activeTheme != null)
                {
                    _logger.Debug("Applying active theme: {0}", activeTheme.Name);
                    var css = await _themeManager.GenerateThemeCssAsync(activeTheme);
                    await _styleInjector.InjectStylesAsync(css);
                    _logger.Info("Active theme '{0}' applied successfully", activeTheme.Name);
                }
                else
                {
                    _logger.Warn("No active theme found, using default styling");
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error initializing theme system", ex);
                throw;
            }
        }

        /// <summary>
        /// Handle initialization failure with graceful degradation
        /// </summary>
        private async Task HandleInitializationFailureAsync(Exception ex)
        {
            try
            {
                _logger.Warn("Attempting graceful degradation due to initialization failure");
                
                // Try to apply minimal default styling
                if (_styleInjector != null)
                {
                    var fallbackCss = GetFallbackCss();
                    await _styleInjector.InjectStylesAsync(fallbackCss);
                    _logger.Info("Fallback styling applied");
                }

                _logger.Warn("Plugin running in degraded mode due to initialization error: {0}", ex.Message);
            }
            catch (Exception fallbackEx)
            {
                _logger.ErrorException("Failed to apply graceful degradation", fallbackEx);
            }
        }

        /// <summary>
        /// Get minimal fallback CSS for graceful degradation
        /// </summary>
        private string GetFallbackCss()
        {
            return @"
                /* Emby Beautify Plugin - Fallback Styles */
                .emby-header {
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                }
                .card {
                    border-radius: 8px;
                    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
                    transition: transform 0.2s ease;
                }
                .card:hover {
                    transform: translateY(-2px);
                }
            ";
        }

        /// <summary>
        /// Get the current plugin configuration
        /// </summary>
        public async Task<BeautifyConfig> GetConfigurationAsync()
        {
            try
            {
                if (_configurationManager == null)
                {
                    _logger.Warn("Configuration manager not initialized, returning default configuration");
                    return new BeautifyConfig();
                }

                return await _configurationManager.LoadConfigurationAsync();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error getting configuration", ex);
                return new BeautifyConfig(); // Return default configuration on error
            }
        }

        /// <summary>
        /// Update the plugin configuration
        /// </summary>
        public async Task UpdateConfigurationAsync(BeautifyConfig config)
        {
            try
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                if (_configurationManager == null)
                {
                    _logger.Error("Configuration manager not initialized, cannot update configuration");
                    throw new InvalidOperationException("Plugin not properly initialized");
                }

                _logger.Debug("Updating plugin configuration");

                // Validate configuration
                var isValid = await _configurationManager.ValidateConfigurationAsync(config);
                if (!isValid)
                {
                    _logger.Error("Invalid configuration provided");
                    throw new ArgumentException("Invalid configuration", nameof(config));
                }

                // Save configuration
                await _configurationManager.SaveConfigurationAsync(config);
                _logger.Debug("Configuration saved successfully");

                // Apply theme changes if theme ID changed
                if (_themeManager != null && _styleInjector != null)
                {
                    var currentTheme = await _themeManager.GetActiveThemeAsync();
                    if (currentTheme?.Id != config.ActiveThemeId)
                    {
                        _logger.Debug("Theme changed from '{0}' to '{1}', applying new theme", 
                            currentTheme?.Id ?? "none", config.ActiveThemeId);

                        await _themeManager.SetActiveThemeAsync(config.ActiveThemeId);
                        var newTheme = await _themeManager.GetActiveThemeAsync();
                        
                        if (newTheme != null)
                        {
                            var css = await _themeManager.GenerateThemeCssAsync(newTheme);
                            await _styleInjector.InjectStylesAsync(css);
                            _logger.Info("Theme changed to '{0}' successfully", newTheme.Name);
                        }
                    }
                }

                _logger.Info("Configuration updated successfully");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error updating configuration", ex);
                throw;
            }
        }

        /// <summary>
        /// Dispose resources - called by Emby Server on shutdown
        /// </summary>
        public void Dispose()
        {
            try
            {
                _logger.Debug("Disposing Emby Beautify Plugin resources");

                // Dispose managed resources
                _themeManager = null;
                _styleInjector = null;
                _configurationManager = null;

                lock (_initializationLock)
                {
                    _isInitialized = false;
                }

                _logger.Info("Emby Beautify Plugin disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error disposing plugin resources", ex);
            }
        }
    }
}