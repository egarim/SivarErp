using DevExpress.Xpo;
using System;

namespace Sivar.Erp.Xpo.Core
{
    /// <summary>
    /// Base persistent class for all XPO entities that support versioning
    /// </summary>
    [Persistent("VersionableEntity")]
    public abstract class XpoVersionableBase : XpoPersistentBase, IVersionable
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoVersionableBase(Session session) : base(session) { }

    

        private DateOnly _effectiveDate;

        /// <summary>
        /// The date from which this version is effective
        /// </summary>`
        [Persistent("EffectiveDate")]
        public DateOnly EffectiveDate
        {
            get => _effectiveDate;
            set => SetPropertyValue(nameof(EffectiveDate), ref _effectiveDate, value);
        }
    }
}