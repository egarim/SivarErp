namespace Sivar.Erp.Core.Demo
{
    /// <summary>
    /// Interface for generating sample data for demos
    /// </summary>
    public interface ISampleDataGenerator
    {
        /// <summary>
        /// Generates sample data in the specified repository
        /// </summary>
        /// <param name="repository">The repository to populate with sample data</param>
        Task GenerateSampleDataAsync(Core.IRepository repository);
    }
}