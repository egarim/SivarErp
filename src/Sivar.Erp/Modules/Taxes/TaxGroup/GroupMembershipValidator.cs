using System;

namespace Sivar.Erp.Services.Taxes.TaxGroup
{
    /// <summary>
    /// Validator for group membership entities
    /// </summary>
    public class GroupMembershipValidator
    {
        /// <summary>
        /// Validates a group membership entity
        /// </summary>
        /// <param name="membership">Membership to validate</param>
        /// <returns>True if the membership is valid, false otherwise</returns>
        public bool ValidateMembership(GroupMembershipDto membership)
        {
            if (membership == null)
            {
                return false;
            }

            // Check required fields
            if (string.IsNullOrWhiteSpace(membership.GroupId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(membership.EntityId))
            {
                return false;
            }

            // Ensure GroupType is a valid enum value
            if (!Enum.IsDefined(typeof(GroupType), membership.GroupType))
            {
                return false;
            }

            return true;
        }
    }
}