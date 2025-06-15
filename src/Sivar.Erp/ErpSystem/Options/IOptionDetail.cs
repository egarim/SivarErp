using System;

namespace Sivar.Erp.System.Options
{
    /// <summary>
    /// Interface for option detail entities that store time-bound values
    /// </summary>
    public interface IOptionDetail : IEntity
    {
        /// <summary>
        /// Reference to the parent option
        /// </summary>
        Guid OptionId { get; set; }
        
        /// <summary>
        /// Reference to the selected option choice
        /// </summary>
        Guid OptionChoiceId { get; set; }
        
        /// <summary>
        /// The actual value for this option setting
        /// </summary>
        string Value { get; set; }
        
        /// <summary>
        /// Date from which this option detail is valid
        /// </summary>
        DateTime ValidFrom { get; set; }
        
        /// <summary>
        /// Date until which this option detail is valid (null means indefinitely)
        /// </summary>
        DateTime? ValidTo { get; set; }
        
        /// <summary>
        /// Whether this detail is active
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// When this detail was created
        /// </summary>
        DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Who created this detail
        /// </summary>
        string? CreatedBy { get; set; }
    }
}