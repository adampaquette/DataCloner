using DataCloner.Universal.ComponentModel;
using System.Threading.Tasks;

namespace DataCloner.Universal.ViewModels
{
    /// <summary>
    /// A base class for ViewModels.
    /// </summary>
    public class ViewModelBase : ObservableObjectBase
    {
        /// <summary>
        /// Loads the state.
        /// </summary>
        public virtual Task LoadState()
        {
            return Task.CompletedTask;
        }
    }
}
