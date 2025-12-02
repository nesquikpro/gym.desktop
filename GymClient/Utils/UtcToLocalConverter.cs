using System.Globalization;
using System.Windows.Data;

namespace GymClient.Utils
{
    public class UtcToLocalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                DateTime utcTime;

                switch (dt.Kind)
                {
                    case DateTimeKind.Utc:
                        utcTime = dt;
                        break;

                    case DateTimeKind.Local:
                        // Уже локальное время, конвертировать не нужно
                        return dt;

                    case DateTimeKind.Unspecified:
                    default:
                        // Считаем, что сервер прислал UTC
                        utcTime = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                        break;
                }

                // Конвертируем UTC в локальную Windows-зону
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
            }

            return System.Windows.Data.Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
