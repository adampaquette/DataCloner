using System;
using System.Globalization;

#if NETFX_CORE
using Windows.UI.Xaml.Data;
#elif WPF
using System.Windows;
using System.Windows.Data;
#endif

namespace DataCloner.Infrastructure.Converters
{
    /// <summary>
    /// Agnostic value converter  
    /// </summary>
    public abstract class AgnosticConverter : IValueConverter
    {
        public abstract object AgnosticConvert(object value, Type targetType, object parameter, CultureInfo culture);
        public abstract object AgnosticConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

#if NETFX_CORE
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return AgnosticConvert(value, targetType, parameter, new CultureInfo(language));
        }
#elif WPF
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return AgnosticConvert(value, targetType, parameter, culture);
        }
#endif


#if NETFX_CORE
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return AgnosticConvertBack(value, targetType, parameter, new CultureInfo(language));
        }
#elif WPF
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return AgnosticConvertBack(value, targetType, parameter, culture);
        }
#endif
    }
}
