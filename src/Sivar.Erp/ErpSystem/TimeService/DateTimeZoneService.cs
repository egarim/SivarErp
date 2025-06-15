using System;
using System.Collections.Generic;
using System.Linq;

namespace Sivar.Erp.ErpSystem.TimeService
{
    /// <summary>
    /// Implementation of the service for handling date, time, and timezone operations
    /// </summary>
    public class DateTimeZoneService : IDateTimeZoneService
    {
        /// <summary>
        /// Sets the date, time, and timezone for an entity
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <param name="dateTime">Source DateTime</param>
        /// <param name="sourceTimeZoneId">Source timezone ID (null means UTC)</param>
        public void SetDateTimeZone(IDateTimeZoneTrackable entity, DateTime dateTime, string? sourceTimeZoneId = null)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            // If source timezone is provided, convert to UTC first
            DateTime utcDateTime;
            if (!string.IsNullOrEmpty(sourceTimeZoneId))
            {
                try
                {
                    TimeZoneInfo sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZoneId);
                    utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, sourceTimeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    throw new ArgumentException($"The timezone ID '{sourceTimeZoneId}' was not found.", nameof(sourceTimeZoneId));
                }
                catch (InvalidTimeZoneException)
                {
                    throw new ArgumentException($"The timezone ID '{sourceTimeZoneId}' is invalid.", nameof(sourceTimeZoneId));
                }
            }
            else
            {
                // Assume UTC if not specified
                utcDateTime = dateTime.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                    : dateTime.ToUniversalTime();
            }
            
            // Set the properties
            //entity.Date = DateOnly.FromDateTime(utcDateTime);
            //entity.Time = TimeOnly.FromDateTime(utcDateTime);
            //entity.TimeZoneId = "UTC"; // Store that we're using UTC
        }

        /// <summary>
        /// Converts the separate date, time, and timezone to a UTC DateTime
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <returns>DateTime in UTC</returns>
        public DateTime ToUtc(IDateTimeZoneTrackable entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            // Create a DateTime from the components
            DateTime dateTime = entity.Date.ToDateTime(entity.Time);
            
            // If already UTC, return as is
            if (entity.TimeZoneId == "UTC")
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            
            try
            {
                // Otherwise convert from source timezone to UTC
                TimeZoneInfo sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(entity.TimeZoneId);
                return TimeZoneInfo.ConvertTimeToUtc(dateTime, sourceTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new ArgumentException($"The timezone ID '{entity.TimeZoneId}' was not found.");
            }
            catch (InvalidTimeZoneException)
            {
                throw new ArgumentException($"The timezone ID '{entity.TimeZoneId}' is invalid.");
            }
        }

        /// <summary>
        /// Converts the separate date, time, and timezone to a DateTime in the specified timezone
        /// </summary>
        /// <param name="entity">Entity implementing IDateTimeZoneTrackable</param>
        /// <param name="targetTimeZoneId">Target timezone ID</param>
        /// <returns>DateTime in the target timezone</returns>
        public DateTime ToTimeZone(IDateTimeZoneTrackable entity, string targetTimeZoneId)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            if (string.IsNullOrEmpty(targetTimeZoneId))
            {
                throw new ArgumentNullException(nameof(targetTimeZoneId));
            }
            
            try
            {
                // First convert to UTC
                DateTime utcDateTime = ToUtc(entity);
                
                // Then convert to target timezone
                TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(targetTimeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new ArgumentException($"The timezone ID '{targetTimeZoneId}' was not found.", nameof(targetTimeZoneId));
            }
            catch (InvalidTimeZoneException)
            {
                throw new ArgumentException($"The timezone ID '{targetTimeZoneId}' is invalid.", nameof(targetTimeZoneId));
            }
        }

        /// <summary>
        /// Gets a list of available timezone IDs
        /// </summary>
        /// <returns>Array of timezone IDs</returns>
        public string[] GetAvailableTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => tz.Id)
                .ToArray();
        }

        /// <summary>
        /// Gets the current system timezone ID
        /// </summary>
        /// <returns>Current system timezone ID</returns>
        public string GetSystemTimeZoneId()
        {
            return TimeZoneInfo.Local.Id;
        }
    }
}