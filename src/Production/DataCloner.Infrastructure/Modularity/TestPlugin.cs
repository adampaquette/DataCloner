using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Infrastructure.Modularity
{
    public class TestPlugin : IPlugin
    {
        private INavigationService _navigationService;

        public List<MenuItem> MenuItems { get; private set; }

        public TestPlugin(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            var contextMenu = new MenuFlyout();
            contextMenu.Items.Add(new MenuFlyoutItem { Text = "Add", Command = new DelegateCommand(() => _navigationService.Navigate("Home", null)) });
            contextMenu.Items.Add(new MenuFlyoutItem { Text = "Delete", Command = new DelegateCommand(() => _navigationService.Navigate("TestPlugin", null)) });

            var contextMenu2 = new MenuFlyout();
            contextMenu2.Items.Add(new MenuFlyoutItem { Text = "Add" });
            contextMenu2.Items.Add(new MenuFlyoutItem { Text = "Delete" });
            contextMenu2.Items.Add(new MenuFlyoutSeparator());
            contextMenu2.Items.Add(new MenuFlyoutItem { Text = "Create a new Serveur" });
            contextMenu2.Items.Add(new MenuFlyoutItem { Text = "Create a new Template" });

            MenuItems = new List<MenuItem>
            {
                new MenuItem("General", new DelegateCommand(() => _navigationService.Navigate("TestPlugin",null)), contextMenu),
                new MenuItem("Tableau de board", new DelegateCommand(() => _navigationService.Navigate("Home",null)),contextMenu),
                new MenuItem("Cloner", new DelegateCommand(() => Debug.WriteLine("plugin 2")),contextMenu),
                new MenuItem("Configuration", new DelegateCommand(() => Debug.WriteLine("plugin 3")),contextMenu),
                new MenuItem("Serveur SQL", new DelegateCommand(() => Debug.WriteLine("plugin 4")),contextMenu2),
                new MenuItem("Environnements", new DelegateCommand(() => Debug.WriteLine("plugin 5")),contextMenu2),
                new MenuItem("Modèles", new DelegateCommand(() => Debug.WriteLine("plugin 6")),contextMenu2),
                new MenuItem("Comportements", new DelegateCommand(() => Debug.WriteLine("plugin 7")),contextMenu2)
            };
        }
    }
}
