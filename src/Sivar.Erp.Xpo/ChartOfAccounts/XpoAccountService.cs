using DevExpress.Xpo;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Xpo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Xpo.ChartOfAccounts
{
    /// <summary>
    /// Service for managing accounts in the chart of accounts
    /// </summary>
    public class XpoAccountService
    {
        private readonly IAuditService _auditService;
        private readonly IArchiveService _archiveService;

        /// <summary>
        /// Initializes a new instance of the account service
        /// </summary>
        /// <param name="auditService">Audit service</param>
        /// <param name="archiveService">Archive service</param>
        public XpoAccountService(IAuditService auditService, IArchiveService archiveService)
        {
            _auditService = auditService;
            _archiveService = archiveService;
        }

        /// <summary>
        /// Creates a new account
        /// </summary>
        /// <param name="account">Account to create</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Created account</returns>
        public async Task<IAccount> CreateAccountAsync(IAccount account, string userName)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            XpoAccount xpoAccount;

            if (account is XpoAccount existingXpoAccount)
            {
                // If it's already an XPO object, we need to recreate it in this UnitOfWork
                xpoAccount = new XpoAccount(uow);
                // Copy properties
                xpoAccount.BalanceAndIncomeLineId = existingXpoAccount.BalanceAndIncomeLineId;
                xpoAccount.AccountName = existingXpoAccount.AccountName;
                xpoAccount.AccountType = existingXpoAccount.AccountType;
                xpoAccount.OfficialCode = existingXpoAccount.OfficialCode;
                xpoAccount.IsArchived = existingXpoAccount.IsArchived;
            }
            else
            {
                // Create new XPO account from the interface
                xpoAccount = new XpoAccount(uow)
                {
                    BalanceAndIncomeLineId = account.BalanceAndIncomeLineId,
                    AccountName = account.AccountName,
                    AccountType = account.AccountType,
                    OfficialCode = account.OfficialCode,
                    IsArchived = account.IsArchived
                };
            }

            // Validate the account
            if (!xpoAccount.Validate())
            {
                throw new Exception("Account validation failed");
            }

            // Generate new ID if not provided
            if (xpoAccount.Id == Guid.Empty)
            {
                xpoAccount.Id = Guid.NewGuid();
            }

            // Set audit information
            _auditService.SetCreationAudit(xpoAccount, userName);

            // Save changes
            await uow.CommitChangesAsync();

            return xpoAccount;
        }

        /// <summary>
        /// Updates an existing account
        /// </summary>
        /// <param name="account">Account with updated values</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Updated account</returns>
        public async Task<IAccount> UpdateAccountAsync(IAccount account, string userName)
        {
            if (account.Id == Guid.Empty)
            {
                throw new ArgumentException("Account ID must be provided for update", nameof(account));
            }

            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Find existing account
            var xpoAccount = await uow.GetObjectByKeyAsync<XpoAccount>(account.Id);

            if (xpoAccount == null)
            {
                throw new Exception($"Account with ID {account.Id} not found");
            }

            // Update properties
            xpoAccount.BalanceAndIncomeLineId = account.BalanceAndIncomeLineId;
            xpoAccount.AccountName = account.AccountName;
            xpoAccount.AccountType = account.AccountType;
            xpoAccount.OfficialCode = account.OfficialCode;
            xpoAccount.IsArchived = account.IsArchived;

            // Validate the account
            if (!xpoAccount.Validate())
            {
                throw new Exception("Account validation failed");
            }

            // Set audit information for update
            _auditService.SetUpdateAudit(xpoAccount, userName);

            // Save changes
            await uow.CommitChangesAsync();

            return xpoAccount;
        }

        /// <summary>
        /// Archives an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ArchiveAccountAsync(Guid accountId, string userName)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Find existing account
            var account = await uow.GetObjectByKeyAsync<XpoAccount>(accountId);

            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found");
            }

            // Archive the account
            bool result = _archiveService.Archive(account);

            // Set audit information for update
            _auditService.SetUpdateAudit(account, userName);

            // Save changes
            await uow.CommitChangesAsync();

            return result;
        }

        /// <summary>
        /// Restores a previously archived account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>True if successful</returns>
        public async Task<bool> RestoreAccountAsync(Guid accountId, string userName)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Find existing account
            var account = await uow.GetObjectByKeyAsync<XpoAccount>(accountId);

            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found");
            }

            // Restore the account
            bool result = _archiveService.Restore(account);

            // Set audit information for update
            _auditService.SetUpdateAudit(account, userName);

            // Save changes
            await uow.CommitChangesAsync();

            return result;
        }

        /// <summary>
        /// Retrieves an account by ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Account if found, null otherwise</returns>
        public async Task<IAccount?> GetAccountByIdAsync(Guid accountId)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();
            return await uow.GetObjectByKeyAsync<XpoAccount>(accountId);
        }

        /// <summary>
        /// Retrieves all accounts
        /// </summary>
        /// <param name="includeArchived">Whether to include archived accounts</param>
        /// <returns>Collection of accounts</returns>
        public async Task<IEnumerable<IAccount>> GetAllAccountsAsync(bool includeArchived = false)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            var query = uow.Query<XpoAccount>();

            if (!includeArchived)
            {
                //TODO fix
               // query = query.Where(a => !a.IsArchived);
            }

            return await Task.Run(() => query.OrderBy(a => a.OfficialCode).ToList());
        }

        /// <summary>
        /// Retrieves accounts by type
        /// </summary>
        /// <param name="accountType">Account type</param>
        /// <param name="includeArchived">Whether to include archived accounts</param>
        /// <returns>Collection of accounts</returns>
        public async Task<IEnumerable<IAccount>> GetAccountsByTypeAsync(
            AccountType accountType, bool includeArchived = false)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            var query = uow.Query<XpoAccount>().Where(a => a.AccountType == accountType);

            if (!includeArchived)
            {
                query = query.Where(a => !a.IsArchived);
            }

            return await Task.Run(() => query.OrderBy(a => a.OfficialCode).ToList());
        }

        /// <summary>
        /// Checks if an account with the given official code exists
        /// </summary>
        /// <param name="officialCode">Official code to check</param>
        /// <param name="excludeAccountId">Optional account ID to exclude (for updates)</param>
        /// <returns>True if exists, false otherwise</returns>
        public async Task<bool> OfficialCodeExistsAsync(string officialCode, Guid? excludeAccountId = null)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            var query = uow.Query<XpoAccount>().Where(a => a.OfficialCode == officialCode);

            if (excludeAccountId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAccountId.Value);
            }

            return await Task.Run(() => query.Any());
        }
    }
}