using DataCloner.Infrastructure.Modularity;
using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloner.Uwp.Plugins
{
    public class GeneralMenuPlugin : IPlugin
    {
        private INavigationService _navigationService;

        public List<MenuItem> MenuItems { get; private set; }

        public string GeneralSectionMenuPath => "GENERAL_SECTION";
        public string DashboardMenuPath => GeneralSectionMenuPath + "_DASHBOARD";
        public string ClonerMenuPath => GeneralSectionMenuPath + "_CLONER";

        public GeneralMenuPlugin(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            MenuItems = new List<MenuItem>
            {
                new MenuItem(GeneralSectionMenuPath, null, null, null, null, null) { Text = "Général" },
                new MenuItem(DashboardMenuPath, GeneralSectionMenuPath, null, null, null, new DelegateCommand(() =>  _navigationService.Navigate("Dashboard", null))) { Text = "Tableau de board" },
                new MenuItem(ClonerMenuPath, GeneralSectionMenuPath, null, null, null, new DelegateCommand(() => Debug.WriteLine("CLONER"))) { Text = "Cloner" }
            };

            //new MenuItem(DashboardMenuPath, GeneralSectionMenuPath, null, null, null, new DelegateCommand(() => _navigationService.Navigate("Dashboard", null))) { Text = "Tableau de board" },
            //    new MenuItem(ClonerMenuPath, GeneralSectionMenuPath, null, null, null, new DelegateCommand(() => _navigationService.Navigate("Cloner", null))) { Text = "Cloner" }
        }
    }
}
