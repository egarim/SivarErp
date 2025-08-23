using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents a change between document versions
    /// </summary>
    public class VersionChange
    {
        /// <summary>
        /// Property path that changed
        /// </summary>
        [Description("Property path that changed")]
        public string PropertyPath { get; set; } = string.Empty;

        /// <summary>
        /// Type of change
        /// </summary>
        [Description("Type of change")]
        public VersionChangeType ChangeType { get; set; }

        /// <summary>
        /// Old value
        /// </summary>
        [Description("Old value")]
        public object? OldValue { get; set; }

        /// <summary>
        /// New value
        /// </summary>
        [Description("New value")]
        public object? NewValue { get; set; }
    }

    /// <summary>
    /// Type of version change
    /// </summary>
    public enum VersionChangeType
    {
        /// <summary>
        /// Property was added
        /// </summary>
        [Description("Property was added")]
        Added = 1,

        /// <summary>
        /// Property was modified
        /// </summary>
        [Description("Property was modified")]
        Modified = 2,

        /// <summary>
        /// Property was deleted
        /// </summary>
        [Description("Property was deleted")]
        Deleted = 3
    }
}
