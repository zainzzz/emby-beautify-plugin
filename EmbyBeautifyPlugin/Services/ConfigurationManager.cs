using EmbyBeautifyPlugin.Abstracts;
using EmbyBeautifyPlugin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// Concrete implementation of configuration manager using JSON file storage
    /// </summary>
    public class ConfigurationManager : BaseConfigurationManager
    {
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public ConfigurationManager(ILogger<BaseConfigurationManager> logger, string configDirectory = null) 
            : base(logger)
        {
            var configDir = configDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmbyBeautifyPlugin");
            _configFilePath = Path.Combine(configDir, "config.json");
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            EnsureConfigDirectoryExists();
        }

        /// <summary>
        /// Load configuration from JSON file
        /// </summary>
        public override async Task<BeautifyConfig> LoadConfigurationAsync()
        {
            try
            {
                _logger.LogDebug("Loading configuration from {ConfigPath}", _configFilePath);

                if (!File.Exists(_configFilePath))
                {
                    _logger.LogInformation("Configuration file not found, creating default configuration");
                    var defaultConfig = new BeautifyConfig();
                    await SaveConfigurationAsync(defaultConfig);
                    UpdateCurrentConfig(defaultConfig);
                    return defaultConfig;
                }

                var jsonContent = await File.ReadAllTextAsync(_configFilePath);
                var config = JsonSerializer.Deserialize<BeautifyConfig>(jsonContent, _jsonOptions);

                if (config == null)
                {
                    _logger.LogWarning("Failed to deserialize configuration, using default");
                    config = new BeautifyConfig();
                }

                var isValid = await ValidateConfigurationAsync(config);
                if (!isValid)
                {
                    _logger.LogWarning("Configuration validation failed, using default configuration");
                    config = new BeautifyConfig();
                    await SaveConfigurationAsync(config);
                }

                UpdateCurrentConfig(config);
                _logger.LogDebug("Configuration loaded successfully");
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration from {ConfigPath}", _configFilePath);
                var defaultConfig = new BeautifyConfig();
                UpdateCurrentConfig(defaultConfig);
                return defaultConfig;
            }
        }

        /// <summary>
        /// Save configuration to JSON file
        /// </summary>
        public override async Task SaveConfigurationAsync(BeautifyConfig config)
        {
            try
            {
                if (config == null)
                {
                    throw new ArgumentNullException(nameof(config));
                }

                _logger.LogDebug("Saving configuration to {ConfigPath}", _configFilePath);

                var isValid = await ValidateConfigurationAsync(config);
                if (!isValid)
                {
                    throw new InvalidOperationException("Configuration validation failed");
                }

                var jsonContent = JsonSerializer.Serialize(config, _jsonOptions);
                await File.WriteAllTextAsync(_configFilePath, jsonContent);

                UpdateCurrentConfig(config);
                _logger.LogDebug("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration to {ConfigPath}", _configFilePath);
                throw;
            }
        }

        /// <summary>
        /// Enhanced validation with additional checks
        /// </summary>
        public override async Task<bool> ValidateConfigurationAsync(BeautifyConfig config)
        {
            try
            {
                // Call base validation first
                var baseValidation = await base.ValidateConfigurationAsync(config);
                if (!baseValidation)
                {
                    return false;
                }

                // Additional validation checks
                if (config.ResponsiveSettings == null)
                {
                    _logger.LogWarning("ResponsiveSettings is null");
                    return false;
                }

                if (config.CustomSettings == null)
                {
                    _logger.LogWarning("CustomSettings is null");
                    return false;
                }

                // Validate responsive settings
                if (!ValidateBreakpointSettings(config.ResponsiveSettings.Desktop, "Desktop") ||
                    !ValidateBreakpointSettings(config.ResponsiveSettings.Tablet, "Tablet") ||
                    !ValidateBreakpointSettings(config.ResponsiveSettings.Mobile, "Mobile"))
                {
                    return false;
                }

                // Validate animation duration range
                if (config.AnimationDuration > 5000)
                {
                    _logger.LogWarning("Animation duration is too high: {Duration}ms", config.AnimationDuration);
                    return false;
                }

                _logger.LogDebug("Enhanced configuration validation passed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during enhanced configuration validation");
                return false;
            }
        }

        /// <summary>
        /// Validate breakpoint settings
        /// </summary>
        private bool ValidateBreakpointSettings(BreakpointSettings settings, string breakpointName)
        {
            if (settings == null)
            {
                _logger.LogWarning("{BreakpointName} breakpoint settings is null", breakpointName);
                return false;
            }

            if (settings.GridColumns <= 0)
            {
                _logger.LogWarning("{BreakpointName} grid columns must be positive", breakpointName);
                return false;
            }

            if (settings.FontScale <= 0)
            {
                _logger.LogWarning("{BreakpointName} font scale must be positive", breakpointName);
                return false;
            }

            if (string.IsNullOrEmpty(settings.GridGap))
            {
                _logger.LogWarning("{BreakpointName} grid gap cannot be empty", breakpointName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure the configuration directory exists
        /// </summary>
        private void EnsureConfigDirectoryExists()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug("Created configuration directory: {Directory}", directory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating configuration directory");
                throw;
            }
        }

        /// <summary>
        /// Get the configuration file path (for testing purposes)
        /// </summary>
        public string GetConfigFilePath() => _configFilePath;
    }
}