using System.Collections.Generic;

namespace DataCloner.Infrastructure.Modularity
{
    /// <summary>
    /// Defines the contract for the plugin deployed in the application.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Initialize the plugin
        /// </summary>
        void Initialize();

        List<MenuItem> MenuItems { get; }
    }
}
