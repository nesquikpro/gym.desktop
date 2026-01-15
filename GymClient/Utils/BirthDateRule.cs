using System.Globalization;
using System.Windows.Controls;

namespace GymClient.Utils
{
    public class BirthDateRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "Дата рождения обязательна!");

            if (value is not DateTime date)
                return new ValidationResult(false, "Некорректная дата!");

            if (date > DateTime.Today)
                return new ValidationResult(false, "Дата не может быть в будущем!");

            int age = DateTime.Today.Year - date.Year;
            if (date > DateTime.Today.AddYears(-age))
                age--;

            if (age < 14)
                return new ValidationResult(false, "Возраст должен быть не менее 14 лет!");

            return ValidationResult.ValidResult;
        }
    }
}
