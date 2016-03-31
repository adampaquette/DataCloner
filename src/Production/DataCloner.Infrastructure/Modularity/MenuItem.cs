﻿using DataCloner.Infrastructure.UserControls;
using Prism.Commands;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Infrastructure.Modularity
{
    public class MenuItem : TreeViewLazyItemViewModel
    {
        private ContextMenuManager _contextMenuManager;
        private string _contextMenuPath;

        public string Id { get; }
        public string ContainerPath { get; }
        public string Name { get; }
        public DelegateCommand Command { get; }
        public MenuFlyout ContextMenu => _contextMenuManager.GetContextMenu(_contextMenuPath);

        public MenuItem(string id, string containerPath, string name, DelegateCommand command,
            string contextMenuPath, TreeViewLazyItemViewModel parent, ContextMenuManager contextMenuManager)
            : base(parent, false)
        {
            Id = id;
            ContainerPath = containerPath;
            Name = name;
            Command = command;
            _contextMenuPath = contextMenuPath;
            _contextMenuManager = contextMenuManager;
        }
    }
}
