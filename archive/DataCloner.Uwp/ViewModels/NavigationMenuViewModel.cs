using DataCloner.Infrastructure.Modularity;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System.Collections.ObjectModel;

namespace DataCloner.Uwp.ViewModels
{
    public class NavigationMenuViewModel : ViewModelBase
    {
        private INavigationService _navigationService;
        private ObservableCollection<NavigationMenuItem> _menuItems;

        public ObservableCollection<NavigationMenuItem> MenuItems
        {
            get { return _menuItems; }
            set { SetProperty(ref _menuItems, value); }
        }

        public NavigationMenuViewModel(INavigationService navigationService, ObservableCollection<NavigationMenuItem> menuItems)
        {
            _navigationService = navigationService;
            _menuItems = menuItems;
        }         
    }
}
