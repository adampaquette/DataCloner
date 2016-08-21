using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Top
{
    public class ToolsMenuItem : IMenuItem
    {
        private INavigationFacade _navigation;

        public ToolsMenuItem(INavigationFacade navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToMainPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/wrench.png"));
        }

        public RelayCommand Command { get; }
        public ImageSource Image { get; }
        public string Label => "Outils";
        public MenuItemLocation Location => MenuItemLocation.Top;
        public MenuItemPosition Position => MenuItemPosition.Start;
    }
}
