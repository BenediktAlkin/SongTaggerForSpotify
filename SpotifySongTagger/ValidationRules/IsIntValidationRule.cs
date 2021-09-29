using System.Globalization;
using System.Windows.Controls;

namespace SpotifySongTagger.ValidationRules
{
    public class IsIntValidationRule : ValidationRule
    {
        public string ErrorText { get; set; } = "Not a number";
        public bool AllowNull { get; set; } = true;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var valueStr = (value ?? "").ToString();
            if (string.IsNullOrEmpty(valueStr)) 
                return AllowNull ? ValidationResult.ValidResult : new ValidationResult(false, ErrorText);

            if (int.TryParse(valueStr, out _))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, ErrorText);
        }
    }
}
