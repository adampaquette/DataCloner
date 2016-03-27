using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataCloner.Infrastructure.Modularity
{
    public class TestPlugin : IPlugin
    {
        private INavigationService _navigationService;

        public List<MenuEntry> MenuEntries { get; private set; }

        public TestPlugin(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            MenuEntries = new List<MenuEntry>
            {
                new MenuEntry("General", new DelegateCommand(() => _navigationService.Navigate("TestPlugin",null))),
                new MenuEntry("Tableau de board", new DelegateCommand(() => _navigationService.Navigate("Home",null))),
                new MenuEntry("Cloner", new DelegateCommand(() => Debug.WriteLine("plugin 2"))),
                new MenuEntry("Configuration", new DelegateCommand(() => Debug.WriteLine("plugin 3"))),
                new MenuEntry("Serveur SQL", new DelegateCommand(() => Debug.WriteLine("plugin 4"))),
                new MenuEntry("Environnements", new DelegateCommand(() => Debug.WriteLine("plugin 5"))),
                new MenuEntry("Modèles", new DelegateCommand(() => Debug.WriteLine("plugin 6"))),
                new MenuEntry("Comportements", new DelegateCommand(() => Debug.WriteLine("plugin 7")))
            };
        }
    }
}
