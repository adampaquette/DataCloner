using System.Linq;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight;

namespace DataCloner.GUI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private Configuration _config;
        private Application _currentApp;

        public MainViewModel()
        {
            _config = Configuration.Load(Configuration.ConfigFileName);
            _currentApp = _config.Applications.FirstOrDefault(a => a.Id == Settings.Default.DefaultAppId);
            if (_currentApp == null)
                _currentApp = _config.Applications.FirstOrDefault();

            //if (IsInDesignMode)
            //{
            //    // Code runs in Blend --> create design time data.
            //}
            //else
            //{
            //    // Code runs "for real"
            //}
        }

        public Application CurrentApp
        {
            get { return _currentApp; }
            set
            {
                if (Set("", ref _currentApp, value))
                {
                    Settings.Default.DefaultAppId = _currentApp.Id;
                    RaisePropertyChanged("Application");
                }
            }
        }

        public string ApplicationName { get { return _currentApp.Name; } }
    }
}