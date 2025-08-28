using System;

namespace Sivar.Erp.Modules.Taxes.TaxGroup
{
    public interface IGroupMembership

    {
        /// <summary>
        /// The entity ID that is a member of the group (either business entity or item depending on GroupType)
        /// </summary>
        string EntityId { get; set; }
        /// <summary>
        /// The tax group ID that this membership belongs to
        /// </summary>
        string GroupCode { get; set; }
        /// <summary>
        /// The type of group this membership relates to
        /// </summary>
        GroupType GroupType { get; set; }
        /// <summary>
        /// Unique identifier for the membership
        /// </summary>
        Guid ID { get; set; }
    }
}
