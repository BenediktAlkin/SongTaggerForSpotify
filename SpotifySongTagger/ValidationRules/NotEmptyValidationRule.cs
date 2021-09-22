using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace SpotifySongTagger.ValidationRules
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public string ErrorText { get; set; } = "Field is required";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var valueStr = (value ?? "").ToString();
            return string.IsNullOrWhiteSpace(valueStr)
                ? new ValidationResult(false, ErrorText)
                : ValidationResult.ValidResult;
        }
    }
}
