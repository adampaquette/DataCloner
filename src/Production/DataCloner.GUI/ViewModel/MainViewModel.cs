using System.Linq;
using DataCloner.Core.Metadata;

using DataCloner.Core.Configuration;

using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight;
using DataCloner.Core.Data;

using DataCloner.GUI.Services;
using DataCloner.GUI.ViewModel.SolutionExplorer;
using DataCloner.GUI.UserControls;
using System.Collections.ObjectModel;

namespace DataCloner.GUI.ViewModel
{
    class MainViewModel : Framework.ModelBase
    {
        private ProjectContainer _proj;
        private ApplicationViewModel _currentApp;
        private object _solutionExplorer;


        public MainViewModel()
        {
            _proj = ProjectContainer.Load("northWind.dcproj");

            var defaultMetadata = new MetadataContainer();
            MetadataContainer.VerifyIntegrityOfSqlMetadata(new QueryDispatcher(), _proj, ref defaultMetadata);
            _currentApp = ConfigurationService.Load(_proj, defaultMetadata.Metadatas);

            _solutionExplorer = new SolutionExplorer.SolutionExplorerViewModel();
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
    
        public string ApplicationName
        {
            get
            {
                if (_currentApp != null)
                    return _currentApp.Name;
                return null;
            }
        }

        public object SolutionExplorer
        {
            get { return _solutionExplorer; }
            set { SetProperty(ref _solutionExplorer, value); }
        }
    }
}