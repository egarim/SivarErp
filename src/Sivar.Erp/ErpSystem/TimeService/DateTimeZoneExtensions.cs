using System;

namespace Sivar.Erp.ErpSystem.TimeService
{
    /// <summary>
    /// Extension methods for IDateTimeZoneTrackable
    /// </summary>
    public static class DateTimeZoneExtensions
    {
        /// <summary>
        /// Sets the date, time, and timezone from a UTC DateTime
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <param name="utcDateTime">DateTime in UTC</param>
        public static void SetFromUtc(this IDateTimeZoneTrackable entity, DateTime utcDateTime)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // Ensure the DateTime is in UTC
            DateTime normalizedUtc = utcDateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc)
                : utcDateTime.ToUniversalTime();

            entity.Date = DateOnly.FromDateTime(normalizedUtc);
            entity.Time = TimeOnly.FromDateTime(normalizedUtc);
            entity.TimeZoneId = "UTC";
        }

        /// <summary>
        /// Sets the date, time, and timezone from a local DateTime
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <param name="localDateTime">DateTime in local timezone</param>
        public static void SetFromLocal(this IDateTimeZoneTrackable entity, DateTime localDateTime)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // Convert to UTC
            DateTime utcDateTime = localDateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(localDateTime, DateTimeKind.Local).ToUniversalTime()
                : localDateTime.ToUniversalTime();

            entity.Date = DateOnly.FromDateTime(utcDateTime);
            entity.Time = TimeOnly.FromDateTime(utcDateTime);
            entity.TimeZoneId = "UTC";
        }

        /// <summary>
        /// Gets a DateTime representing this entity's date and time in UTC
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <param name="service">DateTimeZone service to use for conversion</param>
        /// <returns>DateTime in UTC</returns>
        public static DateTime ToUtcDateTime(this IDateTimeZoneTrackable entity, IDateTimeZoneService service)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return service.ToUtc(entity);
        }
        
        /// <summary>
        /// Gets a DateTime representing this entity's date and time in the local timezone
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <param name="service">DateTimeZone service to use for conversion</param>
        /// <returns>DateTime in local timezone</returns>
        public static DateTime ToLocalDateTime(this IDateTimeZoneTrackable entity, IDateTimeZoneService service)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return service.ToTimeZone(entity, TimeZoneInfo.Local.Id);
        }
    }
}