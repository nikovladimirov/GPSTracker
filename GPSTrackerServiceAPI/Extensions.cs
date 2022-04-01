using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSTrackerServiceAPI
{
    public static class Extensions
    {
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public static string ToStringDegree(this double value)
        {
            return value.ToString("0.00000000", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var e in enumerable)
            {
                action(e);
            }
        }
        public static string ToSQL(this DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormat);
        }
        public static double FromSqlFloat(this string value)
        {
            return Single.Parse(value, CultureInfo.InvariantCulture);
        }
        public static string ToSQL(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
