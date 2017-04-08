using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Top
{
    public class SettingsMenuItem : IMenuItem
    {
        private INavigationFacade _navigation;

        public SettingsMenuItem(INavigationFacade navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToClonerPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/tool.png"));
        }

        public RelayCommand Command { get; }
        public ImageSource Image { get; }
        public string Label => "Settings";
        public MenuItemLocation Location => MenuItemLocation.Top;
        public MenuItemPosition Position => MenuItemPosition.End;
        public MenuFlyout ContextMenu { get; }
    }
}
