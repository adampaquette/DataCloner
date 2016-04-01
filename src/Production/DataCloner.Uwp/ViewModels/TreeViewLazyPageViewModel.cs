using DataCloner.Infrastructure.Modularity;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataCloner.Uwp.ViewModels
{
    public class TreeViewLazyPageViewModel : ViewModelBase
    {
        private INavigationService _navigationService;
        private ObservableCollection<MenuItem> _menuItems;

        public ObservableCollection<MenuItem> MenuItems
        {
            get { return _menuItems; }
            set { SetProperty(ref _menuItems, value); }
        }

        public TreeViewLazyPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            var test = new TestPlugin(_navigationService);
            test.Initialize();

            _menuItems = BuildTreeView(test.MenuItems);
        }

        private ObservableCollection<MenuItem> BuildTreeView(List<MenuItem> items)
        {
            var menu = new ObservableCollection<MenuItem>();

            foreach (var item in items)
            {
                //Root element
                if (String.IsNullOrWhiteSpace(item.ContainerPath))
                {
                    menu.Add(item);
                }
                else
                {
                    //Search for the parent
                    menu.First(i => i.Id == item.ContainerPath).Children.Add(item);
                }
            }
            return menu;
        }
    }
}
