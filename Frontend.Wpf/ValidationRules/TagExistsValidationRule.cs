using Backend;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace Frontend.Wpf.ValidationRules
{
    public class TagExistsValidationRule : ValidationRule
    {
        public bool Invert { get; set; }
        public string ErrorText { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var tagName = (value ?? "").ToString();
            var tagExists = DatabaseOperations.TagExists(tagName);
            if (Invert)
                tagExists = !tagExists;
            return tagExists
                ? new ValidationResult(false, ErrorText)
                : ValidationResult.ValidResult;
        }
    }
}
