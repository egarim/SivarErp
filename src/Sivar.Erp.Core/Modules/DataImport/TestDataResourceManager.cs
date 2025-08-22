using System.ComponentModel;
using System.Reflection;

namespace Sivar.Erp.Core.Modules.DataImport
{
    /// <summary>
    /// Manager for embedded test data resources
    /// </summary>
    [Description("Manager for embedded test data resources")]
    public static class TestDataResourceManager
    {
        /// <summary>
        /// Gets all available test data resources
        /// </summary>
        /// <returns>Collection of available resource names</returns>
        [Description("Gets all available test data resources")]
        public static List<string> GetAvailableResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(name => name.Contains("TestData") && name.EndsWith(".csv"))
                .ToList();

            // Also check for common test data file patterns
            var commonPatterns = new[]
            {
                "Accounts.csv",
                "Taxes.csv", 
                "BusinessEntities.csv",
                "Items.csv",
                "DocumentTypes.csv",
                "ElSalvador.csv"
            };

            foreach (var pattern in commonPatterns)
            {
                var matchingResources = assembly.GetManifestResourceNames()
                    .Where(name => name.EndsWith(pattern))
                    .ToList();
                    
                resourceNames.AddRange(matchingResources);
            }

            return resourceNames.Distinct().ToList();
        }

        /// <summary>
        /// Loads CSV content from an embedded resource
        /// </summary>
        /// <param name="resourceName">Name of the resource to load</param>
        /// <returns>CSV content as string</returns>
        [Description("Loads CSV content from an embedded resource")]
        public static async Task<string> LoadCsvAsync(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // Try exact match first
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }

            // Try to find resource by pattern matching
            var allResources = assembly.GetManifestResourceNames();
            var matchingResource = allResources.FirstOrDefault(r => 
                r.EndsWith(resourceName) || r.Contains(resourceName));

            if (matchingResource != null)
            {
                using var matchingStream = assembly.GetManifestResourceStream(matchingResource);
                if (matchingStream != null)
                {
                    using var reader = new StreamReader(matchingStream);
                    return await reader.ReadToEndAsync();
                }
            }

            // If no embedded resource found, return a minimal CSV structure
            return GenerateMinimalCsvForResource(resourceName);
        }

        /// <summary>
        /// Generates minimal CSV content when no embedded resource is found
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <returns>Minimal CSV content</returns>
        private static string GenerateMinimalCsvForResource(string resourceName)
        {
            // Return minimal CSV based on the resource type
            if (resourceName.Contains("Account", StringComparison.OrdinalIgnoreCase))
            {
                return "Code,Name,AccountType\nCASH,Cash,Asset\nREVENUE,Revenue,Revenue";
            }
            else if (resourceName.Contains("Tax", StringComparison.OrdinalIgnoreCase))
            {
                return "Code,Name,Rate,TaxType\nVAT,Value Added Tax,13.0,Percentage";
            }
            else if (resourceName.Contains("BusinessEntity", StringComparison.OrdinalIgnoreCase))
            {
                return "Code,Name,EntityType\nCUST001,Customer 1,Customer\nSUPP001,Supplier 1,Supplier";
            }
            else if (resourceName.Contains("Item", StringComparison.OrdinalIgnoreCase))
            {
                return "Code,Name,UnitPrice\nITEM001,Product 1,10.00\nITEM002,Service 1,25.00";
            }
            else if (resourceName.Contains("DocumentType", StringComparison.OrdinalIgnoreCase))
            {
                return "Code,Name,DocumentOperation\nINV,Invoice,SalesInvoice\nPUR,Purchase,PurchaseInvoice";
            }
            
            // Default minimal CSV
            return "Id,Name\n1,Sample Data";
        }
    }
}