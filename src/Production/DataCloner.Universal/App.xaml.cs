using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using DataCloner.Universal.Views;
using DataCloner.Universal.Unity;
using Windows.Globalization;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.LifeCycle;

namespace DataCloner.Universal
{
    /// <summary>
    /// Fournit un comportement spécifique à l'application afin de compléter la classe Application par défaut.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initialise l'objet d'application de singleton.  Il s'agit de la première ligne du code créé
        /// à être exécutée. Elle correspond donc à l'équivalent logique de main() ou WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            // Register for events
            Suspending += OnSuspending;
            UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// Initialize the App launch.
        /// </summary>
        /// <returns>The AppShell of the app.</returns>
        private AppShell Initialize()
        {
            var shell = Window.Current.Content as AppShell;

            if (shell == null)
            {
                UnityBootstrapper.Init();
                UnityBootstrapper.ConfigureRegistries();

                // Create a AppShell to act as the navigation context and navigate to the first page
                shell = new AppShell();

                // Set the default language
                shell.Language = ApplicationLanguages.Languages[0];

                shell.AppFrame.NavigationFailed += OnNavigationFailed;
            }

            return shell;
        }


        /// <summary>
        /// Invoqué lorsque l'application est lancée normalement par l'utilisateur final.  D'autres points d'entrée
        /// seront utilisés par exemple au moment du lancement de l'application pour l'ouverture d'un fichier spécifique.
        /// </summary>
        /// <param name="e">Détails concernant la requête et le processus de lancement.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var shell = Initialize();
            
            // Place our app shell in the current Window
            Window.Current.Content = shell;

            if (shell.AppFrame.Content == null)
            {
                var facade = ServiceLocator.Current.GetInstance<INavigationFacade>();

                if (AppLaunchCounter.IsFirstLaunch())
                    facade.NavigateToWelcomePage();
                else
                    facade.NavigateToMainPage();
            }

            // Refresh launch counter, needs to be done
            // after AppLaunchCounter.IsFirstLaunch() is being checked.
            AppLaunchCounter.RegisterLaunch();
            
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Appelé lorsque l'exécution de l'application est suspendue.  L'état de l'application est enregistré
        /// sans savoir si l'application pourra se fermer ou reprendre sans endommager
        /// le contenu de la mémoire.
        /// </summary>
        /// <param name="sender">Source de la requête de suspension.</param>
        /// <param name="e">Détails de la requête de suspension.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: enregistrez l'état de l'application et arrêtez toute activité en arrière-plan
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the app runs into an unhandled exception. Telemetry logs the
        /// unhandled exception and appropriate message is displayed to the user experiencing
        /// the exception.
        /// </summary>
        /// <param name="sender">The source of the unhandled exception.</param>
        /// <param name="e">Details about the unhandled exception event.</param>
        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var resourceLoader = ResourceLoader.GetForCurrentView();
            var dialog = new MessageDialog(resourceLoader.GetString("UnexpectedError_Message"),
                resourceLoader.GetString("UnexpectedError_Title"));

            await dialog.ShowAsync();
        }
    }
}
