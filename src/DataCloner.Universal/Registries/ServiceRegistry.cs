using DataCloner.Universal.Facedes;
using Microsoft.Practices.Unity;

namespace DataCloner.Universal.Registries
{
    /// <summary>
    /// Registry for services.
    /// </summary>
    public class ServiceRegistry : RegistryBase, IRegistry
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="container">The Unity container.</param>
        public ServiceRegistry(IUnityContainer container) : base(container)
        {
        }

        /// <summary>
        /// Configures dependencies.
        /// </summary>
        public void Configure()
        {
            Container.RegisterType<INavigationFacade, NavigationFacade>();
        }
    }
}
