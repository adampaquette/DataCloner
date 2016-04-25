using DataCloner.Infrastructure.UserControls;
using Prism.Commands;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Infrastructure.Modularity
{
    public class NavigationMenuItem : MenuItem
    {
        public MenuFlyout ContextMenu => _contextMenuManager?.GetContextMenu(_contextMenuPath);

        public NavigationMenuItem(string pathId, string containerPath, string contextMenuPath, TreeViewLazyItemViewModel parent, 
            ContextMenuManager contextMenuManager, DelegateCommand command)
            : base(pathId, containerPath, contextMenuPath, parent, contextMenuManager, command)
        {
        }
    }
}
