using System.ComponentModel;
using System.Reflection;

namespace Sivar.Erp.Core.Demo
{
    /// <summary>
    /// Manages embedded test data resources
    /// </summary>
    [Description("Manages embedded test data resources")]
    public class TestDataResourceManager
    {
        /// <summary>
        /// Loads CSV content from embedded resources
        /// </summary>
        /// <param name="fileName">The name of the file to load</param>
        /// <returns>The content of the file as a string</returns>
        /// <exception cref="FileNotFoundException">Thrown when the embedded resource is not found</exception>
        [Description("Loads CSV content from embedded resources")]
        public static async Task<string> LoadCsvAsync(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Sivar.Erp.Core.Demo.TestData.ElSalvador.{fileName}";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Embedded resource not found: {resourceName}");
                
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        
        /// <summary>
        /// Lists all available CSV files in the embedded resources
        /// </summary>
        /// <returns>A collection of available CSV file names</returns>
        [Description("Lists all available CSV files")]
        public static IEnumerable<string> GetAvailableCsvFiles()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceNames()
                .Where(name => name.Contains("TestData.ElSalvador"))
                .Select(name => Path.GetFileName(name));
        }
    }
}