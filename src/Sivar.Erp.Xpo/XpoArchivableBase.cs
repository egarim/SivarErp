using DevExpress.Xpo;
using System;

namespace Sivar.Erp.Xpo.Core
{
    /// <summary>
    /// Base persistent class for all XPO entities that support archiving
    /// </summary>
    [Persistent("ArchivableEntity")]
    public abstract class XpoArchivableBase : XpoPersistentBase, IArchivable
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoArchivableBase(Session session) : base(session) { }

      

        private bool _isArchived;

        /// <summary>
        /// Indicates whether the entity is archived
        /// </summary>
        [Persistent("IsArchived")]
        public bool IsArchived
        {
            get => _isArchived;
            set => SetPropertyValue(nameof(IsArchived), ref _isArchived, value);
        }
    }
}