using Windows.UI.Xaml.Controls;

namespace DataCloner.Infrastructure.Modularity
{
    public class ContextMenuItem : MenuFlyoutItem
    {
        public string ContainerPath { get; }

        public ContextMenuItem(string containerPath)
        {
            ContainerPath = containerPath;
        }
    }
}
