using System.Collections.Generic;

namespace DataCloner.Infrastructure.Modularity
{
    /// <summary>
    /// Defines the interface for the service that will retrieve the plugins.
    /// </summary>
    interface IPluginManager<T>
    {
        /// <summary>
        /// Loads and initializes the plugin within the specified folder.
        /// </summary>
        /// <param name="pluginFolderPath">Name of the plugin's folder path.</param>
        List<T> LoadPlugins(string pluginFolderPath);
    }
}
