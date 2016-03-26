using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Uwp
{
    public sealed partial class AppShell : Page
    {
        public AppShell()
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
