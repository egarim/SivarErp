using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Demo
{
    /// <summary>
    /// Generates comprehensive sample data for demos
    /// </summary>
    public class SampleDataGenerator : ISampleDataGenerator
    {
        private readonly ILogger<SampleDataGenerator> _logger;
        private readonly TestDataImportService _testDataImportService;
        private readonly DemoOptions _options;
        
        /// <summary>
        /// Initializes a new instance of the SampleDataGenerator class
        /// </summary>
        /// <param name="logger">The logger for diagnostic information</param>
        /// <param name="testDataImportService">The service for importing test data</param>
        /// <param name="options">The demo options</param>
        public SampleDataGenerator(
            ILogger<SampleDataGenerator> logger,
            TestDataImportService testDataImportService,
            IOptions<DemoOptions> options)
        {
            _logger = logger;
            _testDataImportService = testDataImportService;
            _options = options.Value;
        }
        
        /// <inheritdoc/>
        public async Task GenerateSampleDataAsync(IRepository repository)
        {
            _logger.LogInformation("Generating sample data for dataset: {DataSet}", _options.TestDataSet);
            
            try
            {
                // Use the test data import service to import all data
                var results = await _testDataImportService.ImportAllTestDataAsync("SampleDataGenerator");
                
                // Log results
                foreach (var result in results)
                {
                    _logger.LogInformation("Import of {Type}: {Result}", result.Key, result.Value);
                }
                
                // Commit all changes
                await repository.CommitChanges();
                
                _logger.LogInformation("Sample data generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sample data");
                repository.Rollback();
                throw;
            }
        }
    }
}