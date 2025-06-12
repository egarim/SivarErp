using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Documents.Tax;

namespace Sivar.Erp.Taxes.TaxRule
{
    /// <summary>
    /// Defines rules for when a tax should be applied based on document type and entity/item groups
    /// </summary>
    public class TaxRuleDto : INotifyPropertyChanged
    {
        private Guid _oid;
        private Guid _taxId;
        private string _documentTypeCode;
        private Guid? _businessEntityGroupId;
        private Guid? _itemGroupId;
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
        public Guid TaxId
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
        /// Business entity group ID this rule applies to (null means any entity)
        /// </summary>
        public Guid? BusinessEntityGroupId
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
        public Guid? ItemGroupId
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