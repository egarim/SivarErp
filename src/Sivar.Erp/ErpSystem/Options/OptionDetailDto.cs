using System;

namespace Sivar.Erp.ErpSystem.Options
{
    /// <summary>
    /// Data transfer object for option details that store time-bound values
    /// </summary>
    public class OptionDetailDto : IOptionDetail
    {
        /// <summary>
        /// Unique identifier for this option detail
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Reference to the parent option
        /// </summary>
        public Guid OptionId { get; set; }

        /// <summary>
        /// Reference to the selected option choice
        /// </summary>
        public Guid OptionChoiceId { get; set; }

        /// <summary>
        /// The actual value for this option setting
        /// </summary>
        public required string Value { get; set; }

        /// <summary>
        /// Date from which this option detail is valid
        /// </summary>
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date until which this option detail is valid (null means indefinitely)
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// Whether this detail is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When this detail was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Who created this detail
        /// </summary>
        public string? CreatedBy { get; set; }
        
        /// <summary>
        /// Validates this option detail
        /// </summary>
        /// <returns>True if the option detail is valid, false otherwise</returns>
        public bool Validate()
        {
            return OptionId != Guid.Empty &&
                   OptionChoiceId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(Value);
        }
        
        /// <summary>
        /// Checks if this option detail is active for a given date
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if this detail is active on the given date</returns>
        public bool IsActiveOnDate(DateTime date)
        {
            return IsActive &&
                   ValidFrom <= date &&
                   (!ValidTo.HasValue || ValidTo.Value >= date);
        }
    }
}