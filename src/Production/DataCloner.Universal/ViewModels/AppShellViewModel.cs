using DataCloner.Universal.Menu;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Universal.ViewModels
{
    public class AppShellViewModel : ViewModelBase
    {
        public AppShellViewModel()
        {
            NavigationBarMenuItemsTop = ServiceLocator.Current
                .GetAllInstances<IMenuItem>()
                .Where(i => i.Location == MenuItemLocation.Top)
                .ToList();

            NavigationBarMenuItemsLeft = ServiceLocator.Current
                .GetAllInstances<ITreeViewLazyItem<IMenuItem>>()
                .Where(i => i.Content.Location == MenuItemLocation.Left)
                .ToList();
        }

        /// <summary>
        /// The navigation bar items at the top.
        /// </summary>
        public List<IMenuItem> NavigationBarMenuItemsTop { get; private set; }

        /// <summary>
        /// The navigation bar items at the left.
        /// </summary>
        public List<ITreeViewLazyItem<IMenuItem>> NavigationBarMenuItemsLeft { get; private set; }
    }
}
