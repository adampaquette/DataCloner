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
        private ObservableCollection<TreeViewItemBaseViewModel> _treeData;

        public MainViewModel()
        {
            _proj = ProjectContainer.Load("northWind.dcproj");

            var defaultMetadata = new MetadataContainer();
            MetadataContainer.VerifyIntegrityOfSqlMetadata(new QueryDispatcher(), _proj, ref defaultMetadata);

            _currentApp = ConfigurationService.Load(_proj, defaultMetadata.Metadatas);


            _treeData = new ObservableCollection<TreeViewItemBaseViewModel>();
            _treeData.Add(new ProjectTreeViewModel { Text="asd" });
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

        public ObservableCollection<TreeViewItemBaseViewModel> TreeData
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