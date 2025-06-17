using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Services.Taxes.TaxRule
{
    /// <summary>
    /// Defines rules for when a tax should be applied based on document type and entity/item groups
    /// </summary>
    public class TaxRuleDto : ITaxRule, INotifyPropertyChanged
    {
        private Guid _oid;
        private string _taxId;
        private string _documentTypeCode;
        private DocumentOperation? _documentOperation;
        private string _businessEntityGroupId;
        private string _itemGroupId;
        private bool _isEnabled = true;
        private int _priority = 1;

        /// <summary>
        /// Unique identifier for the tax rule
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
        /// Reference to the tax that should be applied
        /// </summary>
        public string TaxId
        {
            get => _taxId;
            set
            {
                if (_taxId != value)
                {
                    _taxId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Document type code this rule applies to (null means any document type)
        /// </summary>
        [Obsolete("Use DocumentOperation property instead")]
        public string DocumentTypeCode
        {
            get => _documentTypeCode;
            set
            {
                if (_documentTypeCode != value)
                {
                    _documentTypeCode = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Document operation this rule applies to (null means any document operation)
        /// </summary>
        public DocumentOperation? DocumentOperation
        {
            get => _documentOperation;
            set
            {
                if (_documentOperation != value)
                {
                    _documentOperation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Business entity group ID this rule applies to (null means any entity)
        /// </summary>
        public string BusinessEntityGroupId
        {
            get => _businessEntityGroupId;
            set
            {
                if (_businessEntityGroupId != value)
                {
                    _businessEntityGroupId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Item group ID this rule applies to (null means any item)
        /// </summary>
        public string ItemGroupId
        {
            get => _itemGroupId;
            set
            {
                if (_itemGroupId != value)
                {
                    _itemGroupId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether this tax rule is currently active
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Priority of the rule (lower numbers = higher priority)
        /// </summary>
        public int Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
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