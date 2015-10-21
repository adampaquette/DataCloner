using System.Linq;
using Cache = DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight;
using DataCloner.DataAccess;
using DataCloner.GUI.Services;

namespace DataCloner.GUI.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private Configuration _config;
        private ApplicationViewModel _currentApp;

        public MainViewModel()
        {
            _config = Configuration.Load(Configuration.ConfigFileName);
            var app = _config.Applications.FirstOrDefault(a => a.Id == Settings.Default.DefaultAppId);
            if (app == null)
                app = _config.Applications.FirstOrDefault();

            var defaultCache = new Cache.Cache();
            Cache.Cache.InitializeSchema(new QueryDispatcher(), app, ref defaultCache);

            _currentApp = ConfigurationService.Load(app, defaultCache.DatabasesSchema);
        }

        public ApplicationViewModel CurrentApp
        {
            get { return _currentApp; }
            set
            {
                if (Set(ref _currentApp, value))
                {
                    Settings.Default.DefaultAppId = _currentApp.Id;
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