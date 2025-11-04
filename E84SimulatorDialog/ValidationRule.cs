using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace E84SimulatorDialog
{
    public class PositiveNumberValidationRule : ValidationRule
    {
        public string ErrorMessage { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && int.TryParse(value.ToString(), out int number) && number >= 0)
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, ErrorMessage);
        }
    }
}
