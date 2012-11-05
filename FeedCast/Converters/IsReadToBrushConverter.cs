// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace FeedCast.Converters
{
    public class IsReadToBrushConverter : IValueConverter
    {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(null != value)
            {
                return (((bool)value) ? Application.Current.Resources["PhoneForegroundBrush"] : Application.Current.Resources["PhoneAccentBrush"]);
            }
            return Application.Current.Resources["PhoneForegroundBrush"];
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Brush brush = value as SolidColorBrush;
            if (null != brush)
            {
                return brush == Application.Current.Resources["PhoneForegroundBrush"];
            }
            return false;
        }
    }
}
