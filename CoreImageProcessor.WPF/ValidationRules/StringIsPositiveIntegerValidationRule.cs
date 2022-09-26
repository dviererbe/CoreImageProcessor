using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CoreImageProcessor.ValidationRules
{
    public class StringIsPositiveIntegerValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (int.TryParse(value.ToString(), out int number))
            {
                if (number < 1)
                    return new ValidationResult(false, "Value has to be an Integer >= 1.");

                return new ValidationResult(true, null);
            }

            return new ValidationResult(false, "Please enter a valid integer value.");
        }
    }
}
