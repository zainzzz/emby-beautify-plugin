using EmbyBeautifyPlugin.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Abstracts
{
    /// <summary>
    /// Abstract base class for style injectors
    /// </summary>
    public abstract class BaseStyleInjector : IStyleInjector
    {
        protected readonly ILogger<BaseStyleInjector> _logger;
        protected readonly Dictionary<string, string> _injectedStyles;

        protected BaseStyleInjector(ILogger<BaseStyleInjector> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _injectedStyles = new Dictionary<string, string>();
        }

        /// <summary>
        /// Inject CSS styles into the web client
        /// </summary>
        public abstract Task InjectStylesAsync(string css);

        /// <summary>
        /// Remove previously injected styles by ID
        /// </summary>
        public virtual async Task RemoveStylesAsync(string styleId)
        {
            try
            {
                if (string.IsNullOrEmpty(styleId))
                    throw new ArgumentException("Style ID cannot be null or empty", nameof(styleId));

                if (_injectedStyles.ContainsKey(styleId))
                {
                    _injectedStyles.Remove(styleId);
                    _logger.LogDebug("Removed styles with ID: {StyleId}", styleId);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing styles with ID: {StyleId}", styleId);
                throw;
            }
        }

        /// <summary>
        /// Update all global styles based on current configuration
        /// </summary>
        public abstract Task UpdateGlobalStylesAsync();

        /// <summary>
        /// Track injected styles
        /// </summary>
        protected virtual void TrackInjectedStyle(string styleId, string css)
        {
            if (!string.IsNullOrEmpty(styleId) && !string.IsNullOrEmpty(css))
            {
                _injectedStyles[styleId] = css;
            }
        }
    }
}