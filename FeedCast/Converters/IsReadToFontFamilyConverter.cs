/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace FeedCast.Converters
{
    public class IsReadToFontFamilyConverter : IValueConverter
    {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(null != value)
            {
                return (((bool)value) ? Application.Current.Resources["PhoneFontFamilySemiLight"] : Application.Current.Resources["PhoneFontFamilySemiBold"]);
            }
            return Application.Current.Resources["PhoneFontFamilySemiLight"];
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FontFamily fontFamily = value as FontFamily;
            if (null != fontFamily)
            {
                return fontFamily == Application.Current.Resources["PhoneFontFamilySemiLight"];
            }
            return false;
        }
    }
}
