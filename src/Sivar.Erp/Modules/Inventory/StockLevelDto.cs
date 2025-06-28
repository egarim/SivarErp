using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Data Transfer Object implementation for IStockLevel
    /// </summary>
    public class StockLevelDto : IStockLevel, INotifyPropertyChanged
    {
        private string _id;
        private IInventoryItem _item;
        private string _warehouseCode;
        private decimal _quantityOnHand;
        private decimal _quantityReserved;
        private decimal _quantityOnOrder;
        private DateTime _lastUpdated;

        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
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

        public string WarehouseCode
        {
            get => _warehouseCode;
            set
            {
                if (_warehouseCode != value)
                {
                    _warehouseCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal QuantityOnHand
        {
            get => _quantityOnHand;
            set
            {
                if (_quantityOnHand != value)
                {
                    _quantityOnHand = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AvailableQuantity));
                }
            }
        }

        public decimal QuantityReserved
        {
            get => _quantityReserved;
            set
            {
                if (_quantityReserved != value)
                {
                    _quantityReserved = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AvailableQuantity));
                }
            }
        }

        public decimal QuantityOnOrder
        {
            get => _quantityOnOrder;
            set
            {
                if (_quantityOnOrder != value)
                {
                    _quantityOnOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set
            {
                if (_lastUpdated != value)
                {
                    _lastUpdated = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal AvailableQuantity => _quantityOnHand - _quantityReserved;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}