using Prism.Commands;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Infrastructure.Modularity
{
    public class MenuItem
    {
        public string EntryName { get; }
        public DelegateCommand Command { get; }

        public MenuFlyout ContextMenu { get; }

        public MenuItem(string entryName, DelegateCommand command, MenuFlyout contextMenu)
        {
            EntryName = entryName;
            Command = command;
            ContextMenu = contextMenu;
        }
    }
}
