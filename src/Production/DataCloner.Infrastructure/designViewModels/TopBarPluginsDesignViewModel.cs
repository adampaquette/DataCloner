using DataCloner.Infrastructure.Modularity;
using Prism.Windows.Mvvm;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Infrastructure.designViewModels
{
    public class TopBarPluginsDesignViewModel : ViewModelBase
    {
        private ObservableCollection<NavigationMenuItem> _menuItems;

        public ObservableCollection<NavigationMenuItem> MenuItems
        {
            get { return _menuItems; }
            set { SetProperty(ref _menuItems, value); }
        }

        #region Menu path declaration

        public string FileMenuPath => "FILESECTION";
        public string ToolsMenuPath => "TOOLSSECTION";
        public string HelpMenuPath => "HELPSECTION";
        public string AboutMenuPath => "ABOUTSECTION";

        #endregion

        public TopBarPluginsDesignViewModel()
        {
            MenuItems = new ObservableCollection<NavigationMenuItem>
            {
                new NavigationMenuItem(FileMenuPath)
                {
                    Text = "Fichier",
                    IconSrc = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/interface.png"))
                },
                new NavigationMenuItem(ToolsMenuPath)
                {
                    Text = "Outils",
                    IconSrc = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/tool.png"))
                },
                new NavigationMenuItem(HelpMenuPath)
                {
                    Text = "Aide",
                    IconSrc = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/people.png"))
                },
                new NavigationMenuItem(AboutMenuPath)
                {
                    Text = "À propos",
                    IconSrc = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/web-1.png"))
                }
            };
        }
    }
}
