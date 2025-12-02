using System.Globalization;
using System.Windows.Controls;

namespace GymClient.Utils
{
    public class RequiredRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((value ?? "").ToString()))
                return new ValidationResult(false, "Поле обязательно");
            return ValidationResult.ValidResult;
        }
    }
}
