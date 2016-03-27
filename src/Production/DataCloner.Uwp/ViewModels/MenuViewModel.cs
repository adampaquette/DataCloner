using DataCloner.Infrastructure.Modularity;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System.Collections.Generic;

namespace DataCloner.Uwp.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private INavigationService _navigationService;
        private List<MenuEntry> _menuEntries;

        public List<MenuEntry> MenuEntries
        {
            get { return _menuEntries; }
            set { SetProperty(ref _menuEntries, value); }
        }

        public MenuViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _menuEntries = new List<MenuEntry>();

            var test = new TestPlugin(_navigationService);
            test.Initialize();
            _menuEntries.AddRange(test.MenuEntries);
        }
    }
}
