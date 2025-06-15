namespace Sivar.Erp.Services.Accounting.Transactions
{
    /// <summary>
    /// Status of transaction batch
    /// </summary>
    public enum BatchStatus
    {
        /// <summary>
        /// Batch is being prepared
        /// </summary>
        Draft,

        /// <summary>
        /// Batch is pending approval
        /// </summary>
        PendingApproval,

        /// <summary>
        /// Batch has been approved
        /// </summary>
        Approved,

        /// <summary>
        /// Batch has been processed
        /// </summary>
        Processed,

        /// <summary>
        /// Batch has been rejected
        /// </summary>
        Rejected
    }
}