using DataCloner.Universal.Registries;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System.Collections.Generic;

namespace DataCloner.Universal.Unity
{
    public class UnityBootstrapper
    {
        public static UnityContainer Container { get; set; }

        public static List<IRegistry> Registries { get; private set; }

        private static void AddRegistries()
        {
            Registries.Add(new ServiceRegistry(Container));
            Registries.Add(new MenuRegistry(Container));
            Registries.Add(new ViewRegistry());            
        }

        /// <summary>
        /// Configures all registered dependencies.
        /// </summary>
        /// <remarks>
        /// The <see cref="ServiceLocator" /> needs to be initialized
        /// when calling this method.
        /// </remarks>
        public static void ConfigureRegistries()
        {
            AddRegistries();
            Registries.ForEach(r => r.Configure());
        }

        public static void Init()
        {
            Container = new UnityContainer();
            Registries = new List<IRegistry>();

            var serviceLocator = new UnityServiceLocator(Container);
            ServiceLocator.SetLocatorProvider(()=>serviceLocator);
        }
    }
}
