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
                new ContextMenuItem(CONTEXT_MENU_PATH_1) { Text = "Delete"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Delete"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create as new server"},
                new ContextMenuItem(CONTEXT_MENU_PATH_2) { Text = "Create as new Template"}
             });

            MenuItems = new List<MenuItem>
            {
                new MenuItem(MENU_PATH_1, null, 
                    new DelegateCommand(() => _navigationService.Navigate("TestPlugin",null)), 
                    CONTEXT_MENU_PATH_1, null, contextMenuManager) { Text = "General" },
                new MenuItem(MENU_PATH_2, MENU_PATH_1,
                    new DelegateCommand(() => _navigationService.Navigate("Home",null)),
                    CONTEXT_MENU_PATH_1, null, contextMenuManager) {Text = "Tableau de board"},
                new MenuItem(MENU_PATH_3, MENU_PATH_1,
                    new DelegateCommand(() => Debug.WriteLine("plugin 2")),
                    CONTEXT_MENU_PATH_1, null, contextMenuManager) {Text = "Cloner"},

                new MenuItem(MENU_PATH_4, null,
                    new DelegateCommand(() => Debug.WriteLine("plugin 3")),
                    CONTEXT_MENU_PATH_1, null, contextMenuManager) {Text = "Configuration"},
                new MenuItem(MENU_PATH_5, MENU_PATH_4,
                    new DelegateCommand(() => Debug.WriteLine("plugin 4")),
                    CONTEXT_MENU_PATH_2, null, contextMenuManager) {Text = "Serveur SQL"},
                new MenuItem(MENU_PATH_6, MENU_PATH_4,
                    new DelegateCommand(() => Debug.WriteLine("plugin 5")),
                    CONTEXT_MENU_PATH_2, null, contextMenuManager) {Text = "Environnements"},
                new MenuItem(MENU_PATH_7, MENU_PATH_4,
                    new DelegateCommand(() => Debug.WriteLine("plugin 6")),
                    CONTEXT_MENU_PATH_2, null, contextMenuManager) {Text = "Modèles"},
                new MenuItem(MENU_PATH_8, MENU_PATH_4,
                    new DelegateCommand(() => Debug.WriteLine("plugin 7")),
                    CONTEXT_MENU_PATH_2, null, contextMenuManager) {Text = "Comportements"}
            };
        }
    }
}
