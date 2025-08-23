using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Result of comparing two document versions
    /// </summary>
    public class DocumentVersionComparison
    {
        /// <summary>
        /// Source version number
        /// </summary>
        [Description("Source version number")]
        public int FromVersion { get; set; }

        /// <summary>
        /// Target version number
        /// </summary>
        [Description("Target version number")]
        public int ToVersion { get; set; }

        /// <summary>
        /// List of changes between versions
        /// </summary>
        [Description("List of changes between versions")]
        public List<VersionChange> Changes { get; set; } = new List<VersionChange>();

        /// <summary>
        /// Indicates if the versions are identical
        /// </summary>
        [Description("Indicates if the versions are identical")]
        public bool AreIdentical => !Changes.Any();
    }
}
