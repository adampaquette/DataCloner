using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.Diagnostics;

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
            const string CONTEXT_MENU_PATH_1 = "MENU_PATH_1";
            const string CONTEXT_MENU_PATH_2 = "MENU_PATH_2";

            const string MENU_PATH_1 = "MENU_PATH_1";
            const string MENU_PATH_2 = "MENU_PATH_2";
            const string MENU_PATH_3 = "MENU_PATH_3";
            const string MENU_PATH_4 = "MENU_PATH_4";
            const string MENU_PATH_5 = "MENU_PATH_4";
            const string MENU_PATH_6 = "MENU_PATH_4";
            const string MENU_PATH_7 = "MENU_PATH_4";
            const string MENU_PATH_8 = "MENU_PATH_4";

            var contextMenuManager = new ContextMenuManager();
            contextMenuManager.Append(new List<ContextMenuItem>
            {
                new ContextMenuItem(CONTEXT_MENU_PATH_1) { Text = "Add", Command = new DelegateCommand(() => _navigationService.Navigate("Home", null)) },
                new ContextMenuItem(CONTEXT_MENU_PATH_1) { Text = "Delete", Command = new DelegateCommand(() => _navigationService.Navigate("TestPlugin", null)) },
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Delete"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create as new server"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create as new Template"}
             });

            MenuItems = new List<MenuItem>
            {
                new MenuItem(MENU_PATH_1, null,"General", new DelegateCommand(() => _navigationService.Navigate("TestPlugin",null)), CONTEXT_MENU_PATH_1, null, contextMenuManager),
                new MenuItem(MENU_PATH_2, null,"Tableau de board", new DelegateCommand(() => _navigationService.Navigate("Home",null)),CONTEXT_MENU_PATH_1, null, contextMenuManager),
                new MenuItem(MENU_PATH_3, null,"Cloner", new DelegateCommand(() => Debug.WriteLine("plugin 2")),CONTEXT_MENU_PATH_1, null, contextMenuManager),
                new MenuItem(MENU_PATH_4, null,"Configuration", new DelegateCommand(() => Debug.WriteLine("plugin 3")),CONTEXT_MENU_PATH_1, null, contextMenuManager),
                new MenuItem(MENU_PATH_5, null,"Serveur SQL", new DelegateCommand(() => Debug.WriteLine("plugin 4")),CONTEXT_MENU_PATH_2, null, contextMenuManager),
                new MenuItem(MENU_PATH_6, null,"Environnements", new DelegateCommand(() => Debug.WriteLine("plugin 5")),CONTEXT_MENU_PATH_2, null, contextMenuManager),
                new MenuItem(MENU_PATH_7, null,"Modèles", new DelegateCommand(() => Debug.WriteLine("plugin 6")),CONTEXT_MENU_PATH_2, null, contextMenuManager),
                new MenuItem(MENU_PATH_8, null,"Comportements", new DelegateCommand(() => Debug.WriteLine("plugin 7")),CONTEXT_MENU_PATH_2, null, contextMenuManager)
            };
        }
    }
}
