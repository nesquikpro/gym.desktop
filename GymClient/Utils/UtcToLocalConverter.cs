using System.Globalization;
using System.Windows.Data;

namespace GymClient.Utils
{
    public class UtcToLocalConverter : IValueConverter
    {
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    if (value is DateTime dt)
        //    {
        //        DateTime localTime = dt.Kind switch
        //        {
        //            DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.Local),
        //            DateTimeKind.Local => dt,
        //            _ => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dt, DateTimeKind.Utc), TimeZoneInfo.Local)
        //        };

        //        // Форматируем для России
        //        var russianCulture = new CultureInfo("ru-RU");
        //        return localTime.ToString("dd.MM.yyyy HH:mm", russianCulture);
        //    }
        //    else if (value is DateOnly dateOnly)
        //    {
        //        return dateOnly.ToString("dd.MM.yyyy", new CultureInfo("ru-RU"));
        //    }

        //    return System.Windows.Data.Binding.DoNothing;
        //}

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
                        return dt;

                    case DateTimeKind.Unspecified:
                    default:
                        utcTime = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                        break;
                }

                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
            }
            else if (value is DateOnly dateOnly)
            {
                var dateTime = dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
                return dateTime;
            }

            return System.Windows.Data.Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
