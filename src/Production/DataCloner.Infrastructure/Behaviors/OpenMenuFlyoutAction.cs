using Microsoft.Xaml.Interactivity;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace DataCloner.Infrastructure.Behaviors
{
    public class OpenMenuFlyoutAction : DependencyObject, IAction
    {
        public Style MenuFlyoutPresenter { get; set; }

        public IList<MenuFlyoutItemBase> MenuItems { get; set; }

        public object Execute(object sender, object parameter)
        {
            Point offset;
            var rightClickEvent = parameter as RightTappedRoutedEventArgs;
            if (rightClickEvent != null)
                offset = rightClickEvent.GetPosition(null);

            var senderElement = sender as FrameworkElement;
            
            var flyout = FlyoutBase.GetAttachedFlyout(senderElement) as MenuFlyout;
            //flyout.ShowAt(null, offset);

            //var flyout = new MenuFlyout();
            flyout.MenuFlyoutPresenterStyle = MenuFlyoutPresenter;
            flyout.ShowAt(null, offset);

            return null;
        }
    }
}
