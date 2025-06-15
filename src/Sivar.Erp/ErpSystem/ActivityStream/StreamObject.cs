namespace Sivar.Erp.ErpSystem.ActivityStream
{
    /// <summary>
    /// Standard implementation of IStreamObject
    /// </summary>
    public class StreamObject : IStreamObject
    {
        /// <summary>
        /// The type of object (e.g., "User", "Invoice", "Customer", "Product")
        /// </summary>
        public string ObjectType { get; set; }
        
        /// <summary>
        /// Unique identifier for the object
        /// </summary>
        public string ObjectKey { get; set; }
        
        /// <summary>
        /// Human-friendly name for displaying the object in the UI
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// URL or resource identifier for the object's image/icon
        /// </summary>
        public string DisplayImage { get; set; }
        
        /// <summary>
        /// Creates a new stream object
        /// </summary>
        public StreamObject() { }
        
        /// <summary>
        /// Creates a new stream object with the specified properties
        /// </summary>
        public StreamObject(string objectType, string objectKey, string displayName, string displayImage = null)
        {
            ObjectType = objectType;
            ObjectKey = objectKey;
            DisplayName = displayName;
            DisplayImage = displayImage;
        }
    }
}