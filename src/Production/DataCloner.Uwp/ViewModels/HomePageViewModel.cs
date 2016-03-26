using Prism.Windows.Mvvm;

namespace DataCloner.Uwp.ViewModels
{
    class HomePageViewModel : ViewModelBase
    {
        private string _title = "DataCloner V1";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}
