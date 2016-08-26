using Prism.Commands;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Infrastructure.Modularity
{
    public class NavigationMenuItem : MenuItem
    {
        public MenuFlyout ContextMenu => _contextMenuManager?.GetContextMenu(_contextMenuPath);

        public NavigationMenuItem(string pathId, string containerPath = null, string contextMenuPath = null, DelegateCommand command = null)
            : base(pathId, containerPath, contextMenuPath, null, null, command)
        {
        }
    }
}
