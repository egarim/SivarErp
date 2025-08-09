using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents.Models
{
    /// <summary>
    /// Implementation of the IBusinessEntity interface
    /// </summary>
    public class BusinessEntityDto : Entity, IBusinessEntity
    {
        /// <summary>
        /// Gets or sets the business entity code
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the business entity name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the business entity type
        /// </summary>
        public BusinessEntityType EntityType { get; set; }
        
        /// <summary>
        /// Gets or sets the tax identification number
        /// </summary>
        public string? TaxId { get; set; }
        
        /// <summary>
        /// Gets or sets whether the business entity is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}