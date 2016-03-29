using DataCloner.Infrastructure.Modularity;
using System.Collections.Generic;

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

            MenuItems.AddRange(test.MenuItems);
        }
    }
}
