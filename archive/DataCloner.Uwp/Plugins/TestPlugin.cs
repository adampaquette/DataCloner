using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataCloner.Infrastructure.Modularity
{
    public class TestPlugin : IPlugin
    {
        private INavigationService _navigationService;

        public List<NavigationMenuItem> NavigationMenuItems { get; private set; }
        public List<NavigationMenuItem> TopBarMenuItems { get; private set; }

        public TestPlugin(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            const string CONTEXT_MENU_PATH_1 = "MENU_PATH_1";
            const string CONTEXT_MENU_PATH_2 = "MENU_PATH_2";

            const string MENU_PATH_1 = "MENU_PATH_1";
            const string MENU_PATH_2 = "MENU_PATH_2";
            const string MENU_PATH_3 = "MENU_PATH_3";

            var contextMenuManager = new ContextMenuManager();
            contextMenuManager.Append(new List<ContextMenuItem>
            {
                new ContextMenuItem(CONTEXT_MENU_PATH_1) { Text = "Add", Command = new DelegateCommand(() => _navigationService.Navigate("Dashboard", null)) },
                new ContextMenuItem(CONTEXT_MENU_PATH_1) { Text = "Delete"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Delete"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create as new server"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create as new Template"}
             });

            NavigationMenuItems = new List<NavigationMenuItem>
            {
                new NavigationMenuItem(MENU_PATH_1, null, CONTEXT_MENU_PATH_1, new DelegateCommand(() => _navigationService.Navigate("TestPlugin",null))) { Text = "Configuration" },
                new NavigationMenuItem(MENU_PATH_2, MENU_PATH_1, CONTEXT_MENU_PATH_1, new DelegateCommand(() => _navigationService.Navigate("Dashboard",null))) {Text = "Serveur SQL"},
                new NavigationMenuItem(MENU_PATH_3, MENU_PATH_1, CONTEXT_MENU_PATH_1, new DelegateCommand(() => Debug.WriteLine("plugin 2"))) {Text = "Environnements"}
            };
        }
    }
}
