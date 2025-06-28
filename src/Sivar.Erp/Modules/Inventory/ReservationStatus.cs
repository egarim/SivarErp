namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Enum for inventory reservation statuses
    /// </summary>
    public enum ReservationStatus
    {
        /// <summary>
        /// Reservation is active and valid
        /// </summary>
        Active = 0,
        
        /// <summary>
        /// Reservation has been fulfilled (converted to a transaction)
        /// </summary>
        Fulfilled = 1,
        
        /// <summary>
        /// Reservation has been cancelled
        /// </summary>
        Cancelled = 2,
        
        /// <summary>
        /// Reservation has expired
        /// </summary>
        Expired = 3
    }
}