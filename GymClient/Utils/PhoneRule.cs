using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace GymClient.Utils
{
    public class PhoneRule : ValidationRule
    {
        private static readonly Regex _phoneRegex =
            new Regex(@"^\+?\d{10,15}$", RegexOptions.Compiled);

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string phone = (value ?? "").ToString();
            if (string.IsNullOrWhiteSpace(phone))
                return new ValidationResult(false, "Телефон обязателен");
            if (!_phoneRegex.IsMatch(phone))
                return new ValidationResult(false, "Неверный формат телефона");
            return ValidationResult.ValidResult;
        }
    }
}
