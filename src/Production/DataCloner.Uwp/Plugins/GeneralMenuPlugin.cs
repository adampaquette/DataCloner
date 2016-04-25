using DataCloner.Infrastructure.Modularity;
using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.Generic;

namespace DataCloner.Uwp.Plugins
{
    public class GeneralMenuPlugin : IPlugin
    {
        private INavigationService _navigationService;

        public List<NavigationMenuItem> NavigationMenuItems { get; private set; }
        public List<NavigationMenuItem> FileMenuItems { get; private set; }

        #region Menu path declaration

        public string GeneralSectionMenuPath => "GENERAL_SECTION";
        public string DashboardMenuPath => GeneralSectionMenuPath + "_DASHBOARD";
        public string ClonerMenuPath => GeneralSectionMenuPath + "_CLONER";

        #endregion

        public GeneralMenuPlugin(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            NavigationMenuItems = new List<NavigationMenuItem>
            {
                new NavigationMenuItem(GeneralSectionMenuPath, null, null, null, null, null) { Text = "Général" },
                new NavigationMenuItem(DashboardMenuPath, GeneralSectionMenuPath, null, null, null, new DelegateCommand(() =>  _navigationService.Navigate("Dashboard", null))) { Text = "Tableau de board" },
                new NavigationMenuItem(ClonerMenuPath, GeneralSectionMenuPath, null, null, null, new DelegateCommand(() => _navigationService.Navigate("Cloner", null))) { Text = "Cloner" }
            };           
        }
    }
}
