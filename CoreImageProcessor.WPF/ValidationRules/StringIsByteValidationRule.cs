using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CoreImageProcessor.ValidationRules
{
    class StringIsByteValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (byte.TryParse(value.ToString(), out byte number))
            {
                return new ValidationResult(true, null);
            }

            return new ValidationResult(false, "Please enter a valid byte value.");
        }
    }
}
