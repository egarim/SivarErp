namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Enumeration of document types
    /// </summary>
    public enum DocumentType
    {
        /// <summary>
        /// Balance transfer statement document
        /// </summary>
        BalanceTransfer = 1,

        /// <summary>
        /// Closing entry document
        /// </summary>
        ClosingEntry = 2,

        /// <summary>
        /// Accounting note document
        /// </summary>
        AccountingNote = 3,

        /// <summary>
        /// Miscellaneous document
        /// </summary>
        Miscellaneous = 4,

        /// <summary>
        /// Fixed assets operational statement
        /// </summary>
        FixedAssetsOperational = 5,

        /// <summary>
        /// Fixed assets non-operational statement
        /// </summary>
        FixedAssetsNonOperational = 6,

        /// <summary>
        /// Fixed assets depreciation document
        /// </summary>
        FixedAssetsDepreciation = 7,

        /// <summary>
        /// Fixed assets impairment document
        /// </summary>
        FixedAssetsImpairment = 8,

        /// <summary>
        /// Fixed assets impairment reversal document
        /// </summary>
        FixedAssetsImpairmentReversal = 9,

        /// <summary>
        /// Fixed assets revaluation document
        /// </summary>
        FixedAssetsRevaluation = 10,

        /// <summary>
        /// Fixed assets investment revaluation document
        /// </summary>
        FixedAssetsInvestmentRevaluation = 11,

        /// <summary>
        /// Fixed assets requalification document
        /// </summary>
        FixedAssetsRequalification = 12,

        /// <summary>
        /// Fixed assets requalification reversal document
        /// </summary>
        FixedAssetsRequalificationReversal = 13,

        /// <summary>
        /// Fixed assets discard document
        /// </summary>
        FixedAssetsDiscard = 14,

        /// <summary>
        /// Fixed assets split document
        /// </summary>
        FixedAssetsSplit = 15,

        /// <summary>
        /// Fixed assets consolidation document
        /// </summary>
        FixedAssetsConsolidation = 16,

        /// <summary>
        /// Fixed assets accounts change document
        /// </summary>
        FixedAssetsAccountsChange = 17
    }
}