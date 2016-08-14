using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using Windows.UI.Xaml.Media;

namespace DataCloner.Universal.Menu.Top
{
    public class FileMenuItem : IMenuItem
    {
        private INavigationFacade _navigation;

        public FileMenuItem(INavigationFacade navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToMainPage());
        }

        public RelayCommand Command { get; }
        public ImageSource Image => null;
        public string Label => "Fichier";
        public MenuItemLocation Location => MenuItemLocation.Top;
        public MenuItemPosition Position => MenuItemPosition.Start;
    }
}
