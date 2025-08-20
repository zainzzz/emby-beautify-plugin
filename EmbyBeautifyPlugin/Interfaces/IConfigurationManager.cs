using EmbyBeautifyPlugin.Models;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Interfaces
{
    /// <summary>
    /// Interface for managing plugin configuration
    /// </summary>
    public interface IConfigurationManager
    {
        /// <summary>
        /// Load the current configuration from storage
        /// </summary>
        /// <returns>Current configuration</returns>
        Task<BeautifyConfig> LoadConfigurationAsync();

        /// <summary>
        /// Save configuration to storage
        /// </summary>
        /// <param name="config">Configuration to save</param>
        /// <returns>Task representing the save operation</returns>
        Task SaveConfigurationAsync(BeautifyConfig config);

        /// <summary>
        /// Validate a configuration object
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        /// <returns>True if configuration is valid, false otherwise</returns>
        Task<bool> ValidateConfigurationAsync(BeautifyConfig config);
    }
}