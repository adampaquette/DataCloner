using DataCloner.Universal.Menu;
using DataCloner.Universal.Menu.Left;
using DataCloner.Universal.Menu.Top;
using System.Collections.Generic;

namespace DataCloner.Universal.ViewModels.Design
{
    /// <summary>
    /// The design-time ViewModel for the AppShell.
    /// </summary>
    public class AppShellDesignViewModel
    {
        public AppShellDesignViewModel()
        {
            NavigationBarMenuItemsTopLeft = new List<IMenuItem>()
            {
                new FileMenuItem(null),
                new ToolsMenuItem(null),
                new HelpMenuItem(null)
            };

            NavigationBarMenuItemsTopMiddle = new List<IMenuItem>()
            {
                new ClonerMenuItem(null)
            };

            NavigationBarMenuItemsLeft = new List<IMenuItem>()
            {
                new SqlServersMenuItem(null),
                new ClonerMenuItem(null),
                new ExtractionModelsMenuItem(null)
            };
        }


        public List<IMenuItem> NavigationBarMenuItemsTopLeft { get; private set; }
        public List<IMenuItem> NavigationBarMenuItemsTopMiddle { get; private set; }
        public List<IMenuItem> NavigationBarMenuItemsTopRight { get; private set; }
        public List<IMenuItem> NavigationBarMenuItemsLeft { get; private set; }
    }
}
