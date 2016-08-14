using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;

namespace DataCloner.Universal.Menu.Left
{
    public class GeneralMenuItemSection : TreeViewLazyItem<IMenuItem>
    {
        public GeneralMenuItemSection(INavigationFacade navigationFacade) : base(null, true)
        {
            Content = new GeneralMenuItem(navigationFacade);
            //Children.Add()
        }
    }    
}
