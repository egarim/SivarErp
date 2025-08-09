using System.IO;
using System.Threading.Tasks;

namespace Sivar.Erp.Core.Demo.Tools
{
    /// <summary>
    /// Helper tool to copy CSV test data from the Tests project to the Demo folder as embedded resources
    /// </summary>
    public static class TestDataCopyTool
    {
        /// <summary>
        /// Copies test data files from the source directory to the target directory
        /// </summary>
        /// <param name="sourceDir">The source directory containing the test data files</param>
        /// <param name="targetDir">The target directory to copy the files to</param>
        /// <returns>The number of files copied</returns>
        public static async Task<int> CopyTestDataFilesAsync(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            }

            // Create target directory if it doesn't exist
            Directory.CreateDirectory(targetDir);

            // Get all CSV and TXT files from the source directory
            var files = Directory.GetFiles(sourceDir, "*.csv").Concat(Directory.GetFiles(sourceDir, "*.txt"));
            int count = 0;

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var targetPath = Path.Combine(targetDir, fileName);
                
                // Read the file content
                var content = await File.ReadAllTextAsync(file);
                
                // Write to the target path
                await File.WriteAllTextAsync(targetPath, content);
                count++;
            }

            return count;
        }
    }
}