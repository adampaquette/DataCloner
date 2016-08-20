using Prism.Windows.Navigation;

namespace DataCloner.Uwp.ViewModels
{
    public class ShellViewModel
    {
        private INavigationService _navigationService;

        public ShellViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
    }
}