using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CoreImageProcessor.ValidationRules
{
    public class StringIsDoubleBetweenMinus300AndPlus300ValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (double.TryParse(value.ToString(), out double number))
            {
                if (!double.IsFinite(number))
                    return new ValidationResult(false, "Value has to be a finite number.");

                if (number < -300d || number > 300d)
                    return new ValidationResult(false, "Value has to be a between -300 and 300.");

                return new ValidationResult(true, null);
            }

            return new ValidationResult(false, "Please enter a valid double value.");
        }
    }
}
