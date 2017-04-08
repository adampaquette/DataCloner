using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Top
{
    public class HelpMenuItem : IMenuItem
    {
        private INavigationFacade _navigation;

        public HelpMenuItem(INavigationFacade navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToMainPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/web-1.png"));

            ContextMenu = new MenuFlyout();
            ContextMenu.Items.Add(new MenuFlyoutItem { Text = "Documentation..." });
            ContextMenu.Items.Add(new MenuFlyoutItem { Text = "À propos de Datacloner.." });
        }

        public RelayCommand Command { get; }
        public ImageSource Image { get; }
        public string Label => "Aide";
        public MenuItemLocation Location => MenuItemLocation.Top;
        public MenuItemPosition Position => MenuItemPosition.Start;
        public MenuFlyout ContextMenu { get; }
    }
}
