using System;
using System.Globalization;

namespace AspxExamples.Common.Models
{
    public static class DateTimeAdapter
    {
        private static readonly string[] DateFormats = new[]
        {
            "dd.MM.yyyy",
            "d.M.yyyy",
            "d.MM.yyyy",
            "dd.M.yyyy"
        };

        public static DateTime? ConvertToDateTime(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;

            // Tarihi parse etmeyi dene
            if (DateTime.TryParseExact(
                dateString,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }

            return null;
        }

        public static string ConvertToString(DateTime? date)
        {
            if (!date.HasValue)
                return string.Empty;

            return date.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}
