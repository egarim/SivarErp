using System;

namespace Sivar.Erp.ErpSystem.ActivityStream
{
    /// <summary>
    /// Interface representing an object in the activity stream
    /// </summary>
    public interface IStreamObject
    {
        /// <summary>
        /// The type of object (e.g., "User", "Invoice", "Customer", "Product")
        /// </summary>
        string ObjectType { get; set; }
        
        /// <summary>
        /// Unique identifier for the object (could be a GUID, integer, or string ID)
        /// </summary>
        string ObjectKey { get; set; }
        
        /// <summary>
        /// Human-friendly name for displaying the object in the UI
        /// </summary>
        string DisplayName { get; set; }
        
        /// <summary>
        /// URL or resource identifier for the object's image/icon
        /// </summary>
        string DisplayImage { get; set; }
    }
}