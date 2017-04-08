using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Top
{
    public class ClonerMenuItem : IMenuItem
    {
        private INavigationFacade _navigation;

        public ClonerMenuItem(INavigationFacade navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToClonerPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/squares.png"));
        }

        public RelayCommand Command { get; }
        public ImageSource Image { get; }
        public string Label => "Cloner";
        public MenuItemLocation Location => MenuItemLocation.Top;
        public MenuItemPosition Position => MenuItemPosition.Middle;
        public MenuFlyout ContextMenu { get; }
    }
}
