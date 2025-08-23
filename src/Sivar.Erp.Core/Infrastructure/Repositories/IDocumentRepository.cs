using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for document-specific operations
    /// </summary>
    [Description("Repository interface for document-specific operations")]
    public interface IDocumentRepository : IRepository
    {
        /// <summary>
        /// Gets documents by business entity
        /// </summary>
        /// <param name="businessEntityId">Business entity ID</param>
        /// <returns>Documents for the business entity</returns>
        [Description("Gets documents by business entity")]
        Task<IEnumerable<IDocument>> GetDocumentsByBusinessEntityAsync(Guid businessEntityId);

        /// <summary>
        /// Gets documents by document type
        /// </summary>
        /// <param name="documentTypeId">Document type ID</param>
        /// <returns>Documents of the specified type</returns>
        [Description("Gets documents by document type")]
        Task<IEnumerable<IDocument>> GetDocumentsByTypeAsync(Guid documentTypeId);

        /// <summary>
        /// Gets documents by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Documents within the date range</returns>
        [Description("Gets documents by date range")]
        Task<IEnumerable<IDocument>> GetDocumentsByDateRangeAsync(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Gets document by document number
        /// </summary>
        /// <param name="documentNumber">Document number</param>
        /// <returns>Document with the specified number</returns>
        [Description("Gets document by document number")]
        Task<IDocument?> GetDocumentByNumberAsync(string documentNumber);

        /// <summary>
        /// Gets pending documents for approval
        /// </summary>
        /// <returns>Documents pending approval</returns>
        [Description("Gets pending documents for approval")]
        Task<IEnumerable<IDocument>> GetPendingDocumentsAsync();
    }
}
