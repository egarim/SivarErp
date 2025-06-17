using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sivar.Erp.Services.Taxes.TaxGroup
{
    /// <summary>
    /// Represents membership of an entity or item in a tax group
    /// </summary>
    public class GroupMembershipDto : INotifyPropertyChanged
    {
        private Guid _oid;
        private string _groupId;
        private string _entityId;
        private GroupType _groupType;

        /// <summary>
        /// Unique identifier for the membership
        /// </summary>
        public Guid Oid
        {
            get => _oid;
            set
            {
                if (_oid != value)
                {
                    _oid = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The tax group ID that this membership belongs to
        /// </summary>
        public string GroupId
        {
            get => _groupId;
            set
            {
                if (_groupId != value)
                {
                    _groupId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The entity ID that is a member of the group (either business entity or item depending on GroupType)
        /// </summary>
        public string EntityId
        {
            get => _entityId;
            set
            {
                if (_entityId != value)
                {
                    _entityId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The type of group this membership relates to
        /// </summary>
        public GroupType GroupType
        {
            get => _groupType;
            set
            {
                if (_groupType != value)
                {
                    _groupType = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}