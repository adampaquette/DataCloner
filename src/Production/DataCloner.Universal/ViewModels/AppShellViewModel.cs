using DataCloner.Core.Configuration;
using DataCloner.Universal.Menu;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataCloner.Universal.ViewModels
{
    public class AppShellViewModel : ViewModelBase
    {
        private bool _isBusy;

        public AppShellViewModel()
        {
            NavigationBarMenuItemsTop = ServiceLocator.Current
                .GetAllInstances<IMenuItem>()
                .Where(i => i.Location == MenuItemLocation.Top)
                .ToList();

            //NavigationBarMenuItemsLeft = ServiceLocator.Current
            //    .GetAllInstances<ITreeViewMenuItem>()
            //    .Where(i => i.Location == MenuItemLocation.Left)
            //    .ToList();

            //var proj = ProjectContainer.Load("northWind.dcproj");



        }

        /// <summary>
        /// The navigation bar items at the top.
        /// </summary>
        public List<IMenuItem> NavigationBarMenuItemsTop { get; private set; }

        /// <summary>
        /// The navigation bar items at the left.
        /// </summary>
        public List<ITreeViewMenuItem> NavigationBarMenuItemsLeft { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    NotifyPropertyChanged(nameof(IsBusy));
                }
            }
        }

        public override async Task LoadState()
        {
            await base.LoadState();

            IsBusy = true;

            try
            {
                var proj = await ProjectContainer.LoadAsync(@"C:\Users\Naster\Source\Repos\DataCloner\src\Production\DataCloner.Universal\northWind.dcproj");


            }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
