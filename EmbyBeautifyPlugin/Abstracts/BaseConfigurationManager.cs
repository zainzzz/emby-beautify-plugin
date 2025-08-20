using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Abstracts
{
    /// <summary>
    /// Abstract base class for configuration managers
    /// </summary>
    public abstract class BaseConfigurationManager : IConfigurationManager
    {
        protected readonly ILogger<BaseConfigurationManager> _logger;
        protected BeautifyConfig _currentConfig;

        protected BaseConfigurationManager(ILogger<BaseConfigurationManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentConfig = new BeautifyConfig();
        }

        /// <summary>
        /// Load the current configuration from storage
        /// </summary>
        public abstract Task<BeautifyConfig> LoadConfigurationAsync();

        /// <summary>
        /// Save configuration to storage
        /// </summary>
        public abstract Task SaveConfigurationAsync(BeautifyConfig config);

        /// <summary>
        /// Validate a configuration object
        /// </summary>
        public virtual async Task<bool> ValidateConfigurationAsync(BeautifyConfig config)
        {
            try
            {
                if (config == null)
                {
                    _logger.LogWarning("Configuration is null");
                    return false;
                }

                if (string.IsNullOrEmpty(config.ActiveThemeId))
                {
                    _logger.LogWarning("Active theme ID is null or empty");
                    return false;
                }

                if (config.AnimationDuration < 0)
                {
                    _logger.LogWarning("Animation duration cannot be negative");
                    return false;
                }

                _logger.LogDebug("Configuration validation passed");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration");
                return false;
            }
        }

        /// <summary>
        /// Get the current cached configuration
        /// </summary>
        protected virtual BeautifyConfig GetCurrentConfig()
        {
            return _currentConfig;
        }

        /// <summary>
        /// Update the current cached configuration
        /// </summary>
        protected virtual void UpdateCurrentConfig(BeautifyConfig config)
        {
            _currentConfig = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
}