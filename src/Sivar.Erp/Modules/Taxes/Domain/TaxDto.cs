using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Core.Enums;

namespace Sivar.Erp.Modules.Taxes.Domain
{
    /// <summary>
    /// Represents a tax item that can be applied to document lines
    /// </summary>
    public class TaxDto : INotifyPropertyChanged, ITax
    {
        private Guid _oid;
        private string _name;
        private string _code;
        private TaxType _taxType;
        private TaxApplicationLevel _applicationLevel;
        private decimal _amount;
        private decimal _percentage;
        private bool _isEnabled = true;
        private bool _isIncludedInPrice;

        /// <summary>
        /// Unique identifier for the tax
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
        /// Display name of the tax
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Short code for the tax (e.g., VAT, GST)
        /// </summary>
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Type of tax calculation (by percentage, fixed amount, or per quantity)
        /// </summary>
        public TaxType TaxType
        {
            get => _taxType;
            set
            {
                if (_taxType != value)
                {
                    _taxType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Level at which the tax should be applied (line or document)
        /// </summary>
        public TaxApplicationLevel ApplicationLevel
        {
            get => _applicationLevel;
            set
            {
                if (_applicationLevel != value)
                {
                    _applicationLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Fixed amount to apply when TaxType is FixedAmount or AmountPerUnit
        /// </summary>
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Percentage to apply when TaxType is Percentage
        /// </summary>
        public decimal Percentage
        {
            get => _percentage;
            set
            {
                if (_percentage != value)
                {
                    _percentage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether this tax is currently active
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
        /// Whether the tax is already included in the line price
        /// </summary>
        public bool IsIncludedInPrice
        {
            get => _isIncludedInPrice;
            set
            {
                if (_isIncludedInPrice != value)
                {
                    _isIncludedInPrice = value;
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