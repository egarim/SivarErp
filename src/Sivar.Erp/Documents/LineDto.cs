using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Documents.Tax;
using Sivar.Erp.Taxes;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Represents a line item in a document with tax support
    /// </summary>
    public class LineDto : IDocumentLine, INotifyPropertyChanged
    {
        private IItem _item;
        private string _lineItem;
        private decimal _amount;
        private decimal _quantity = 1;
        private decimal _unitPrice;
        private ObservableCollection<ITotal> _lineTotals = new();
        private ObservableCollection<TaxDto> _taxes = new();

        public LineDto()
        {
            // Subscribe to collections
            _lineTotals.CollectionChanged += LineTotals_CollectionChanged;
            _taxes.CollectionChanged += Taxes_CollectionChanged;
        }

        public IItem Item
        {
            get => _item;
            set
            {
                if (_item != value)
                {
                    _item = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string LineItem
        {
            get => _lineItem;
            set
            {
                if (_lineItem != value)
                {
                    _lineItem = value;
                    OnPropertyChanged();
                }
            }
        }
        
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
        /// Quantity of items in the line
        /// </summary>
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    RecalculateLineAmount();
                }
            }
        }

        /// <summary>
        /// Unit price of the item
        /// </summary>
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged();
                    RecalculateLineAmount();
                }
            }
        }
        
        public IList<ITotal> LineTotals
        {
            get => _lineTotals;
            set
            {
                if (_lineTotals != null)
                {
                    _lineTotals.CollectionChanged -= LineTotals_CollectionChanged;
                }

                if (value is ObservableCollection<ITotal> collection)
                {
                    _lineTotals = collection;
                }
                else
                {
                    _lineTotals = new ObservableCollection<ITotal>(value ?? new List<ITotal>());
                }

                _lineTotals.CollectionChanged += LineTotals_CollectionChanged;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Taxes applied to this line
        /// </summary>
        public IList<TaxDto> Taxes
        {
            get => _taxes;
            set
            {
                if (_taxes != null)
                {
                    _taxes.CollectionChanged -= Taxes_CollectionChanged;
                }

                if (value is ObservableCollection<TaxDto> collection)
                {
                    _taxes = collection;
                }
                else
                {
                    _taxes = new ObservableCollection<TaxDto>(value ?? new List<TaxDto>());
                }

                _taxes.CollectionChanged += Taxes_CollectionChanged;
                OnPropertyChanged();
                RecalculateTaxes();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LineTotals_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Handle changes to line totals
        }

        private void Taxes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Handle new taxes
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is TaxDto taxDto)
                    {
                        taxDto.PropertyChanged += TaxDto_PropertyChanged;
                    }
                }
            }

            // Handle removed taxes
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is TaxDto taxDto)
                    {
                        taxDto.PropertyChanged -= TaxDto_PropertyChanged;
                    }
                }
            }

            RecalculateTaxes();
        }

        private void TaxDto_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // When any tax property changes, recalculate
            RecalculateTaxes();
        }

        /// <summary>
        /// Recalculates the line amount based on quantity and unit price
        /// </summary>
        private void RecalculateLineAmount()
        {
            Amount = Quantity * UnitPrice;
            RecalculateTaxes();
        }

        /// <summary>
        /// Calculates all line level taxes and updates totals
        /// </summary>
        private void RecalculateTaxes()
        {
            // Remove existing tax totals
            RemoveExistingTaxTotals();

            // Only calculate line level taxes
            foreach (var tax in _taxes)
            {
                if (!tax.IsEnabled || tax.ApplicationLevel != TaxApplicationLevel.Line)
                    continue;

                decimal taxAmount = CalculateTaxAmount(tax);
                
                // Add as a line total
                AddTaxTotal(tax, taxAmount);
            }
        }

        /// <summary>
        /// Calculate tax amount based on tax type
        /// </summary>
        private decimal CalculateTaxAmount(TaxDto tax)
        {
            switch (tax.TaxType)
            {
                case TaxType.Percentage:
                    return Amount * (tax.Percentage / 100m);
                    
                case TaxType.FixedAmount:
                    return tax.Amount;
                    
                case TaxType.AmountPerUnit:
                    return tax.Amount * Quantity;
                    
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Remove existing tax totals before recalculation
        /// </summary>
        private void RemoveExistingTaxTotals()
        {
            for (int i = _lineTotals.Count - 1; i >= 0; i--)
            {
                var total = _lineTotals[i];
                // Assume tax totals have concept starting with "Tax:"
                if (total.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                {
                    _lineTotals.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Add a tax as a line total
        /// </summary>
        private void AddTaxTotal(TaxDto tax, decimal amount)
        {
            // Create a new total for this tax
            var taxTotal = new TotalDto
            {
                Oid = Guid.NewGuid(),
                Concept = $"Tax: {tax.Name} ({tax.Code})",
                Total = amount
            };
            
            _lineTotals.Add(taxTotal);
        }
    }
}