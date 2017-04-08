using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Left
{
    public class EnvironmentsMenuItem : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public EnvironmentsMenuItem(INavigationFacade navigation): base(navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToMainPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/direction.png"));
        }

        public override string Label => "Envrironnements";
    }
}
