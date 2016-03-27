using Prism.Commands;

namespace DataCloner.Infrastructure.Modularity
{
    public class MenuEntry
    {
        public string EntryName { get; }
        public DelegateCommand Command { get; }

        public MenuEntry(string entryName, DelegateCommand command)
        {
            EntryName = entryName;
            Command = command;
        }
    }
}
