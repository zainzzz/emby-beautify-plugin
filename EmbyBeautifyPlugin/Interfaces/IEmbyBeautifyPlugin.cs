using MediaBrowser.Controller.Plugins;
using EmbyBeautifyPlugin.Models;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Interfaces
{
    /// <summary>
    /// Main plugin interface for Emby Beautify Plugin
    /// </summary>
    public interface IEmbyBeautifyPlugin : IServerEntryPoint
    {
        /// <summary>
        /// Initialize the plugin asynchronously
        /// </summary>
        /// <returns>Task representing the initialization operation</returns>
        Task InitializeAsync();

        /// <summary>
        /// Get the current plugin configuration
        /// </summary>
        /// <returns>Current plugin configuration</returns>
        Task<BeautifyConfig> GetConfigurationAsync();

        /// <summary>
        /// Update the plugin configuration
        /// </summary>
        /// <param name="config">New configuration to apply</param>
        /// <returns>Task representing the update operation</returns>
        Task UpdateConfigurationAsync(BeautifyConfig config);
    }
}