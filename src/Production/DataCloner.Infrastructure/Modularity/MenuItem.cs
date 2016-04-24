using DataCloner.Infrastructure.UserControls;
using Prism.Commands;
using System;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Infrastructure.Modularity
{
    public class MenuItem : TreeViewLazyItemViewModel, IEquatable<MenuItem>
    {
        private ContextMenuManager _contextMenuManager;
        private string _contextMenuPath;

        public string PathId { get; }
        public string ContainerPath { get; }
        public DelegateCommand Command { get; }
        public MenuFlyout ContextMenu => _contextMenuManager?.GetContextMenu(_contextMenuPath);

        public MenuItem(string pathId, string containerPath, string contextMenuPath, TreeViewLazyItemViewModel parent, 
            ContextMenuManager contextMenuManager, DelegateCommand command)
            : base(parent, false)
        {
            PathId = pathId;
            ContainerPath = containerPath;
            Command = command;
            _contextMenuPath = contextMenuPath;
            _contextMenuManager = contextMenuManager;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MenuItem;
            return Equals(other);
        }

        public bool Equals(MenuItem other)
        {
            if (other != null)
                return PathId == other.PathId;
            return false;
        }

        public override int GetHashCode()
        {
            return PathId.GetHashCode();
        }
    }
}
