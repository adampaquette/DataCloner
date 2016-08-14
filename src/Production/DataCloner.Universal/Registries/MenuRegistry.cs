using DataCloner.Universal.Menu;
using DataCloner.Universal.Menu.Top;
using Microsoft.Practices.Unity;
using DataCloner.Universal.Unity;
using DataCloner.Universal.Menu.Left;
using DataCloner.Universal.ViewModels;

namespace DataCloner.Universal.Registries
{
    /// <summary>
    /// Registry for menus.
    /// </summary>
    public class MenuRegistry : RegistryBase, IRegistry
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="container">The Unity container.</param>
        public MenuRegistry(IUnityContainer container) : base(container)
        {
        }

        /// <summary>
        /// Configures dependencies.
        /// </summary>
        public void Configure()
        {
            // Top menu
            Container.RegisterTypeWithName<IMenuItem, FileMenuItem>();
            Container.RegisterTypeWithName<IMenuItem, ToolsMenuItem>();

            // Left menu
            Container.RegisterTypeWithName<ITreeViewMenuItem, GeneralMenuItem>();
        }
    }
}
