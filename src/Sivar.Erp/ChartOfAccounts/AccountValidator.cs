namespace Sivar.Erp.ChartOfAccounts
{
    /// <summary>
    /// Validator for account business rules
    /// </summary>
    public class AccountValidator
    {
        /// <summary>
        /// Validates that an account code follows the required format
        /// </summary>
        /// <param name="accountCode">Account code to validate</param>
        /// <param name="accountType">Type of account</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateAccountCode(string accountCode, AccountType accountType)
        {
            if (string.IsNullOrWhiteSpace(accountCode))
            {
                return false;
            }

            // Account code should be numeric
            if (!accountCode.All(char.IsDigit))
            {
                return false;
            }

            // Check that account code prefix matches account type
            char expectedPrefix = GetExpectedPrefix(accountType);
            return accountCode.Length > 0 && accountCode[0] == expectedPrefix;
        }

        /// <summary>
        /// Gets the expected first digit for an account code based on account type
        /// </summary>
        /// <param name="accountType">Type of account</param>
        /// <returns>Expected first digit</returns>
        private char GetExpectedPrefix(AccountType accountType)
        {
            // Convention: Assets start with 1, Liabilities with 2, Equity with 3, etc.
            switch (accountType)
            {
                case AccountType.Asset:
                    return '1';
                case AccountType.Liability:
                    return '2';
                case AccountType.Equity:
                    return '3';
                case AccountType.Revenue:
                    return '4';
                case AccountType.Expense:
                    return '6';
                default:
                    return '0';
            }
        }

        /// <summary>
        /// Validates the consistency between account type and financial statement line
        /// </summary>
        /// <param name="accountType">Type of account</param>
        /// <param name="balanceAndIncomeLineId">Financial statement line ID</param>
        /// <returns>True if consistent, false otherwise</returns>
        public bool ValidateFinancialStatementLine(AccountType accountType, Guid? balanceAndIncomeLineId)
        {
            // If no line is assigned, that's valid for any account type
            if (!balanceAndIncomeLineId.HasValue)
            {
                return true;
            }

            // In a real implementation, we would check if the line type matches the account type
            // This would require looking up the line type from the database
            // For this example, we'll return true
            return true;
        }

        /// <summary>
        /// Validates the complete account
        /// </summary>
        /// <param name="account">Account to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateAccount(AccountDto account)
        {
            // Basic validation
            if (!account.Validate())
            {
                return false;
            }

            // Account code validation
            if (!ValidateAccountCode(account.OfficialCode, account.AccountType))
            {
                return false;
            }

            // Financial statement line validation
            if (!ValidateFinancialStatementLine(account.AccountType, account.BalanceAndIncomeLineId))
            {
                return false;
            }

            return true;
        }
    }
}