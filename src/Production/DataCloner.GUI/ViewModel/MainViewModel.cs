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
        private ConfigurationContainer _config;
        private ApplicationViewModel _currentApp;

        public MainViewModel()
        {
            _config = ConfigurationContainer.Load(ConfigurationContainer.ConfigFileName);
            var app = _config.Applications.FirstOrDefault(a => a.Id == Properties.Settings.Default.DefaultAppId);
            if (app == null)
                app = _config.Applications.FirstOrDefault();

            var defaultMetadata = new MetadataContainer();
            MetadataContainer.VerifyIntegrityOfSqlMetadata(new QueryDispatcher(), app, ref defaultMetadata);

            _currentApp = ConfigurationService.Load(app, defaultMetadata.Metadatas);
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