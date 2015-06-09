using System.Linq;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Message;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight;

namespace DataCloner.GUI.ViewModel
{
    public class MainViewModel : ViewModelBase
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
                if (Set("CurrentApp", ref _currentApp, value))
                {
                    Settings.Default.DefaultAppId = _currentApp.Id;
                    RaisePropertyChanged("ApplicationName");

                    MessengerInstance.Send(new SelectedApplicationMessage {Application = _currentApp});
                }
            }
        }

        public string ApplicationName { get { return _currentApp?.Name; } }
    }
}