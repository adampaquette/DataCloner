using DataCloner.Universal.Menu;
using DataCloner.Universal.Menu.Top;
using Microsoft.Practices.Unity;

namespace DataCloner.Universal.Registries
{
    public class MenuRegistry : RegistryBase, IRegistry
    {
        public MenuRegistry(IUnityContainer container) : base(container)
        {
        }

        public void Configure()
        {
            Container.RegisterType<IMenuItem, FileMenuItem>();
        }
    }
}
