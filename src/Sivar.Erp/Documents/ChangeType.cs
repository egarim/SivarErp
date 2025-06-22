using System;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Defines types of changes that can occur in document objects
    /// </summary>
    public enum ChangeType
    {
        /// <summary>Property value changed</summary>
        PropertyChanged,

        /// <summary>Item added to collection</summary>
        CollectionItemAdded,

        /// <summary>Item removed from collection</summary>
        CollectionItemRemoved,

        /// <summary>Collection replaced entirely</summary>
        CollectionReplaced,

        /// <summary>Property changed in nested object</summary>
        NestedPropertyChanged
    }
}