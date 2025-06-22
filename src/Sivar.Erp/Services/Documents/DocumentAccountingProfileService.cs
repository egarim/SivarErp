using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.Modules;

namespace Sivar.Erp.Services.Documents
{
    /// <summary>
    /// Service for managing document accounting profiles
    /// </summary>
    public class DocumentAccountingProfileService : IDocumentAccountingProfileService
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<DocumentAccountingProfileService> _logger;

        public DocumentAccountingProfileService(
            IObjectDb objectDb,
            ILogger<DocumentAccountingProfileService> logger)
        {
            _objectDb = objectDb;
            _logger = logger;
        }

        /// <summary>
        /// Get document accounting profile by operation
        /// </summary>
        /// <param name="documentOperation">The document operation</param>
        /// <returns>The document accounting profile</returns>
        public async Task<DocumentAccountingProfileDto> GetProfileByOperationAsync(string documentOperation)
        {
            var profile = _objectDb.DocumentAccountingProfiles?.FirstOrDefault(p => p.DocumentOperation == documentOperation);
            return await Task.FromResult(profile as DocumentAccountingProfileDto);
        }

        /// <summary>
        /// Get all document accounting profiles
        /// </summary>
        /// <returns>List of all document accounting profiles</returns>
        public async Task<List<DocumentAccountingProfileDto>> GetAllProfilesAsync()
        {
            var profiles = _objectDb.DocumentAccountingProfiles?
                .Select(p => p as DocumentAccountingProfileDto)
                .Where(p => p != null)
                .ToList() ?? new List<DocumentAccountingProfileDto>();

            return await Task.FromResult(profiles);
        }

        /// <summary>
        /// Create a new document accounting profile
        /// </summary>
        /// <param name="profile">The profile to create</param>
        /// <param name="username">The username creating the profile</param>
        /// <returns>The created profile</returns>
        public async Task<DocumentAccountingProfileDto> CreateProfileAsync(DocumentAccountingProfileDto profile, string username)
        {
            if (_objectDb.DocumentAccountingProfiles == null)
            {
                _objectDb.DocumentAccountingProfiles = new List<IDocumentAccountingProfile>();
            }

            // Check if profile already exists
            if (_objectDb.DocumentAccountingProfiles.Any(p => p.DocumentOperation == profile.DocumentOperation))
            {
                return await UpdateProfileAsync(profile, username);
            }

            profile.CreatedBy = username;
            profile.CreatedDate = DateTimeOffset.Now;

            _objectDb.DocumentAccountingProfiles.Add(profile);
            _logger.LogInformation("Created document accounting profile for operation {Operation}", profile.DocumentOperation);

            return await Task.FromResult(profile);
        }

        /// <summary>
        /// Update an existing document accounting profile
        /// </summary>
        /// <param name="profile">The profile with updated values</param>
        /// <param name="username">The username updating the profile</param>
        /// <returns>The updated profile</returns>
        public async Task<DocumentAccountingProfileDto> UpdateProfileAsync(DocumentAccountingProfileDto profile, string username)
        {
            if (_objectDb.DocumentAccountingProfiles == null)
            {
                _objectDb.DocumentAccountingProfiles = new List<IDocumentAccountingProfile>();
                return await CreateProfileAsync(profile, username);
            }

            var existingProfile = _objectDb.DocumentAccountingProfiles
                .FirstOrDefault(p => p.DocumentOperation == profile.DocumentOperation) as DocumentAccountingProfileDto;

            if (existingProfile == null)
            {
                return await CreateProfileAsync(profile, username);
            }

            // Update properties
            existingProfile.SalesAccountCode = profile.SalesAccountCode;
            existingProfile.AccountsReceivableCode = profile.AccountsReceivableCode;
            existingProfile.InventoryAccountCode = profile.InventoryAccountCode;
            existingProfile.CostOfGoodsSoldAccountCode = profile.CostOfGoodsSoldAccountCode;
            existingProfile.CostRatio = profile.CostRatio;

            _logger.LogInformation("Updated document accounting profile for operation {Operation}", profile.DocumentOperation);

            return await Task.FromResult(existingProfile);
        }

        /// <summary>
        /// Delete a document accounting profile
        /// </summary>
        /// <param name="documentOperation">The operation of the profile to delete</param>
        /// <param name="username">The username deleting the profile</param>
        /// <returns>True if deleted, false otherwise</returns>
        public async Task<bool> DeleteProfileAsync(string documentOperation, string username)
        {
            if (_objectDb.DocumentAccountingProfiles == null)
                return await Task.FromResult(false);

            var profileToRemove = _objectDb.DocumentAccountingProfiles
                .FirstOrDefault(p => p.DocumentOperation == documentOperation);

            if (profileToRemove == null)
                return await Task.FromResult(false);

            _objectDb.DocumentAccountingProfiles.Remove(profileToRemove);
            _logger.LogInformation("Deleted document accounting profile for operation {Operation}", documentOperation);

            return await Task.FromResult(true);
        }
    }
}
