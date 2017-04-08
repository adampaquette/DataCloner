using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.Models;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Top
{
    public class FileMenuItem : IMenuItem
    {
        private INavigationFacade _navigation;

        public FileMenuItem(INavigationFacade navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToMainPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/interface.png"));

            ContextMenu = new MenuFlyout();
            ContextMenu.Items.Add(new MenuFlyoutItem { Text = "Nouveau projet...", Command = new RelayCommand(NewProjectAsync) });
            ContextMenu.Items.Add(new MenuFlyoutItem { Text = "Ouvrir un projet...", Command = new RelayCommand(OpenProjectAsync) });
            ContextMenu.Items.Add(new MenuFlyoutItem { Text = "Ouvrir une requête...", Command = new RelayCommand(OpenProjectAsync) });
        }

        public RelayCommand Command { get; }
        public ImageSource Image { get; }
        public string Label => "Fichier";
        public MenuItemLocation Location => MenuItemLocation.Top;
        public MenuItemPosition Position => MenuItemPosition.Start;
        public MenuFlyout ContextMenu { get; }

        private async void NewProjectAsync()
        {           
        }

        private async void OpenProjectAsync()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".dcp");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                AppModelContext.Instance.CurrentFilePath = file.Path;
            }
        }

        private async void OpenQueryAsync()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".dcq");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                AppModelContext.Instance.CurrentFilePath = file.Path;
            }
        }
    }
}
