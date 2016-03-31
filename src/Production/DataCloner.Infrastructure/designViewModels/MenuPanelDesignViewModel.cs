using DataCloner.Infrastructure.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Infrastructure.designViewModels
{
    public class MenuPanelDesignViewModel
    {
        public List<MenuItem> MenuItems { get; }

        public MenuPanelDesignViewModel()
        {
            MenuItems = new List<MenuItem>();

            var test = new TestPlugin(null);
            test.Initialize();
            
            foreach (var item in test.MenuItems)
            {
                //Root element
                if (String.IsNullOrWhiteSpace(item.Id))
                {
                    MenuItems.Add(item);
                }
                else
                {
                    //Search for the parent
                    MenuItems.First(i => i.Id == item.ContainerPath).Children.Add(item);
                } 
            }
        }
    }
}
