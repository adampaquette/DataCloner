using DataCloner.Infrastructure.Modularity;
using DataCloner.Uwp.Plugins;
using DataCloner.Uwp.Services;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataCloner.Uwp.ViewModels
{
    public class MenuPanelViewModel : ViewModelBase
    {
        private INavigationService _navigationService;
        private ObservableCollection<MenuItem> _menuItems;

        public ObservableCollection<MenuItem> MenuItems
        {
            get { return _menuItems; }
            set { SetProperty(ref _menuItems, value); }
        }

        public MenuPanelViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            var menuBuilder = new TreeMenuBuilder();

            var generalMenu = new GeneralMenuPlugin(_navigationService);
            generalMenu.Initialize();
            menuBuilder.Append(generalMenu.MenuItems);

            var test = new TestPlugin(_navigationService);
            test.Initialize();
            menuBuilder.Append(test.MenuItems);

            _menuItems = menuBuilder.ToObservableCollection();
        }
    }
}
