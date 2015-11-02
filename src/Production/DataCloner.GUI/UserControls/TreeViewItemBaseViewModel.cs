using DataCloner.GUI.Framework;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace DataCloner.GUI.UserControls
{
    public class TreeViewItemBaseViewModel : ModelBase
    {
        static readonly TreeViewItemBaseViewModel DummyChild = new TreeViewItemBaseViewModel();

        private bool _isSelected;
        private bool _isExpanded;

        protected TreeViewItemBaseViewModel(TreeViewItemBaseViewModel parent, bool lazyLoadChildren)
        {
            Parent = parent;
            Children = new ObservableCollection<TreeViewItemBaseViewModel>();

            if (lazyLoadChildren)
            {
                Children.Add(DummyChild);
            }
        }

        /// <summary>
        /// For dummy child
        /// </summary>
        private TreeViewItemBaseViewModel() { }

        public TreeViewItemBaseViewModel Parent { get; private set; }
        public ObservableCollection<TreeViewItemBaseViewModel> Children { get; private set; }
        public virtual ImageSource Image { get; protected set; }
        public virtual string Text { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                SetProperty(ref _isExpanded, value);

                //Expand parents to the root
                if (_isExpanded && Parent != null)
                    Parent.IsExpanded = true;

                //Lazy load
                if (HasDummyChild)
                {
                    Children.Clear();
                    LoadChildren();
                }
            }
        }

        private bool HasDummyChild
        {
            get { return Children.Count == 1 && Children[0] == DummyChild; }
        }

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }
    }
}
