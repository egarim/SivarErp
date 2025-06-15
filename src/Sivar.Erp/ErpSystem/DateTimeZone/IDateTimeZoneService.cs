using System;

namespace Sivar.Erp.ErpSystem.DateTimeZone
{
    /// <summary>
    /// Service for handling date, time, and timezone operations
    /// </summary>
    public interface IDateTimeZoneService
    {
        /// <summary>
        /// Sets the date, time, and timezone for an entity
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <param name="dateTime">Source DateTime</param>
        /// <param name="sourceTimeZoneId">Source timezone ID (null means UTC)</param>
        void SetDateTimeZone(IDateTimeZoneTrackable entity, DateTime dateTime, string? sourceTimeZoneId = null);
        
        /// <summary>
        /// Converts the separate date, time, and timezone to a UTC DateTime
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <returns>DateTime in UTC</returns>
        DateTime ToUtc(IDateTimeZoneTrackable entity);
        
        /// <summary>
        /// Converts the separate date, time, and timezone to a DateTime in the specified timezone
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <param name="targetTimeZoneId">Target timezone ID</param>
        /// <returns>DateTime in the target timezone</returns>
        DateTime ToTimeZone(IDateTimeZoneTrackable entity, string targetTimeZoneId);
        
        /// <summary>
        /// Gets a list of available timezone IDs
        /// </summary>
        /// <returns>Array of timezone IDs</returns>
        string[] GetAvailableTimeZones();
        
        /// <summary>
        /// Gets the current system timezone ID
        /// </summary>
        /// <returns>Current system timezone ID</returns>
        string GetSystemTimeZoneId();
    }
}