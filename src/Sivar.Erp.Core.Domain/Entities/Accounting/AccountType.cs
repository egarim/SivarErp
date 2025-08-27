namespace Sivar.Erp.Core.Domain.Entities.Accounting;

/// <summary>
/// Account types for the chart of accounts following standard accounting classification
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Assets - Resources owned by the company
    /// </summary>
    Asset = 1,

    /// <summary>
    /// Liabilities - Debts and obligations
    /// </summary>
    Liability = 2,

    /// <summary>
    /// Equity - Owner's equity in the business
    /// </summary>
    Equity = 3,

    /// <summary>
    /// Revenue - Income from business operations
    /// </summary>
    Revenue = 4,

    /// <summary>
    /// Expenses - Costs of doing business
    /// </summary>
    Expense = 5
}

/// <summary>
/// Sub-categories for more detailed account classification
/// </summary>
public enum AccountSubType
{
    // Asset Sub-types
    CurrentAsset = 11,
    FixedAsset = 12,
    IntangibleAsset = 13,
    
    // Liability Sub-types
    CurrentLiability = 21,
    LongTermLiability = 22,
    
    // Equity Sub-types
    OwnerEquity = 31,
    RetainedEarnings = 32,
    
    // Revenue Sub-types
    OperatingRevenue = 41,
    NonOperatingRevenue = 42,
    
    // Expense Sub-types
    OperatingExpense = 51,
    NonOperatingExpense = 52,
    CostOfGoodsSold = 53
}
