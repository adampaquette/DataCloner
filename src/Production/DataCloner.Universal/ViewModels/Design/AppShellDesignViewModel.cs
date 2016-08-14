using DataCloner.Universal.Menu;
using DataCloner.Universal.Menu.Top;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloner.Universal.ViewModels.Design
{
    /// <summary>
    /// The design-time ViewModel for the AppShell.
    /// </summary>
    public class AppShellDesignViewModel
    {
        public AppShellDesignViewModel()
        {
            NavigationBarMenuItemsTop = new List<IMenuItem>()
            {
                new FileMenuItem(null),
                new ToolsMenuItem(null)
            };

            NavigationBarMenuItemsLeft = new List<IMenuItem>();
        }

        public List<IMenuItem> NavigationBarMenuItemsTop { get; private set; }
        public List<IMenuItem> NavigationBarMenuItemsLeft { get; private set; }
    }
}
