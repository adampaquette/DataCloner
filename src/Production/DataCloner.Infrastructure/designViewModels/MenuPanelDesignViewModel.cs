using DataCloner.Infrastructure.Modularity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataCloner.Infrastructure.designViewModels
{
    public class MenuPanelDesignViewModel
    {
        public ObservableCollection<NavigationMenuItem> MenuItems { get; }

        public MenuPanelDesignViewModel()
        {
            var test = new TestPlugin(null);
            test.Initialize();

            MenuItems = BuildTreeView(test.NavigationMenuItems);
        }

        private ObservableCollection<NavigationMenuItem> BuildTreeView(List<NavigationMenuItem> items)
        {
            var menu = new ObservableCollection<NavigationMenuItem>();

            foreach (var item in items)
            {
                //Root element
                if (String.IsNullOrWhiteSpace(item.PathId))
                {
                    menu.Add(item);
                }
                else
                {
                    //Search for the parent
                    menu.First(i => i.PathId == item.ContainerPath).Children.Add(item);
                } 
            }
            return menu;
        }
    }
}
