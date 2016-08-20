using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Infrastructure.Modularity
{
    public class ContextMenuManager
    {
        private static Dictionary<string, MenuFlyout> _menus;

        static ContextMenuManager()
        {
            _menus = new Dictionary<string, MenuFlyout>();
        }

        public void Append(IList<ContextMenuItem> items)
        {
            foreach (var item in items)
                Append(item);
        }

        public void Append(ContextMenuItem item)
        {
            if (!_menus.ContainsKey(item.ContainerPath))
                _menus.Add(item.ContainerPath, new MenuFlyout());

            _menus[item.ContainerPath].Items.Add(item);
        }

        public MenuFlyout GetContextMenu(string containerPath)
        {
            return _menus[containerPath];
        }
    }
}
