using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;

namespace Sivar.Erp.Xaf.Module.BusinessObjects
{
    /// <summary>
    /// Base class for all ERP business objects providing common audit and tracking functionality
    /// </summary>
    [NonPersistent]
    public abstract class ErpBaseObject : BaseObject
    {
        public ErpBaseObject(Session session) : base(session) { }

        DateTime createdOn = DateTime.UtcNow;
        /// <summary>
        /// Date and time when the record was created (UTC)
        /// </summary>
        [XafDisplayName("Created On")]
        [ModelDefault("AllowEdit", "False")]
        [Index(1000)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }

        string createdBy = string.Empty;
        /// <summary>
        /// User who created the record
        /// </summary>
        [Size(100)]
        [XafDisplayName("Created By")]
        [ModelDefault("AllowEdit", "False")]
        [Index(1001)]
        public string CreatedBy
        {
            get => createdBy;
            set => SetPropertyValue(nameof(CreatedBy), ref createdBy, value);
        }

        DateTime modifiedOn = DateTime.UtcNow;
        /// <summary>
        /// Date and time when the record was last modified (UTC)
        /// </summary>
        [XafDisplayName("Modified On")]
        [ModelDefault("AllowEdit", "False")]
        [Index(1002)]
        public DateTime ModifiedOn
        {
            get => modifiedOn;
            set => SetPropertyValue(nameof(ModifiedOn), ref modifiedOn, value);
        }

        string modifiedBy = string.Empty;
        /// <summary>
        /// User who last modified the record
        /// </summary>
        [Size(100)]
        [XafDisplayName("Modified By")]
        [ModelDefault("AllowEdit", "False")]
        [Index(1003)]
        public string ModifiedBy
        {
            get => modifiedBy;
            set => SetPropertyValue(nameof(ModifiedBy), ref modifiedBy, value);
        }

        int version = 0;
        /// <summary>
        /// Version number for optimistic concurrency control
        /// </summary>
        [XafDisplayName("Version")]
        [ModelDefault("AllowEdit", "False")]
        [Browsable(false)]
        public int Version
        {
            get => version;
            set => SetPropertyValue(nameof(Version), ref version, value);
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            CreatedOn = DateTime.UtcNow;
            ModifiedOn = DateTime.UtcNow;
            
            // Set created/modified by from current security context
            var currentUser = SecuritySystem.CurrentUserName ?? "System";
            CreatedBy = currentUser;
            ModifiedBy = currentUser;
        }

        protected override void OnSaving()
        {
            // Update modification tracking
            ModifiedOn = DateTime.UtcNow;
            ModifiedBy = SecuritySystem.CurrentUserName ?? "System";
            Version++;
            
            base.OnSaving();
        }
    }
}