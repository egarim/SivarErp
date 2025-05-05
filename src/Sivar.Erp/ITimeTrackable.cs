namespace Sivar.Erp
{
    /// <summary>
    /// Interface for entities that require time tracking
    /// </summary>
    public interface ITimeTrackable
    {
        /// <summary>
        /// The time component for time-specific records
        /// </summary>
        TimeOnly Time { get; set; }
    }
}
