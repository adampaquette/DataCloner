﻿using Microsoft.Xaml.Interactivity;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DataCloner.Infrastructure.Behaviors
{
    public class OpenMenuFlyoutAction : DependencyObject, IAction
    {
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
            var rightClickEvent = parameter as RightTappedRoutedEventArgs;
            if (rightClickEvent != null)
                offset = rightClickEvent.GetPosition(null);

            var leftClickEvent = parameter as TappedRoutedEventArgs;
            if (leftClickEvent != null)
                offset = leftClickEvent.GetPosition(null);

            if (MenuFlyoutPresenterStyle != null)
                ContextMenu.MenuFlyoutPresenterStyle = MenuFlyoutPresenterStyle;

            foreach (var item in ContextMenu.Items)
            {
                if(item.GetType() == typeof(MenuFlyoutItem))
                    item.Style = MenuFlyoutItemStyle;
            }

            ContextMenu.ShowAt(null, offset);
            return null;
        }
    }
}
