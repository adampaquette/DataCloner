using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Left
{
    public class ClonerMenuItem : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public ClonerMenuItem(INavigationFacade navigation): base(navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToClonerPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/squares.png"));
        }

        public override string Label => "Cloner";
    }
}
