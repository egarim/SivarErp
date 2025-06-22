using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Services.Documents
{
    /// <summary>
    /// Interface for managing document accounting profiles
    /// </summary>
    public interface IDocumentAccountingProfileService
    {
        /// <summary>
        /// Get document accounting profile by operation
        /// </summary>
        /// <param name="documentOperation">The document operation</param>
        /// <returns>The document accounting profile</returns>
        Task<DocumentAccountingProfileDto> GetProfileByOperationAsync(string documentOperation);

        /// <summary>
        /// Get all document accounting profiles
        /// </summary>
        /// <returns>List of all document accounting profiles</returns>
        Task<List<DocumentAccountingProfileDto>> GetAllProfilesAsync();

        /// <summary>
        /// Create a new document accounting profile
        /// </summary>
        /// <param name="profile">The profile to create</param>
        /// <param name="username">The username creating the profile</param>
        /// <returns>The created profile</returns>
        Task<DocumentAccountingProfileDto> CreateProfileAsync(DocumentAccountingProfileDto profile, string username);

        /// <summary>
        /// Update an existing document accounting profile
        /// </summary>
        /// <param name="profile">The profile with updated values</param>
        /// <param name="username">The username updating the profile</param>
        /// <returns>The updated profile</returns>
        Task<DocumentAccountingProfileDto> UpdateProfileAsync(DocumentAccountingProfileDto profile, string username);

        /// <summary>
        /// Delete a document accounting profile
        /// </summary>
        /// <param name="documentOperation">The operation of the profile to delete</param>
        /// <param name="username">The username deleting the profile</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteProfileAsync(string documentOperation, string username);
    }
}
