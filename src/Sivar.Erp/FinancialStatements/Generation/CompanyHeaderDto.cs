using System;

namespace Sivar.Erp.FinancialStatements.Generation
{
    /// <summary>
    /// DTO for company header information in statements
    /// </summary>
    public class CompanyHeaderDto
    {
        /// <summary>
        /// Company name
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Company address
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Company registration number
        /// </summary>
        public string RegistrationNumber { get; set; } = string.Empty;

        /// <summary>
        /// VAT number
        /// </summary>
        public string VatNumber { get; set; } = string.Empty;

        /// <summary>
        /// Currency used in statements
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Preparation date
        /// </summary>
        public DateOnly PreparedDate { get; set; }

        /// <summary>
        /// Who prepared the statement
        /// </summary>
        public string PreparedBy { get; set; } = string.Empty;
    }
    /// <summary>
    /// Builder for creating balance sheets
    /// </summary>
}