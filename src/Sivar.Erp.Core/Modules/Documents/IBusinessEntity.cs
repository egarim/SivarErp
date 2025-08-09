using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a business entity such as a customer, vendor, or employee
    /// </summary>
    [Description("Represents a business entity such as a customer, vendor, or employee")]
    public interface IBusinessEntity
    {
        /// <summary>
        /// Gets or sets the business entity code
        /// </summary>
        string Code { get; set; }
        
        /// <summary>
        /// Gets or sets the business entity name
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the business entity type
        /// </summary>
        BusinessEntityType EntityType { get; set; }
        
        /// <summary>
        /// Gets or sets the tax identification number
        /// </summary>
        string? TaxId { get; set; }
        
        /// <summary>
        /// Gets or sets whether the business entity is active
        /// </summary>
        bool IsActive { get; set; }
    }
    
    /// <summary>
    /// Types of business entities
    /// </summary>
    public enum BusinessEntityType
    {
        /// <summary>
        /// Customer
        /// </summary>
        Customer,
        
        /// <summary>
        /// Vendor/Supplier
        /// </summary>
        Vendor,
        
        /// <summary>
        /// Employee
        /// </summary>
        Employee,
        
        /// <summary>
        /// Other
        /// </summary>
        Other
    }
}