using DataCloner.Infrastructure.Modularity;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System.Collections.Generic;

namespace DataCloner.Uwp.ViewModels
{
    public class MenuPanelViewModel : ViewModelBase
    {
        private INavigationService _navigationService;
        private List<MenuItem> _menuItems;

        public List<MenuItem> MenuItems
        {
            get { return _menuItems; }
            set { SetProperty(ref _menuItems, value); }
        }

        public MenuPanelViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _menuItems = new List<MenuItem>();

            //Receive all loaded plugins


            var test = new TestPlugin(_navigationService);
            test.Initialize();
            _menuItems.AddRange(test.MenuItems);
        }
    }
}
