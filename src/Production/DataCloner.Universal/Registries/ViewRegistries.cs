using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;
using DataCloner.Universal.Views;

namespace DataCloner.Universal.Registries
{
    public class ViewRegistry : IRegistry
    {
        public void Configure()
        {
            NavigationFacade.AddType(typeof(MainPage), typeof(MainPageViewModel));
            NavigationFacade.AddType(typeof(WelcomePage), typeof(WelcomePageViewModel));
            NavigationFacade.AddType(typeof(ClonerPage), typeof(ClonerPageViewModel));
        }
    }
}
