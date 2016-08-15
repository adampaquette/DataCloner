using System.Threading.Tasks;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;
using Windows.ApplicationModel.Resources;

namespace DataCloner.Universal.Menu.Left
{
    public class GeneralMenuItem : TreeViewMenuItemBase
    {
        public GeneralMenuItem(INavigationFacade navigation) : base(navigation)
        {
            Children.Add(new DashboardMenuItem(Navigation));
            Children.Add(new ClonerMenuItem(Navigation));
        }

        public override string Label => "Général";
    }    
}


