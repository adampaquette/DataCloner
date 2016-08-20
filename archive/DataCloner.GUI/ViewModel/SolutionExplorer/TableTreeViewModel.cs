using DataCloner.GUI.UserControls;
using System.Windows.Media;

namespace DataCloner.GUI.ViewModel.SolutionExplorer
{
    public class TableTreeViewModel : TreeViewLazyItemViewModel
    {
        private static readonly ImageSource _image = (ImageSource)new ImageSourceConverter().ConvertFromString("pack://application:,,,/Resources/Images/table.png");

        public TableTreeViewModel()
            : base(null, false)
        {
            Image = _image;
        }
    }
}