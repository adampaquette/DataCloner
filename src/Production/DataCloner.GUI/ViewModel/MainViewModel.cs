using System.Linq;
using DataCloner.Metadata;
using DataCloner.Configuration;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight;
using DataCloner.Data;
using DataCloner.GUI.Services;
using DataCloner.GUI.UserControls;
using System.Collections.ObjectModel;

namespace DataCloner.GUI.ViewModel
{
    class MainViewModel : Framework.ModelBase
    {
        private ProjectContainer _proj;
        private ApplicationViewModel _currentApp;
        private ObservableCollection<TreeViewLazyItemViewModel> _treeData;

        public MainViewModel()
        {
            _proj = ProjectContainer.Load("northWind.dcproj");

            var defaultMetadata = new MetadataContainer();
            MetadataContainer.VerifyIntegrityOfSqlMetadata(new QueryDispatcher(), _proj, ref defaultMetadata);

            _currentApp = ConfigurationService.Load(_proj, defaultMetadata.Metadatas);

            var srv1 = new ProjectTreeViewModel { Text = "Server NorthWind UNI" };
            var srv2 = new ProjectTreeViewModel { Text = "Server NorthWind FON" };
            var srv3 = new ProjectTreeViewModel { Text = "Server NorthWind ACC" };
            var srv4 = new ProjectTreeViewModel { Text = "Server NorthWind PROD" };

            var project = new ProjectTreeViewModel { Text = "Project Northwind" };
            project.Children.Add(srv1);
            project.Children.Add(srv2);
            project.Children.Add(srv3);
            project.Children.Add(srv4);


            _treeData = new ObservableCollection<TreeViewLazyItemViewModel>();
            _treeData.Add(project);
            _treeData.Add(new ProjectTreeViewModel { Text = "agfdllo" });
            _treeData.Add(new ProjectTreeViewModel { Text = "atregfdsasd" });
            _treeData.Add(new ProjectTreeViewModel { Text = "aresd" });
            _treeData.Add(new ProjectTreeViewModel { Text = "stdafgdasgf" });
            _treeData.Add(new ProjectTreeViewModel { Text = "agfdgfdsd" });
            _treeData.Add(new ProjectTreeViewModel { Text = "agfdgfsd" });
            _treeData.Add(new ProjectTreeViewModel { Text = "adgfdsd" });
            _treeData.Add(new ProjectTreeViewModel { Text = "agrtefdsd" });
            _treeData.Add(new ProjectTreeViewModel { Text = "aghfdsd" });
        }

        public ApplicationViewModel CurrentApp
        {
            get { return _currentApp; }
            set
            {
                if (SetProperty(ref _currentApp, value))
                {
                    Properties.Settings.Default.DefaultAppId = _currentApp.Id;
                    RaisePropertyChanged("ApplicationName");
                }
            }
        }

        public ObservableCollection<TreeViewLazyItemViewModel> TreeData
        {
            get { return _treeData; }
            set { SetProperty(ref _treeData, value); }
        }

        public string ApplicationName
        {
            get
            {
                if (_currentApp != null)
                    return _currentApp.Name;
                return null;
            }
        }
    }
}