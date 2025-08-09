using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Documents;
using Sivar.Erp.Xaf.Module.BusinessObjects.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Xaf.Module.Services.Documents
{
    /// <summary>
    /// XAF-aware implementation of IDocumentAccountingProfileService using object space pattern
    /// </summary>
    public class XafDocumentAccountingProfileService : IDocumentAccountingProfileService
    {
        private readonly IObjectSpaceProvider _objectSpaceProvider;
        private readonly ILogger<XafDocumentAccountingProfileService> _logger;

        /// <summary>
        /// Initializes a new instance of the XafDocumentAccountingProfileService class
        /// </summary>
        /// <param name="objectSpaceProvider">XAF object space provider</param>
        /// <param name="logger">Logger</param>
        public XafDocumentAccountingProfileService(
            IObjectSpaceProvider objectSpaceProvider,
            ILogger<XafDocumentAccountingProfileService> logger)
        {
            _objectSpaceProvider = objectSpaceProvider ?? throw new ArgumentNullException(nameof(objectSpaceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get document accounting profile by operation
        /// </summary>
        /// <param name="documentOperation">The document operation</param>
        /// <returns>The document accounting profile</returns>
        public async Task<DocumentAccountingProfileDto> GetProfileByOperationAsync(string documentOperation)
        {
            try
            {
                using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
                var profile = objectSpace.FindObject<DocumentAccountingProfile>(
                    CriteriaOperator.Parse("DocumentOperation == ?", documentOperation?.ToUpperInvariant()));

                if (profile == null)
                    return null;

                // Convert XAF object to DTO
                var dto = new DocumentAccountingProfileDto
                {
                    Oid = profile.Oid,
                    DocumentOperation = profile.DocumentOperation,
                    SalesAccountCode = profile.SalesAccountCode,
                    AccountsReceivableCode = profile.AccountsReceivableCode,
                    CostOfGoodsSoldAccountCode = profile.CostOfGoodsSoldAccountCode,
                    InventoryAccountCode = profile.InventoryAccountCode,
                    CostRatio = profile.CostRatio,
                    CreatedBy = profile.CreatedBy,
                    CreatedDate = new DateTimeOffset(profile.CreatedOn)
                };

                return await Task.FromResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for operation {Operation}", documentOperation);
                return null;
            }
        }

        /// <summary>
        /// Get all document accounting profiles
        /// </summary>
        /// <returns>List of all document accounting profiles</returns>
        public async Task<List<DocumentAccountingProfileDto>> GetAllProfilesAsync()
        {
            try
            {
                using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
                var profiles = objectSpace.GetObjects<DocumentAccountingProfile>();

                var dtos = profiles.Select(profile => new DocumentAccountingProfileDto
                {
                    Oid = profile.Oid,
                    DocumentOperation = profile.DocumentOperation,
                    SalesAccountCode = profile.SalesAccountCode,
                    AccountsReceivableCode = profile.AccountsReceivableCode,
                    CostOfGoodsSoldAccountCode = profile.CostOfGoodsSoldAccountCode,
                    InventoryAccountCode = profile.InventoryAccountCode,
                    CostRatio = profile.CostRatio,
                    CreatedBy = profile.CreatedBy,
                    CreatedDate = new DateTimeOffset(profile.CreatedOn)
                }).ToList();

                return await Task.FromResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all profiles");
                return new List<DocumentAccountingProfileDto>();
            }
        }

        /// <summary>
        /// Create a new document accounting profile
        /// </summary>
        /// <param name="profile">The profile to create</param>
        /// <param name="username">The username creating the profile</param>
        /// <returns>The created profile</returns>
        public async Task<DocumentAccountingProfileDto> CreateProfileAsync(DocumentAccountingProfileDto profile, string username)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username must be specified", nameof(username));

            try
            {
                using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

                // Check if profile already exists
                var existingProfile = objectSpace.FindObject<DocumentAccountingProfile>(
                    CriteriaOperator.Parse("DocumentOperation == ?", profile.DocumentOperation?.ToUpperInvariant()));

                if (existingProfile != null)
                {
                    _logger.LogWarning("Profile already exists for operation {Operation}", profile.DocumentOperation);
                    return await UpdateProfileAsync(profile, username);
                }

                // Create new XAF profile
                var xafProfile = objectSpace.CreateObject<DocumentAccountingProfile>();
                xafProfile.DocumentOperation = profile.DocumentOperation;
                xafProfile.SalesAccountCode = profile.SalesAccountCode;
                xafProfile.AccountsReceivableCode = profile.AccountsReceivableCode;
                xafProfile.CostOfGoodsSoldAccountCode = profile.CostOfGoodsSoldAccountCode;
                xafProfile.InventoryAccountCode = profile.InventoryAccountCode;
                xafProfile.CostRatio = profile.CostRatio;

                objectSpace.CommitChanges();

                // Update DTO with generated values
                profile.Oid = xafProfile.Oid;
                profile.CreatedBy = xafProfile.CreatedBy;
                profile.CreatedDate = new DateTimeOffset(xafProfile.CreatedOn);

                _logger.LogInformation("Created profile for operation {Operation}", profile.DocumentOperation);
                return await Task.FromResult(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating profile for operation {Operation}", profile.DocumentOperation);
                throw;
            }
        }

        /// <summary>
        /// Update an existing document accounting profile
        /// </summary>
        /// <param name="profile">The profile with updated values</param>
        /// <param name="username">The username updating the profile</param>
        /// <returns>The updated profile</returns>
        public async Task<DocumentAccountingProfileDto> UpdateProfileAsync(DocumentAccountingProfileDto profile, string username)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username must be specified", nameof(username));

            try
            {
                using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

                var existingProfile = objectSpace.FindObject<DocumentAccountingProfile>(
                    CriteriaOperator.Parse("DocumentOperation == ?", profile.DocumentOperation?.ToUpperInvariant()));

                if (existingProfile == null)
                {
                    return await CreateProfileAsync(profile, username);
                }

                // Update properties
                existingProfile.SalesAccountCode = profile.SalesAccountCode;
                existingProfile.AccountsReceivableCode = profile.AccountsReceivableCode;
                existingProfile.CostOfGoodsSoldAccountCode = profile.CostOfGoodsSoldAccountCode;
                existingProfile.InventoryAccountCode = profile.InventoryAccountCode;
                existingProfile.CostRatio = profile.CostRatio;

                objectSpace.CommitChanges();

                // Update DTO with current values
                profile.Oid = existingProfile.Oid;
                profile.CreatedBy = existingProfile.CreatedBy;
                profile.CreatedDate = new DateTimeOffset(existingProfile.CreatedOn);

                _logger.LogInformation("Updated profile for operation {Operation}", profile.DocumentOperation);
                return await Task.FromResult(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for operation {Operation}", profile.DocumentOperation);
                throw;
            }
        }

        /// <summary>
        /// Delete a document accounting profile
        /// </summary>
        /// <param name="documentOperation">The operation of the profile to delete</param>
        /// <param name="username">The username deleting the profile</param>
        /// <returns>True if deleted, false otherwise</returns>
        public async Task<bool> DeleteProfileAsync(string documentOperation, string username)
        {
            if (string.IsNullOrWhiteSpace(documentOperation)) throw new ArgumentException("Document operation must be specified", nameof(documentOperation));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username must be specified", nameof(username));

            try
            {
                using var objectSpace = _objectSpaceProvider.CreateObjectSpace();

                var profileToDelete = objectSpace.FindObject<DocumentAccountingProfile>(
                    CriteriaOperator.Parse("DocumentOperation == ?", documentOperation.ToUpperInvariant()));

                if (profileToDelete == null)
                {
                    _logger.LogWarning("Profile not found for operation {Operation}", documentOperation);
                    return await Task.FromResult(false);
                }

                objectSpace.Delete(profileToDelete);
                objectSpace.CommitChanges();

                _logger.LogInformation("Deleted profile for operation {Operation} by user {User}", documentOperation, username);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile for operation {Operation}", documentOperation);
                return await Task.FromResult(false);
            }
        }
    }
}