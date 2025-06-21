using Sivar.Erp.Documents;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.Documents
{
    /// <summary>
    /// Service for adding accounting totals to documents
    /// </summary>
    public interface IDocumentTotalsService
    {
        /// <summary>
        /// Adds accounting totals to a document based on document operation
        /// </summary>
        /// <param name="document">The document to add totals to</param>
        /// <param name="documentOperation">The document operation type (e.g. "SalesInvoice")</param>
        /// <returns>True if totals were added successfully</returns>
        bool AddDocumentAccountingTotals(IDocument document, string documentOperation);

        /// <summary>
        /// Creates a document accounting profile
        /// </summary>
        /// <param name="profile">The profile to create</param>
        /// <param name="userName">The user creating the profile</param>
        /// <returns>True if profile was created successfully</returns>
        Task<bool> CreateDocumentAccountingProfileAsync(IDocumentAccountingProfile profile, string userName);

        /// <summary>
        /// Gets a document accounting profile by document operation
        /// </summary>
        /// <param name="documentOperation">The document operation to find a profile for</param>
        /// <returns>The found profile or null if not found</returns>
        IDocumentAccountingProfile GetDocumentAccountingProfile(string documentOperation);
    }
}