using System;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Attribute to mark properties as business keys for domain entities
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BusinessKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the BusinessKeyAttribute
        /// </summary>
        public BusinessKeyAttribute()
        {
        }
    }
}