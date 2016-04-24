using DataCloner.Infrastructure.Modularity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataCloner.Infrastructure.designViewModels
{
    public class MenuPanelDesignViewModel
    {
        public ObservableCollection<MenuItem> MenuItems { get; }

        public MenuPanelDesignViewModel()
        {
            var test = new TestPlugin(null);
            test.Initialize();

            MenuItems = BuildTreeView(test.MenuItems);
        }

        private ObservableCollection<MenuItem> BuildTreeView(List<MenuItem> items)
        {
            var menu = new ObservableCollection<MenuItem>();

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
