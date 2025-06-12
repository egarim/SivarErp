using System;
using System.ComponentModel;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Extended property changed event args that includes additional information about property changes
    /// </summary>
    public class DocumentPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// The source of the change (e.g., DocumentDto, DocumentLineDto)
        /// </summary>
        public object Source { get; }

        /// <summary>
        /// The type of change that occurred
        /// </summary>
        public ChangeType ChangeType { get; }

        /// <summary>
        /// The old value of the property (if available)
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// The new value of the property (if available)
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// Path to the changed property for nested objects (e.g. "BusinessEntity.Name")
        /// </summary>
        public string PropertyPath { get; }

        /// <summary>
        /// Constructor for property change with old and new values
        /// </summary>
        public DocumentPropertyChangedEventArgs(
            string propertyName, 
            object source, 
            ChangeType changeType,
            object oldValue = null,
            object newValue = null,
            string propertyPath = null) : base(propertyName)
        {
            Source = source;
            ChangeType = changeType;
            OldValue = oldValue;
            NewValue = newValue;
            PropertyPath = propertyPath ?? propertyName;
        }
    }

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