using DataCloner.Universal.Commands;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Left
{
    public class EnvironmentDefinitionMenuItem : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public EnvironmentDefinitionMenuItem(INavigationFacade navigation): base(navigation)
        {
            _navigation = navigation;
            Command = new RelayCommand(() => _navigation.NavigateToMainPage());
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/technology.png"));
        }

        public override string Label => "Définition d'environnement";
    }
}
