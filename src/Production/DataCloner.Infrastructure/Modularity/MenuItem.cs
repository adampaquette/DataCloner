using DataCloner.Infrastructure.UserControls;
using Prism.Commands;
using System;

namespace DataCloner.Infrastructure.Modularity
{
    public class MenuItem : TreeViewLazyItemViewModel, IEquatable<NavigationMenuItem>
    {
        protected ContextMenuManager _contextMenuManager;
        protected string _contextMenuPath;

        public string PathId { get; }
        public string ContainerPath { get; }
        public DelegateCommand Command { get; }

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
            var other = obj as NavigationMenuItem;
            return Equals(other);
        }

        public bool Equals(NavigationMenuItem other)
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
