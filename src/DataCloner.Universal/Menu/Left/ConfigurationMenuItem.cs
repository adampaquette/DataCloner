using System.Threading.Tasks;
using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;

namespace DataCloner.Universal.Menu.Left
{
    public class ConfigurationMenuItem : TreeViewMenuItemBase
    {
        public ConfigurationMenuItem(INavigationFacade navigation) : base(navigation)
        {
            Children.Add(new SqlServersMenuItem(Navigation));
        }

        public override string Label => "Configuration";
    }    
}
