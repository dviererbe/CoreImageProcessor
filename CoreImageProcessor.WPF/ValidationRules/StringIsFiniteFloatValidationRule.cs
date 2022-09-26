using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CoreImageProcessor.ValidationRules
{
    public class StringIsFiniteFloatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (float.TryParse(value.ToString(), out float number))
            {
                if (float.IsFinite(number))
                    return new ValidationResult(true, null);

                return new ValidationResult(false, "Please enter a finite value.");
            }

            return new ValidationResult(false, "Please enter a valid float value.");
        }
    }
}
