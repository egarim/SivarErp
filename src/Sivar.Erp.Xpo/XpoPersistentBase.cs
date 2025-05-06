using DevExpress.Xpo;
using System;

namespace Sivar.Erp.Xpo.Core
{
    /// <summary>
    /// Base persistent class for all XPO entities
    /// </summary>
    [Persistent("BaseEntity")]
    public abstract class XpoPersistentBase : XPObject, IEntity, IAuditable
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoPersistentBase(Session session) : base(session) { }

        private Guid _guid;

        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Persistent("Guid")]
        [Indexed(Unique = true)]
        public Guid Id
        {
            get => _guid;
            set => SetPropertyValue(nameof(Id), ref _guid, value);
        }

        private DateTime _insertedAt;

        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        [Persistent("InsertedAt")]
        public DateTime InsertedAt
        {
            get => _insertedAt;
            set => SetPropertyValue(nameof(InsertedAt), ref _insertedAt, value);
        }

        private string _insertedBy = string.Empty;

        /// <summary>
        /// User who created the entity
        /// </summary>
        [Persistent("InsertedBy"), Size(255)]
        public string InsertedBy
        {
            get => _insertedBy;
            set => SetPropertyValue(nameof(InsertedBy), ref _insertedBy, value);
        }

        private DateTime _updatedAt;

        /// <summary>
        /// UTC timestamp when the entity was last updated
        /// </summary>
        [Persistent("UpdatedAt")]
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetPropertyValue(nameof(UpdatedAt), ref _updatedAt, value);
        }

        private string _updatedBy = string.Empty;

        /// <summary>
        /// User who last updated the entity
        /// </summary>
        [Persistent("UpdatedBy"), Size(255)]
        public string UpdatedBy
        {
            get => _updatedBy;
            set => SetPropertyValue(nameof(UpdatedBy), ref _updatedBy, value);
        }

        /// <summary>
        /// Override to initialize new objects
        /// </summary>
        protected override void OnSaving()
        {
            base.OnSaving();

            if (Session.IsNewObject(this) && Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }
    }
}