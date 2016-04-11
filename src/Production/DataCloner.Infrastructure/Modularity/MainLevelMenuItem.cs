using DataCloner.Infrastructure.UserControls;
using Prism.Commands;

namespace DataCloner.Infrastructure.Modularity
{
    public class MainLevelMenuItem : MenuItem
    {
        public MainLevelMenuItem(string id, string containerPath, DelegateCommand command,
                             string contextMenuPath, TreeViewLazyItemViewModel parent, ContextMenuManager contextMenuManager)
            : base(id, containerPath, command, contextMenuPath, parent, contextMenuManager)
        {
        }
    }
}
