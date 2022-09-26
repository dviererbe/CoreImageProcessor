using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CoreImageProcessor.Converters
{
    public class BooleanInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new NotSupportedException();

            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            else
                throw new NotSupportedException();
        }

        //The Inverse of the Boolean Inverse Function is the Boolean Inverse Function itself.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);
    }
}
