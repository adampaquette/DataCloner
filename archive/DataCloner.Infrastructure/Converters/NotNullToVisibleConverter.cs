using System;
using System.Globalization;

#if NETFX_CORE
using Windows.UI.Xaml;
#elif WPF
using System.Windows;
#endif

namespace DataCloner.Infrastructure.Converters
{
    /// <summary>
    /// Value converter that translates NOT null to <see cref="Visibility.Visible"/> and the opposite to <see cref="Visibility.Collapsed"/> 
    /// </summary>
    public sealed class NotNullToVisibleConverter : AgnosticConverter
    {
        public override object AgnosticConvert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object AgnosticConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Visibility && (Visibility)value == Visibility.Visible) ? true : false;
        }
    }
}
