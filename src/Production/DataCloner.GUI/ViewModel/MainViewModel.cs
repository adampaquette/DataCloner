using System.Linq;
using DataCloner.Metadata;
using DataCloner.Configuration;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight;
using DataCloner.Data;
using DataCloner.GUI.Services;

namespace DataCloner.GUI.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private ProjectContainer _proj;
        private ApplicationViewModel _currentApp;

        public MainViewModel()
        {
            _proj = ProjectContainer.Load("northWind.dcproj");

            var defaultMetadata = new MetadataContainer();
            MetadataContainer.VerifyIntegrityOfSqlMetadata(new QueryDispatcher(), _proj, ref defaultMetadata);

            _currentApp = ConfigurationService.Load(_proj, defaultMetadata.Metadatas);
        }

        public ApplicationViewModel CurrentApp
        {
            get { return _currentApp; }
            set
            {
                if (Set(ref _currentApp, value))
                {
                    Properties.Settings.Default.DefaultAppId = _currentApp.Id;
                    RaisePropertyChanged("ApplicationName");
                }
            }
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