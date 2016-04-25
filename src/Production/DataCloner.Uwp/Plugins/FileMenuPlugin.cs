using DataCloner.Infrastructure.Modularity;
using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataCloner.Uwp.Plugins
{
    public class FileMenuPlugin : IPlugin
    {
        private INavigationService _navigationService;

        public List<NavigationMenuItem> NavigationMenuItems { get; private set; }
        public List<NavigationMenuItem> FileMenuItems { get; private set; }

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

        public FileMenuPlugin(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            FileMenuItems = new List<NavigationMenuItem>
            {
                new NavigationMenuItem(FileMenuPath, null, null, null, null, null) { Text = "Fichier" },
                new NavigationMenuItem(NewProjectMenuPath, FileMenuPath, null, null, null, new DelegateCommand(() =>  Debug.WriteLine("NEW PROJECT"))) { Text = "Nouveau projet..." },
                new NavigationMenuItem(OpenProjectMenuPath, FileMenuPath, null, null, null, new DelegateCommand(() => Debug.WriteLine("OPEN PROJECT"))) { Text = "Ouvrir le projet..." },
                new NavigationMenuItem(OpenFileQueryMenuPath, FileMenuPath, null, null, null, new DelegateCommand(() => Debug.WriteLine("OPEN FILE QUERY"))) { Text = "Ouvrir le fichier de requête..." },

                new NavigationMenuItem(ToolsMenuPath, null, null, null, null, null) { Text = "Outils" },
                new NavigationMenuItem(StaticTablesMenuPath, FileMenuPath, null, null, null, new DelegateCommand(() =>  Debug.WriteLine("STATIC"))) { Text = "Gestion des tables statiques" },
                new NavigationMenuItem(TablesRestrictionsMenuPath, FileMenuPath, null, null, null, new DelegateCommand(() =>  Debug.WriteLine("restriction"))) { Text = "Restriction des tables" },
                new NavigationMenuItem(DetectDataGenerationMenuPath, FileMenuPath, null, null, null, new DelegateCommand(() =>  Debug.WriteLine("génération données"))) { Text = "Détection de génération de données" },

                new NavigationMenuItem(HelpMenuPath, null, null, null, null, null) { Text = "Aide" },

                new NavigationMenuItem(AboutMenuPath, null, null, null, null, null) { Text = "À propos" }
            };
        }
    }
}
