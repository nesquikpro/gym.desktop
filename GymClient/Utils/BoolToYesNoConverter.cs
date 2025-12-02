using System.Globalization;
using System.Windows.Data;

namespace GymClient.Utils
{
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? "Да" : "Нет";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
                return s.Equals("Да", StringComparison.OrdinalIgnoreCase);
            return false;
        }
    }
}
