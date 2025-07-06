using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Infrastructure.Configuration
{
    /// <summary>
    /// Interface for option service
    /// </summary>
    public interface IOptionService
    {
        #region Option Management
        
        /// <summary>
        /// Gets an option by its identifier
        /// </summary>
        /// <param name="id">Option identifier</param>
        /// <returns>Option if found, null otherwise</returns>
        Task<OptionDto?> GetOptionAsync(Guid id);
        
        /// <summary>
        /// Gets an option by its code and module name
        /// </summary>
        /// <param name="code">Option code</param>
        /// <param name="moduleName">Module name</param>
        /// <returns>Option if found, null otherwise</returns>
        Task<OptionDto?> GetOptionByCodeAsync(string code, string moduleName);
        
        /// <summary>
        /// Gets all options for a specific module
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <returns>Collection of options</returns>
        Task<IEnumerable<OptionDto>> GetOptionsByModuleAsync(string moduleName);
        
        /// <summary>
        /// Creates a new option
        /// </summary>
        /// <param name="option">Option to create</param>
        /// <returns>Created option</returns>
        Task<OptionDto> CreateOptionAsync(OptionDto option);
        
        /// <summary>
        /// Updates an existing option
        /// </summary>
        /// <param name="option">Option to update</param>
        /// <returns>Updated option</returns>
        Task<OptionDto> UpdateOptionAsync(OptionDto option);
        
        /// <summary>
        /// Deletes an option
        /// </summary>
        /// <param name="id">Option identifier</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteOptionAsync(Guid id);
        
        #endregion
        
        #region Option Choice Management
        
        /// <summary>
        /// Gets an option choice by its identifier
        /// </summary>
        /// <param name="id">Option choice identifier</param>
        /// <returns>Option choice if found, null otherwise</returns>
        Task<OptionChoiceDto?> GetOptionChoiceAsync(Guid id);
        
        /// <summary>
        /// Gets all choices for a specific option
        /// </summary>
        /// <param name="optionId">Option identifier</param>
        /// <returns>Collection of option choices</returns>
        Task<IEnumerable<OptionChoiceDto>> GetChoicesForOptionAsync(Guid optionId);
        
        /// <summary>
        /// Creates a new option choice
        /// </summary>
        /// <param name="choice">Option choice to create</param>
        /// <returns>Created option choice</returns>
        Task<OptionChoiceDto> CreateOptionChoiceAsync(OptionChoiceDto choice);
        
        /// <summary>
        /// Updates an existing option choice
        /// </summary>
        /// <param name="choice">Option choice to update</param>
        /// <returns>Updated option choice</returns>
        Task<OptionChoiceDto> UpdateOptionChoiceAsync(OptionChoiceDto choice);
        
        /// <summary>
        /// Deletes an option choice
        /// </summary>
        /// <param name="id">Option choice identifier</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteOptionChoiceAsync(Guid id);
        
        #endregion
        
        #region Option Detail Management
        
        /// <summary>
        /// Gets an option detail by its identifier
        /// </summary>
        /// <param name="id">Option detail identifier</param>
        /// <returns>Option detail if found, null otherwise</returns>
        Task<OptionDetailDto?> GetOptionDetailAsync(Guid id);
        
        /// <summary>
        /// Gets the active option detail for a specific option at a given date
        /// </summary>
        /// <param name="optionId">Option identifier</param>
        /// <param name="effectiveDate">Effective date</param>
        /// <returns>Option detail if found, null otherwise</returns>
        Task<OptionDetailDto?> GetActiveOptionDetailAsync(Guid optionId, DateTime effectiveDate);
        
        /// <summary>
        /// Gets all option details for a specific option (history)
        /// </summary>
        /// <param name="optionId">Option identifier</param>
        /// <returns>Collection of option details</returns>
        Task<IEnumerable<OptionDetailDto>> GetOptionHistoryAsync(Guid optionId);
        
        /// <summary>
        /// Creates a new option detail
        /// </summary>
        /// <param name="detail">Option detail to create</param>
        /// <returns>Created option detail</returns>
        Task<OptionDetailDto> CreateOptionDetailAsync(OptionDetailDto detail);
        
        /// <summary>
        /// Updates an existing option detail
        /// </summary>
        /// <param name="detail">Option detail to update</param>
        /// <returns>Updated option detail</returns>
        Task<OptionDetailDto> UpdateOptionDetailAsync(OptionDetailDto detail);
        
        /// <summary>
        /// Deletes an option detail
        /// </summary>
        /// <param name="id">Option detail identifier</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteOptionDetailAsync(Guid id);
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Gets the current value of an option
        /// </summary>
        /// <param name="optionCode">Option code</param>
        /// <param name="moduleName">Module name</param>
        /// <param name="effectiveDate">Optional effective date, defaults to current date</param>
        /// <returns>Current option value if found, null otherwise</returns>
        Task<string?> GetCurrentOptionValueAsync(string optionCode, string moduleName, DateTime? effectiveDate = null);
        
        /// <summary>
        /// Sets the value of an option for a specific period
        /// </summary>
        /// <param name="optionCode">Option code</param>
        /// <param name="moduleName">Module name</param>
        /// <param name="value">Option value</param>
        /// <param name="validFrom">Valid from date</param>
        /// <param name="validTo">Optional valid to date</param>
        /// <param name="userName">User making the change</param>
        /// <returns>True if set, false otherwise</returns>
        Task<bool> SetOptionValueAsync(string optionCode, string moduleName, string value, DateTime validFrom, DateTime? validTo = null, string? userName = null);
        
        #endregion
    }
}