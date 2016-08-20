using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Uwp
{
    public sealed partial class Shell : Page
    {
        public Shell()
        {
            this.InitializeComponent();
        }

        public void SetContentFrame(Frame frame)
        {
            rootSplitView.Content = frame;
        }

        public void SetMenuPaneContent(UIElement content)
        {
            rootSplitView.Pane = content;
        }
    }
}
