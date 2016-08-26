using DataCloner.Universal.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Universal.Views
{
    /// <summary>
    /// The "chrome" layer of the app that provides top-level navigation with
    /// proper keyboarding navigation.
    /// </summary>
    public sealed partial class AppShell : Page
    {
        public static AppShell Current;
        private readonly AppShellViewModel _viewModel;

        public AppShell()
        {
            this.InitializeComponent();

            // Set the data context
            _viewModel = new AppShellViewModel();
            DataContext = _viewModel;

            Loaded += (senderl, args) =>
            {
                Current = this;
            };

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;
        }

        /// <summary>
        /// Gets the app frame.
        /// </summary>
        public Frame AppFrame
        {
            get { return frame; }
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled && AppFrame.CanGoBack)
            {
                e.Handled = true;
                AppFrame.GoBack();
            }
        }
    }
}
