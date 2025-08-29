using Sivar.Erp.Modules.Payments.Models;

namespace Sivar.Erp.Modules.ImportExport
{
    /// <summary>
    /// Interface for payment method import/export operations
    /// </summary>
    public interface IPaymentMethodImportExportService
    {
        /// <summary>
        /// Imports payment methods from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported payment methods and any validation errors</returns>
        Task<(IEnumerable<IPaymentMethod> ImportedPaymentMethods, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);

        /// <summary>
        /// Exports payment methods to a CSV format
        /// </summary>
        /// <param name="paymentMethods">Payment methods to export</param>
        /// <returns>CSV content as a string</returns>
        Task<string> ExportToCsvAsync(IEnumerable<IPaymentMethod> paymentMethods);
    }
}
