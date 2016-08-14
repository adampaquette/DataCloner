using DataCloner.Universal.Menu;
using DataCloner.Universal.Menu.Top;
using Microsoft.Practices.Unity;
using DataCloner.Universal.Unity;

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
            Container.RegisterTypeWithName<IMenuItem, FileMenuItem>();
            Container.RegisterTypeWithName<IMenuItem, ToolsMenuItem>();
        }
    }
}
