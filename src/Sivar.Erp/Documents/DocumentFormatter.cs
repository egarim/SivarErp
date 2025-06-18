using System;
using System.Linq;
using System.Text;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Documents
{
    public static class DocumentFormatter
    {
        /// <summary>
        /// Formats a document with all its lines, line totals, and document totals as a string
        /// </summary>
        /// <param name="document">The document to format</param>
        /// <returns>A formatted string representation of the document</returns>
        public static string FormatDocument(DocumentDto document)
        {
            if (document == null)
                return "Document is null";

            var sb = new StringBuilder();

            // Document Header
            sb.AppendLine("========================================");
            sb.AppendLine("DOCUMENT DETAILS");
            sb.AppendLine("========================================");
            sb.AppendLine($"Document Number: {document.DocumentNumber ?? "N/A"}");
            sb.AppendLine($"Document Type: {document.DocumentType?.Name ?? "N/A"} ({document.DocumentType?.Code ?? "N/A"})");
            sb.AppendLine($"Document Operation: {document.DocumentType?.DocumentOperation.ToString() ?? "N/A"}");
            sb.AppendLine($"Date: {document.Date}");
            sb.AppendLine($"Time: {document.Time}");
            sb.AppendLine($"Business Entity: {document.BusinessEntity?.Name ?? "N/A"} ({document.BusinessEntity?.Code ?? "N/A"})");
            sb.AppendLine();

            // Document Lines
            sb.AppendLine("========================================");
            sb.AppendLine("DOCUMENT LINES");
            sb.AppendLine("========================================");

            if (document.Lines == null || !document.Lines.Any())
            {
                sb.AppendLine("No lines in document");
            }
            else
            {
                int lineNumber = 1;
                foreach (var line in document.Lines.OfType<LineDto>())
                {
                    sb.AppendLine($"Line #{lineNumber}:");
                    sb.AppendLine($"  Item: {line.Item?.Description ?? "N/A"} ({line.Item?.Code ?? "N/A"})");
                    sb.AppendLine($"  Quantity: {line.Quantity:F2}");
                    sb.AppendLine($"  Unit Price: ${line.UnitPrice:F2}");
                    sb.AppendLine($"  Line Amount: ${line.Amount:F2}");

                    // Line Totals
                    if (line.LineTotals != null && line.LineTotals.Any())
                    {
                        sb.AppendLine($"  Line Totals ({line.LineTotals.Count}):");
                        foreach (var lineTotal in line.LineTotals)
                        {
                            sb.AppendLine($"    - {lineTotal.Concept}: ${lineTotal.Total:F2}");
                            if (!string.IsNullOrEmpty(lineTotal.DebitAccountCode))
                                sb.AppendLine($"      Debit Account: {lineTotal.DebitAccountCode}");
                            if (!string.IsNullOrEmpty(lineTotal.CreditAccountCode))
                                sb.AppendLine($"      Credit Account: {lineTotal.CreditAccountCode}");
                            sb.AppendLine($"      Include in Transaction: {lineTotal.IncludeInTransaction}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("  No line totals");
                    }

                    // Line Taxes
                    if (line.Taxes != null && line.Taxes.Any())
                    {
                        sb.AppendLine($"  Line Taxes ({line.Taxes.Count}):");
                        foreach (var tax in line.Taxes)
                        {
                            sb.AppendLine($"    - {tax.Name} ({tax.Code}): {tax.TaxType}");
                            sb.AppendLine($"      Rate: {tax.Percentage}%");
                            sb.AppendLine($"      Application Level: {tax.ApplicationLevel}");
                            sb.AppendLine($"      Is Enabled: {tax.IsEnabled}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("  No line taxes");
                    }

                    sb.AppendLine();
                    lineNumber++;
                }
            }

            // Document Totals
            sb.AppendLine("========================================");
            sb.AppendLine("DOCUMENT TOTALS");
            sb.AppendLine("========================================");

            if (document.DocumentTotals == null || !document.DocumentTotals.Any())
            {
                sb.AppendLine("No document totals");
            }
            else
            {
                sb.AppendLine($"Total Count: {document.DocumentTotals.Count}");
                decimal grandTotal = 0;

                foreach (var total in document.DocumentTotals)
                {
                    sb.AppendLine($"- {total.Concept}: ${total.Total:F2}");

                    if (!string.IsNullOrEmpty(total.DebitAccountCode))
                        sb.AppendLine($"  Debit Account: {total.DebitAccountCode}");
                    if (!string.IsNullOrEmpty(total.CreditAccountCode))
                        sb.AppendLine($"  Credit Account: {total.CreditAccountCode}");

                    sb.AppendLine($"  Include in Transaction: {total.IncludeInTransaction}");

                    // Track totals that look like taxes
                    if (total.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine($"  [TAX TOTAL]");
                    }

                    grandTotal += total.Total;
                }

                sb.AppendLine();
                sb.AppendLine($"Sum of All Document Totals: ${grandTotal:F2}");
            }

            // Summary Statistics
            sb.AppendLine();
            sb.AppendLine("========================================");
            sb.AppendLine("SUMMARY");
            sb.AppendLine("========================================");

            // Calculate subtotal (sum of line amounts)
            decimal subtotal = document.Lines?.OfType<LineDto>().Sum(l => l.Amount) ?? 0;
            sb.AppendLine($"Lines Subtotal: ${subtotal:F2}");

            // Calculate tax totals
            decimal lineTaxTotal = 0;
            decimal documentTaxTotal = 0;

            if (document.Lines != null)
            {
                foreach (var line in document.Lines.OfType<LineDto>())
                {
                    if (line.LineTotals != null)
                    {
                        lineTaxTotal += line.LineTotals
                            .Where(t => t.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                            .Sum(t => t.Total);
                    }
                }
            }

            if (document.DocumentTotals != null)
            {
                documentTaxTotal = document.DocumentTotals
                    .Where(t => t.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Total);
            }

            sb.AppendLine($"Total Line Taxes: ${lineTaxTotal:F2}");
            sb.AppendLine($"Total Document Taxes: ${documentTaxTotal:F2}");
            sb.AppendLine($"Total All Taxes: ${(lineTaxTotal + documentTaxTotal):F2}");

            // Find accounts receivable total if present
            var arTotal = document.DocumentTotals?.FirstOrDefault(t =>
                t.Concept.Contains("Accounts Receivable", StringComparison.OrdinalIgnoreCase));
            if (arTotal != null)
            {
                sb.AppendLine($"Accounts Receivable Total: ${arTotal.Total:F2}");
            }

            sb.AppendLine("========================================");

            return sb.ToString();
        }

        /// <summary>
        /// Formats a document in a compact format for quick viewing
        /// </summary>
        public static string FormatDocumentCompact(DocumentDto document)
        {
            if (document == null)
                return "Document is null";

            var sb = new StringBuilder();

            sb.AppendLine($"Document: {document.DocumentNumber} ({document.DocumentType?.Code}) - {document.Date}");
            sb.AppendLine($"Entity: {document.BusinessEntity?.Name}");

            // Lines summary
            sb.AppendLine("Lines:");
            foreach (var line in document.Lines?.OfType<LineDto>() ?? Enumerable.Empty<LineDto>())
            {
                sb.AppendLine($"  {line.Item?.Code}: {line.Quantity} x ${line.UnitPrice:F2} = ${line.Amount:F2}");

                // Show line taxes
                var lineTaxes = line.LineTotals?.Where(t => t.Concept.StartsWith("Tax:")) ?? Enumerable.Empty<TotalDto>();
                foreach (var tax in lineTaxes)
                {
                    sb.AppendLine($"    + {tax.Concept}: ${tax.Total:F2}");
                }
            }

            // Document totals summary
            sb.AppendLine("Document Totals:");
            foreach (var total in document.DocumentTotals ?? Enumerable.Empty<ITotal>())
            {
                sb.AppendLine($"  {total.Concept}: ${total.Total:F2}");
            }

            return sb.ToString();
        }
    }
}