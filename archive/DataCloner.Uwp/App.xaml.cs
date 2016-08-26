using DataCloner.Infrastructure.Modularity;
using DataCloner.Uwp.Plugins;
using DataCloner.Uwp.Services;
using DataCloner.Uwp.ViewModels;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Unity.Windows;
using Prism.Windows.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Uwp
{
    sealed partial class App : PrismUnityApplication
    {
        public IEventAggregator EventAggregator { get; set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            var shell = Container.Resolve<Shell>();
            shell.SetContentFrame(rootFrame);
            return shell;
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate("Dashboard", null);

            Window.Current.Activate();
            return Task.FromResult<object>(null);
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            EventAggregator = new EventAggregator();

            Container.RegisterInstance<INavigationService>(NavigationService);
            Container.RegisterInstance<IEventAggregator>(EventAggregator);

            //PC customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                //var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                //if (titleBar != null)
                //{
                //    titleBar.ButtonBackgroundColor = Color.FromArgb(0xFF, 0x24, 0x31, 0x35);
                //    titleBar.ButtonForegroundColor = Colors.White;
                //    titleBar.BackgroundColor = Color.FromArgb(0xFF, 0x24, 0x31, 0x35);
                //    titleBar.ForegroundColor = Colors.White;
                //}

                ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
                formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;
                //https://www.eternalcoding.com/?p=1952
            }

            InitializePlugins();

            return base.OnInitializeAsync(args);
        }

        private void InitializePlugins()
        {
            var pluginList = new List<IPlugin>
            {
                new GeneralMenuPlugin(NavigationService),
                new TestPlugin(NavigationService),
                new TopBarPlugins(NavigationService)
            };

            var pm = new PluginManager<IPlugin>();
            var plugins = pm.LoadPlugins("plugins");
            if (plugins != null)
                pluginList.AddRange(plugins);

            var navigationMenuBuilder = new TreeMenuBuilder();
            var topBarMenuBuilder = new TreeMenuBuilder();

            foreach (var plugin in pluginList)
            {
                plugin.Initialize();
                if (plugin.NavigationMenuItems != null)
                    navigationMenuBuilder.Append(plugin.NavigationMenuItems);
                if (plugin.TopBarMenuItems != null)
                    topBarMenuBuilder.Append(plugin.TopBarMenuItems);
            }

            var navigationMenuViewModel = new NavigationMenuViewModel(NavigationService, navigationMenuBuilder.ToObservableCollection());
            var topBarMenuViewModel = new TopBarMenuViewModel(NavigationService, topBarMenuBuilder.ToObservableCollection());

            Container.RegisterInstance(navigationMenuViewModel);
            Container.RegisterInstance(topBarMenuViewModel);
        }
    }
}
