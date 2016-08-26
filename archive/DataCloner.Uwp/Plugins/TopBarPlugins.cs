using DataCloner.Infrastructure.Modularity;
using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Uwp.Plugins
{
    public class TopBarPlugins : IPlugin
    {
        private INavigationService _navigationService;

        public List<NavigationMenuItem> NavigationMenuItems { get; private set; }
        public List<NavigationMenuItem> TopBarMenuItems { get; private set; }

        #region Menu path declaration

        public string FileMenuPath => "FILESECTION";
        public string NewProjectMenuPath => FileMenuPath + "_NEWPROJECT";
        public string OpenProjectMenuPath => FileMenuPath + "_OPENPROJECT";
        public string OpenFileQueryMenuPath => FileMenuPath + "_OPENFILEQUERY";

        public string ToolsMenuPath => "TOOLSSECTION";
        public string StaticTablesMenuPath => ToolsMenuPath + "_STATICTABLES";
        public string TablesRestrictionsMenuPath => ToolsMenuPath + "_TABLESRESTRICTIONS";
        public string DetectDataGenerationMenuPath => ToolsMenuPath + "_DETECTDATAGENERATION";

        public string HelpMenuPath => "HELPSECTION";

        public string AboutMenuPath => "ABOUTSECTION";

        #endregion

        public TopBarPlugins(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            TopBarMenuItems = new List<NavigationMenuItem>
            {
                new NavigationMenuItem(FileMenuPath, command: new DelegateCommand(() =>  Debug.WriteLine("file menu")))
                {
                    Text = "Fichier",
                    IconSrc = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/interface.png"))
                },
                new NavigationMenuItem(NewProjectMenuPath, FileMenuPath,
                    command: new DelegateCommand(() =>  Debug.WriteLine("NEW PROJECT")))
                {
                    Text = "Nouveau projet..."
                },
                new NavigationMenuItem(OpenProjectMenuPath, FileMenuPath,
                    command:new DelegateCommand(() => Debug.WriteLine("OPEN PROJECT")))
                {
                    Text = "Ouvrir le projet..."
                },
                new NavigationMenuItem(OpenFileQueryMenuPath, FileMenuPath,
                    command: new DelegateCommand(() => Debug.WriteLine("OPEN FILE QUERY")))
                {
                    Text = "Ouvrir le fichier de requête..."
                },

                new NavigationMenuItem(ToolsMenuPath)
                {
                    Text = "Outils",
                    IconSrc = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/wrench.png"))
                },
                new NavigationMenuItem(StaticTablesMenuPath, FileMenuPath, 
                    command:new DelegateCommand(() =>  Debug.WriteLine("STATIC")))
                {
                    Text = "Gestion des tables statiques"
                },
                new NavigationMenuItem(TablesRestrictionsMenuPath, FileMenuPath, 
                    command: new DelegateCommand(() =>  Debug.WriteLine("restriction")))
                {
                    Text = "Restriction des tables"
                },
                new NavigationMenuItem(DetectDataGenerationMenuPath, FileMenuPath,  
                    command: new DelegateCommand(() =>  Debug.WriteLine("génération données")))
                {
                    Text = "Détection de génération de données"
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
