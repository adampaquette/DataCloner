using System.Linq;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight;

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
            _currentApp = new ApplicationViewModel(app);
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