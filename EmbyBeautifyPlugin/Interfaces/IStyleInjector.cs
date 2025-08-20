using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Interfaces
{
    /// <summary>
    /// Interface for injecting styles into the Emby web client
    /// </summary>
    public interface IStyleInjector
    {
        /// <summary>
        /// Inject CSS styles into the web client
        /// </summary>
        /// <param name="css">CSS content to inject</param>
        /// <returns>Task representing the injection operation</returns>
        Task InjectStylesAsync(string css);

        /// <summary>
        /// Remove previously injected styles by ID
        /// </summary>
        /// <param name="styleId">ID of the styles to remove</param>
        /// <returns>Task representing the removal operation</returns>
        Task RemoveStylesAsync(string styleId);

        /// <summary>
        /// Update all global styles based on current configuration
        /// </summary>
        /// <returns>Task representing the update operation</returns>
        Task UpdateGlobalStylesAsync();
    }
}