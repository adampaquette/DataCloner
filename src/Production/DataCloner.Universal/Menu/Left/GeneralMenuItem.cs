using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using Windows.UI.Xaml.Media;

namespace DataCloner.Universal.Menu.Left
{
    public class GeneralMenuItem : IMenuItem
    {
        public GeneralMenuItem(INavigationFacade navigation)
        {
        }

        public RelayCommand Command { get; }
        public ImageSource Image { get; }
        public string Label => "Général";
        public MenuItemLocation Location => MenuItemLocation.Left;
        public MenuItemPosition Position => MenuItemPosition.Start;
    }
}
