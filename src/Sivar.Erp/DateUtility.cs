namespace Sivar.Erp
{
    /// <summary>
    /// Utility for working with accounting dates
    /// </summary>
    public static class DateUtility
    {
        /// <summary>
        /// Gets the current date in UTC
        /// </summary>
        /// <returns>Current date without time component</returns>
        public static DateOnly GetCurrentDate()
        {
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }

        /// <summary>
        /// Converts a DateOnly to DateTime
        /// </summary>
        /// <param name="date">Date to convert</param>
        /// <returns>DateTime at start of the day in UTC</returns>
        public static DateTime ToDateTime(DateOnly date)
        {
            return date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        }

        /// <summary>
        /// Gets the first day of the month for a given date
        /// </summary>
        /// <param name="date">Reference date</param>
        /// <returns>First day of the month</returns>
        public static DateOnly GetFirstDayOfMonth(DateOnly date)
        {
            return new DateOnly(date.Year, date.Month, 1);
        }

        /// <summary>
        /// Gets the last day of the month for a given date
        /// </summary>
        /// <param name="date">Reference date</param>
        /// <returns>Last day of the month</returns>
        public static DateOnly GetLastDayOfMonth(DateOnly date)
        {
            return new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }
    }
}
