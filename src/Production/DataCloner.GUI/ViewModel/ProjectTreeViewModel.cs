using DataCloner.GUI.UserControls;
using System.Windows.Media;

namespace DataCloner.GUI.ViewModel
{
    public class ProjectTreeViewModel : TreeViewItemBaseViewModel
    {
        //private static readonly ImageSource _image = (ImageSource)new ImageSourceConverter().ConvertFromString("Resources/images/seo1.png");

        public ProjectTreeViewModel() : base(null, true)
        {
            //Image = _image;
        }
    }
}
