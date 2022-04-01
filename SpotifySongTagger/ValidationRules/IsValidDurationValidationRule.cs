using System;
using System.Globalization;
using System.Windows.Controls;

namespace SpotifySongTagger.ValidationRules
{
    public class IsValidDurationValidationRule : ValidationRule
    {
        public string ErrorText { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string durationString))
                return new ValidationResult(false, ErrorText);

            if (durationString == string.Empty)
                return ValidationResult.ValidResult;

            if (TimeSpan.TryParseExact(durationString, "m\\:ss", CultureInfo.InvariantCulture, out var timespan))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, ErrorText);
        }
    }
}
