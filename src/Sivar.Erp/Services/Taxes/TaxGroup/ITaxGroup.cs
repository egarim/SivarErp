using System;
using System.ComponentModel;

namespace Sivar.Erp.Services.Taxes.TaxGroup
{
    /// <summary>
    /// Interface for tax groups that can be used for tax rule targeting
    /// </summary>
    public interface ITaxGroup : INotifyPropertyChanged
    {
        /// <summary>
        /// Unique identifier for the tax group
        /// </summary>
        Guid Oid { get; set; }
        
        /// <summary>
        /// Unique code for the tax group
        /// </summary>
        string Code { get; set; }
        
        /// <summary>
        /// Display name of the tax group
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Description of the tax group
        /// </summary>
        string Description { get; set; }
        
        /// <summary>
        /// Whether the tax group is currently active
        /// </summary>
        bool IsEnabled { get; set; }
    }
}