using Microsoft.Xaml.Interactivity;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace DataCloner.Universal.Actions
{
    /// <summary>
    /// Opens the attached menu flyout.
    /// </summary>
    public class OpenMenuFlyoutAction : DependencyObject, IAction
    {
        public MenuFlyoutLocation Location { get; set; }
        public Style MenuFlyoutPresenterStyle { get; set; }
        public Style MenuFlyoutItemStyle { get; set; }
        public MenuFlyout ContextMenu
        {
            get { return (MenuFlyout)GetValue(OpenMenuFlyoutAction.ContextMenuProperty); }
            set { SetValue(OpenMenuFlyoutAction.ContextMenuProperty, value); }
        }

        public static readonly DependencyProperty ContextMenuProperty =
            DependencyProperty.Register("ContextMenu", typeof(MenuFlyout), typeof(OpenMenuFlyoutAction), new PropertyMetadata(null));

        public object Execute(object sender, object parameter)
        {
            if (ContextMenu == null)
                return null;

            Point offset;
            if (Location == MenuFlyoutLocation.BelowSenderObject)
            {
                if (sender is FrameworkElement element)
                {
                    var transform = element.TransformToVisual(Window.Current.Content);
                    offset = transform.TransformPoint(new Point(0, 0));
                    offset.Y += element.ActualHeight;
                }
            }
            else
            {
                if (parameter is RightTappedRoutedEventArgs rightClickEvent)
                    offset = rightClickEvent.GetPosition(null);
                if (parameter is TappedRoutedEventArgs leftClickEvent)
                    offset = leftClickEvent.GetPosition(null);
            }

            if (MenuFlyoutPresenterStyle != null)
                ContextMenu.MenuFlyoutPresenterStyle = MenuFlyoutPresenterStyle;

            foreach (var item in ContextMenu.Items)
            {
                if (item.GetType() == typeof(MenuFlyoutItem))
                    item.Style = MenuFlyoutItemStyle;
            }

            ContextMenu.ShowAt(null, offset);
            return null;
        }

        public enum MenuFlyoutLocation
        {
            PointerLocation,
            BelowSenderObject
        }
    }
}