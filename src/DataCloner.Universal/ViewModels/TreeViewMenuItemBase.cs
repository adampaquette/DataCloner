using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.Menu;
using Windows.UI.Xaml.Media;

namespace DataCloner.Universal.ViewModels
{
    public abstract class TreeViewMenuItemBase : TreeViewLazyItem, ITreeViewMenuItem
    {
        public TreeViewMenuItemBase(INavigationFacade navigation) : base(null, false)
        {
            Navigation = navigation;
        }

        protected INavigationFacade Navigation { get; }

        public virtual RelayCommand Command { get; protected set; }
        public virtual ImageSource Image { get; protected set; }
        public abstract string Label { get; }
        public virtual MenuItemLocation Location => MenuItemLocation.Left;

        public virtual MenuItemPosition Position => MenuItemPosition.Start;
    }
}
