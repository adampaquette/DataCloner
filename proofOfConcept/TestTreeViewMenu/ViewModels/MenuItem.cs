using System.Collections.ObjectModel;

namespace TestTreeViewMenu.ViewModels
{
    public class MenuItem : ViewModelBase
    {
        private string _text;
        private ObservableCollection<MenuItem> _children;

        public string Text
        {
            get { return _text; }
            set { Set(ref _text, value); }
        }

        public ObservableCollection<MenuItem> Children
        {
            get { return _children; }
            set { Set(ref _children, value); }
        } 
    }
}
