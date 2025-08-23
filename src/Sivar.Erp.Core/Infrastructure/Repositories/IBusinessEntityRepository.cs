using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for business entity-specific operations
    /// </summary>
    [Description("Repository interface for business entity-specific operations")]
    public interface IBusinessEntityRepository : IRepository
    {
        /// <summary>
        /// Gets business entities by type
        /// </summary>
        /// <param name="entityType">Type of business entity</param>
        /// <returns>Business entities of the specified type</returns>
        [Description("Gets business entities by type")]
        Task<IEnumerable<IBusinessEntity>> GetByTypeAsync(string entityType);

        /// <summary>
        /// Gets business entity by tax identification number
        /// </summary>
        /// <param name="taxId">Tax identification number</param>
        /// <returns>Business entity with the specified tax ID</returns>
        [Description("Gets business entity by tax identification number")]
        Task<IBusinessEntity?> GetByTaxIdAsync(string taxId);

        /// <summary>
        /// Gets business entity by business registration number
        /// </summary>
        /// <param name="registrationNumber">Business registration number</param>
        /// <returns>Business entity with the specified registration number</returns>
        [Description("Gets business entity by business registration number")]
        Task<IBusinessEntity?> GetByRegistrationNumberAsync(string registrationNumber);

        /// <summary>
        /// Searches business entities by name
        /// </summary>
        /// <param name="searchTerm">Search term for name matching</param>
        /// <returns>Business entities matching the search term</returns>
        [Description("Searches business entities by name")]
        Task<IEnumerable<IBusinessEntity>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Gets active business entities
        /// </summary>
        /// <returns>Active business entities</returns>
        [Description("Gets active business entities")]
        Task<IEnumerable<IBusinessEntity>> GetActiveEntitiesAsync();

        /// <summary>
        /// Gets business entities with outstanding balances
        /// </summary>
        /// <returns>Business entities with outstanding balances</returns>
        [Description("Gets business entities with outstanding balances")]
        Task<IEnumerable<IBusinessEntity>> GetEntitiesWithOutstandingBalancesAsync();
    }
}
