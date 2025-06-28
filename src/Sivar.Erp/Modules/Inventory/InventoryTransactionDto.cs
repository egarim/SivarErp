using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Data Transfer Object implementation for IInventoryTransaction
    /// </summary>
    public class InventoryTransactionDto : IInventoryTransaction, INotifyPropertyChanged
    {
        private string _transactionId;
        private string _transactionNumber;
        private IInventoryItem _item;
        private InventoryTransactionType _transactionType;
        private decimal _quantity;
        private decimal _unitCost;
        private string _sourceWarehouseCode;
        private string _destinationWarehouseCode;
        private string _referenceDocumentNumber;
        private DateOnly _transactionDate;
        private string _createdBy;
        private DateTime _createdAt;
        private string _notes;

        public string TransactionId 
        { 
            get => _transactionId; 
            set
            {
                if (_transactionId != value)
                {
                    _transactionId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TransactionNumber
        {
            get => _transactionNumber;
            set
            {
                if (_transactionNumber != value)
                {
                    _transactionNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public IInventoryItem Item
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

        public InventoryTransactionType TransactionType
        {
            get => _transactionType;
            set
            {
                if (_transactionType != value)
                {
                    _transactionType = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalValue));
                }
            }
        }

        public decimal UnitCost
        {
            get => _unitCost;
            set
            {
                if (_unitCost != value)
                {
                    _unitCost = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalValue));
                }
            }
        }

        public string SourceWarehouseCode
        {
            get => _sourceWarehouseCode;
            set
            {
                if (_sourceWarehouseCode != value)
                {
                    _sourceWarehouseCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DestinationWarehouseCode
        {
            get => _destinationWarehouseCode;
            set
            {
                if (_destinationWarehouseCode != value)
                {
                    _destinationWarehouseCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ReferenceDocumentNumber
        {
            get => _referenceDocumentNumber;
            set
            {
                if (_referenceDocumentNumber != value)
                {
                    _referenceDocumentNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateOnly TransactionDate
        {
            get => _transactionDate;
            set
            {
                if (_transactionDate != value)
                {
                    _transactionDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CreatedBy
        {
            get => _createdBy;
            set
            {
                if (_createdBy != value)
                {
                    _createdBy = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalValue => Quantity * UnitCost;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}