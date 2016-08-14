using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Left
{
    public class DashboardMenuItem : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public DashboardMenuItem(INavigationFacade navigation): base(navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToMainPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/circle.png"));
        }

        public override string Label => "Tableau de board";
    }
}
