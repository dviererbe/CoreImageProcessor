using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Data;

namespace CoreImageProcessor.Converters
{

    public class EnumDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum enumObj)
        {
            string enumToString = enumObj.ToString();

            FieldInfo? fieldInfo = enumObj.GetType().GetField(enumToString);
            
            if (fieldInfo != null)
            {
                object[] attributeArray = fieldInfo.GetCustomAttributes(false);

                if (attributeArray.Length > 0)
                {
                    foreach (var attribute in attributeArray)
                    {
                        if (attribute is DescriptionAttribute descriptionAttribute)
                            return descriptionAttribute.Description;
                    }
                }
            }

            return enumToString;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return GetEnumDescription((Enum)value);
            }
            catch (Exception)
            {
                return "N/A";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
