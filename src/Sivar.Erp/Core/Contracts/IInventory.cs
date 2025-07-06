using System;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Interface for inventory items
    /// </summary>
    public interface IInventoryItem
    {
        string Code { get; set; }
        string Description { get; set; }
        decimal UnitCost { get; set; }
        string? CostingMethod { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for stock levels
    /// </summary>
    public interface IStockLevel
    {
        string ItemCode { get; set; }
        string? LocationCode { get; set; }
        decimal QuantityOnHand { get; set; }
        decimal ReservedQuantity { get; set; }
        decimal AvailableQuantity { get; set; }
        DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Interface for inventory transactions
    /// </summary>
    public interface IInventoryTransaction
    {
        string TransactionNumber { get; set; }
        string ItemCode { get; set; }
        string TransactionType { get; set; }
        decimal Quantity { get; set; }
        decimal UnitCost { get; set; }
        DateOnly TransactionDate { get; set; }
        string? ReferenceNumber { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for inventory reservations
    /// </summary>
    public interface IInventoryReservation
    {
        string ReservationNumber { get; set; }
        string ItemCode { get; set; }
        decimal ReservedQuantity { get; set; }
        string? ReservationReason { get; set; }
        DateTime ReservationDate { get; set; }
        DateTime? ExpirationDate { get; set; }
        bool IsActive { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }
}