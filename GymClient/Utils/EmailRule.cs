using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace GymClient.Utils
{
    public class EmailRule : ValidationRule
    {
        private static readonly Regex _emailRegex =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string email = (value ?? "").ToString();
            if (string.IsNullOrWhiteSpace(email))
                return new ValidationResult(false, "Email обязателен");
            if (!_emailRegex.IsMatch(email))
                return new ValidationResult(false, "Неверный формат Email");
            return ValidationResult.ValidResult;
        }
    }
}
