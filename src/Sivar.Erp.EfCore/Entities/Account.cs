using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Collections.Specialized;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.General;
using DevExpress.Data.Filtering;

using Sivar.Erp.Modules.Accounting.ChartOfAccounts;

namespace Sivar.Erp.EfCore.Entities;

[DefaultClassOptions()]
[NavigationItem("Accounting")]
/// <summary>
/// Entity Framework entity for Chart of Accounts
/// </summary>
[Table("Accounts")]
public class Account : BaseEntity, IAccount, ITreeNode
{
    /// <summary>
    /// Optional reference to balance sheet or income statement line
    /// </summary>
    public virtual Guid? BalanceAndIncomeLineId { get; set; }

    /// <summary>
    /// Name of the account
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Type of account (asset, liability, etc.)
    /// </summary>
    public virtual AccountType AccountType { get; set; }

    /// <summary>
    /// Official code/identifier for the account (e.g., for SAF-T reporting)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string OfficialCode { get; set; } = string.Empty;

    /// <summary>
    /// Official code/identifier for the parent account 
    /// </summary>
    [MaxLength(50)]
    public virtual string? ParentOfficialCode { get; set; }

    #region ITreeNode Implementation

    private IBindingList? _children;

    /// <summary>
    /// Gets the child accounts of this account
    /// </summary>
    [NotMapped]
    [Browsable(false)]
    public IBindingList Children
    {
        get
        {
            if (_children == null && ObjectSpace != null)
            {
                // Use ObjectSpace to query child accounts
                var criteria = CriteriaOperator.Parse("ParentOfficialCode = ?", this.OfficialCode);
                var childAccounts = ObjectSpace.GetObjects<Account>(criteria);
                _children = new BindingList<Account>(childAccounts.ToList());
                
                // Subscribe to collection changes to invalidate cache
                if (_children is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += (s, e) => _children = null;
                }
            }
            return _children ?? new BindingList<Account>();
        }
    }

    /// <summary>
    /// Gets the parent account of this account
    /// </summary>
    [NotMapped]
    [Browsable(false)]
    public ITreeNode? Parent
    {
        get
        {
            if (string.IsNullOrEmpty(ParentOfficialCode) || ObjectSpace == null)
                return null;

            var criteria = CriteriaOperator.Parse("OfficialCode = ?", this.ParentOfficialCode);
            return ObjectSpace.FindObject<Account>(criteria);
        }
    }

    /// <summary>
    /// Gets the display name for the tree node
    /// </summary>
    [NotMapped]
    [Browsable(false)]
    string ITreeNode.Name => AccountName;

    /// <summary>
    /// Invalidates the children cache to force refresh
    /// </summary>
    public void InvalidateChildrenCache()
    {
        _children = null;
    }

    #endregion
}
