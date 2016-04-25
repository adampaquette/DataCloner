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

            InitializePlugins();

            return base.OnInitializeAsync(args);
        }

        private void InitializePlugins()
        {
            var pluginList = new List<IPlugin>
            {
                new GeneralMenuPlugin(NavigationService),
                new TestPlugin(NavigationService),
                new FileMenuPlugin(NavigationService)
            };

            var pm = new PluginManager<IPlugin>();
            var plugins = pm.LoadPlugins("plugins");
            if (plugins != null)
                pluginList.AddRange(plugins);

            var navigationMenuBuilder = new TreeMenuBuilder();
            var fileMenuBuilder = new TreeMenuBuilder();

            foreach (var plugin in pluginList)
            {
                plugin.Initialize();
                if (plugin.NavigationMenuItems != null)
                    navigationMenuBuilder.Append(plugin.NavigationMenuItems);
                if (plugin.FileMenuItems != null)
                    fileMenuBuilder.Append(plugin.FileMenuItems);
            }

            var navigationMenuViewModel = new NavigationMenuViewModel(NavigationService, navigationMenuBuilder.ToObservableCollection());
            var fileMenuViewModel = new FileMenuViewModel(NavigationService, fileMenuBuilder.ToObservableCollection());

            Container.RegisterInstance(navigationMenuViewModel);
            Container.RegisterInstance(fileMenuViewModel);
        }
    }
}
