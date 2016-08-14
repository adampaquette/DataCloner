using DataCloner.Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
