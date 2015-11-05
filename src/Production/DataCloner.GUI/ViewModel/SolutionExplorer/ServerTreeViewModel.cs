using DataCloner.GUI.UserControls;
using System.Windows.Media;

namespace DataCloner.GUI.ViewModel.SolutionExplorer
{
    public class ServerTreeViewModel : TreeViewLazyItemViewModel
    {
        private static readonly ImageSource _image = (ImageSource)new ImageSourceConverter().ConvertFromString("pack://application:,,,/Resources/Images/server.png");

        public ServerTreeViewModel()
            : base(null, false)
        {
            Image = _image;
        }
    }
}