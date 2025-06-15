using System;

namespace Sivar.Erp.ErpSystem.DateTimeZone
{
    /// <summary>
    /// Base implementation of IDateTimeZoneTrackable that can be inherited by entities
    /// </summary>
    public class DateTimeZoneTrackableBase : IDateTimeZoneTrackable
    {
        /// <summary>
        /// The date component
        /// </summary>
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        
        /// <summary>
        /// The time component
        /// </summary>
        public TimeOnly Time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
        
        /// <summary>
        /// The timezone identifier
        /// </summary>
        public string TimeZoneId { get; set; } = "UTC";

        /// <summary>
        /// Default constructor initializes with current UTC date and time
        /// </summary>
        public DateTimeZoneTrackableBase()
        {
        }

        /// <summary>
        /// Constructor that initializes with the specified UTC DateTime
        /// </summary>
        /// <param name="utcDateTime">DateTime in UTC</param>
        public DateTimeZoneTrackableBase(DateTime utcDateTime)
        {
            DateTime normalized = utcDateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc)
                : utcDateTime.ToUniversalTime();
                
            Date = DateOnly.FromDateTime(normalized);
            Time = TimeOnly.FromDateTime(normalized);
        }
    }
}