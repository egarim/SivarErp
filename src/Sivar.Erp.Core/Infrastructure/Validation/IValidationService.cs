using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Infrastructure.Validation
{
    /// <summary>
    /// Service interface for validation operations
    /// </summary>
    [Description("Service interface for validation operations")]
    public interface IValidationService
    {
        /// <summary>
        /// Validates an entity
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates an entity")]
        Task<ValidationResult> ValidateAsync<T>(T entity) where T : class;

        /// <summary>
        /// Validates an entity with specific validation rules
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <param name="ruleSet">Validation rule set to apply</param>
        /// <returns>Validation result</returns>
        [Description("Validates an entity with specific validation rules")]
        Task<ValidationResult> ValidateAsync<T>(T entity, string ruleSet) where T : class;

        /// <summary>
        /// Validates a property value
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="entity">Entity instance</param>
        /// <param name="propertyName">Property name to validate</param>
        /// <param name="value">Property value to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates a property value")]
        Task<ValidationResult> ValidatePropertyAsync<T>(T entity, string propertyName, object? value) where T : class;

        /// <summary>
        /// Validates business rules for an entity
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates business rules for an entity")]
        Task<ValidationResult> ValidateBusinessRulesAsync<T>(T entity) where T : class;

        /// <summary>
        /// Validates data integrity constraints
        /// </summary>
        /// <typeparam name="T">Type of entity to validate</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates data integrity constraints")]
        Task<ValidationResult> ValidateDataIntegrityAsync<T>(T entity) where T : class;

        /// <summary>
        /// Registers a custom validator for a type
        /// </summary>
        /// <typeparam name="T">Type to register validator for</typeparam>
        /// <param name="validator">Validator function</param>
        [Description("Registers a custom validator for a type")]
        void RegisterValidator<T>(Func<T, Task<ValidationResult>> validator) where T : class;

        /// <summary>
        /// Registers a custom property validator
        /// </summary>
        /// <typeparam name="T">Type to register validator for</typeparam>
        /// <param name="propertyName">Property name</param>
        /// <param name="validator">Property validator function</param>
        [Description("Registers a custom property validator")]
        void RegisterPropertyValidator<T>(string propertyName, Func<T, object?, Task<ValidationResult>> validator) where T : class;
    }
}
