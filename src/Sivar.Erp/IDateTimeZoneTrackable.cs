namespace Sivar.Erp
{
    /// <summary>
    /// Interface for entities that store date, time, and timezone information separately
    /// </summary>
    public interface IDateTimeZoneTrackable
    {
        /// <summary>
        /// The date component
        /// </summary>
        DateOnly Date { get; set; }
        
        /// <summary>
        /// The time component
        /// </summary>
        TimeOnly Time { get; set; }
        
        /// <summary>
        /// The timezone identifier (IANA or Windows format, e.g., "America/El_Salvador" or "Central America Standard Time")
        /// </summary>
        string TimeZoneId { get; set; }
    }
}