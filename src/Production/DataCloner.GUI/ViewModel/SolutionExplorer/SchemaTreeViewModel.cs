using DataCloner.GUI.UserControls;
using System.Windows.Media;

namespace DataCloner.GUI.ViewModel.SolutionExplorer
{
    public class SchemaTreeViewModel : TreeViewLazyItemViewModel
    {
        private static readonly ImageSource _image = (ImageSource)new ImageSourceConverter().ConvertFromString("pack://application:,,,/Resources/Images/schema.png");

        public SchemaTreeViewModel()
            : base(null, false)
        {
            Image = _image;
        }
    }
}
