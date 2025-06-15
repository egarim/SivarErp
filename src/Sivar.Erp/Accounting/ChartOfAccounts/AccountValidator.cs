namespace Sivar.Erp.Accounting.ChartOfAccounts
{
    /// <summary>
    /// Validator for account business rules
    /// </summary>
    public class AccountValidator
    {
        private readonly Dictionary<AccountType, char> _accountTypePrefixes;

        /// <summary>
        /// Initializes a new instance of AccountValidator with default prefix mappings
        /// </summary>
        public AccountValidator()
        {
            _accountTypePrefixes = GetDefaultAccountTypePrefixes();
        }

        /// <summary>
        /// Initializes a new instance of AccountValidator with custom prefix mappings
        /// </summary>
        /// <param name="accountTypePrefixes">Dictionary mapping account types to their expected starting characters</param>
        public AccountValidator(Dictionary<AccountType, char> accountTypePrefixes)
        {
            _accountTypePrefixes = accountTypePrefixes ?? throw new ArgumentNullException(nameof(accountTypePrefixes));
        }

        /// <summary>
        /// Gets the default account type prefix mappings
        /// </summary>
        /// <returns>Dictionary with default mappings</returns>
        public static Dictionary<AccountType, char> GetDefaultAccountTypePrefixes()
        {
            return new Dictionary<AccountType, char>
            {
                { AccountType.Asset, '1' },
                { AccountType.Liability, '2' },
                { AccountType.Equity, '3' },
                { AccountType.Revenue, '4' },
                { AccountType.Expense, '6' },
                { AccountType.Settlement, '0' }
            };
        }

        /// <summary>
        /// Gets an account type prefix mapping suitable for El Salvador chart of accounts
        /// where expenses start with '4' and revenues start with '5'
        /// </summary>
        /// <returns>Dictionary with El Salvador mappings</returns>
        public static Dictionary<AccountType, char> GetElSalvadorAccountTypePrefixes()
        {
            return new Dictionary<AccountType, char>
            {
                { AccountType.Asset, '1' },
                { AccountType.Liability, '2' },
                { AccountType.Equity, '3' },
                { AccountType.Expense, '4' },  // In El Salvador, expenses start with 4
                { AccountType.Revenue, '5' },   // In El Salvador, revenues start with 5
                { AccountType.Settlement, '6' } // Settlement accounts start with 6 in El Salvador CSV
            };
        }

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
            if (_accountTypePrefixes.TryGetValue(accountType, out char expectedPrefix))
            {
                return accountCode.Length > 0 && accountCode[0] == expectedPrefix;
            }

            // If account type is not in our mapping, allow any prefix
            return true;
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