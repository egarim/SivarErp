using System.ComponentModel;
using System.Reflection;

namespace Sivar.Erp.Core.Demo
{
    /// <summary>
    /// Handles loading of embedded test data resources
    /// </summary>
    public static class TestDataResourceManager
    {
        private static readonly Assembly _assembly = typeof(TestDataResourceManager).Assembly;
        private const string ResourcePrefix = "Sivar.Erp.Core.Demo.TestData.ElSalvador.";
        
        /// <summary>
        /// Loads a CSV file from embedded resources
        /// </summary>
        /// <param name="fileName">The name of the CSV file to load</param>
        /// <returns>The content of the CSV file as a string</returns>
        /// <exception cref="FileNotFoundException">Thrown when the embedded resource is not found</exception>
        [Description("Loads CSV content from embedded resources")]
        public static async Task<string> LoadCsvAsync(string fileName)
        {
            string resourceName = ResourcePrefix + fileName;
            
            // Try to get the resource stream
            Stream? resourceStream = _assembly.GetManifestResourceStream(resourceName);
            
            if (resourceStream == null)
            {
                // If the file is not found with the exact name, try to find it with any case
                var availableResources = _assembly.GetManifestResourceNames();
                var matchingResource = availableResources.FirstOrDefault(r => 
                    r.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
                    
                if (matchingResource != null)
                {
                    resourceStream = _assembly.GetManifestResourceStream(matchingResource);
                }
                else
                {
                    throw new FileNotFoundException($"Resource not found: {resourceName}. Available resources: {string.Join(", ", availableResources)}");
                }
            }
            
            using var reader = new StreamReader(resourceStream!);
            return await reader.ReadToEndAsync();
        }
        
        /// <summary>
        /// Gets a list of all available test data resources
        /// </summary>
        /// <returns>A list of resource names</returns>
        public static IEnumerable<string> GetAvailableResources()
        {
            return _assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(ResourcePrefix))
                .Select(r => r.Substring(ResourcePrefix.Length));
        }
    }
}