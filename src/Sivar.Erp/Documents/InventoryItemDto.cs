using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Data Transfer Object implementation for IInventoryItem with change notification
    /// </summary>
    public class InventoryItemDto : ItemDto, IInventoryItem
    {
        private bool _isInventoryTracked;
        private string _unitOfMeasure;
        private decimal _reorderPoint;
        private decimal _reorderQuantity;
        private decimal _averageCost;
        private string _location;

        public bool IsInventoryTracked
        {
            get => _isInventoryTracked;
            set
            {
                if (_isInventoryTracked != value)
                {
                    var oldValue = _isInventoryTracked;
                    _isInventoryTracked = value;
                    OnPropertyChanged(nameof(IsInventoryTracked), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string UnitOfMeasure
        {
            get => _unitOfMeasure;
            set
            {
                if (_unitOfMeasure != value)
                {
                    var oldValue = _unitOfMeasure;
                    _unitOfMeasure = value;
                    OnPropertyChanged(nameof(UnitOfMeasure), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public decimal ReorderPoint
        {
            get => _reorderPoint;
            set
            {
                if (_reorderPoint != value)
                {
                    var oldValue = _reorderPoint;
                    _reorderPoint = value;
                    OnPropertyChanged(nameof(ReorderPoint), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public decimal ReorderQuantity
        {
            get => _reorderQuantity;
            set
            {
                if (_reorderQuantity != value)
                {
                    var oldValue = _reorderQuantity;
                    _reorderQuantity = value;
                    OnPropertyChanged(nameof(ReorderQuantity), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public decimal AverageCost
        {
            get => _averageCost;
            set
            {
                if (_averageCost != value)
                {
                    var oldValue = _averageCost;
                    _averageCost = value;
                    OnPropertyChanged(nameof(AverageCost), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                if (_location != value)
                {
                    var oldValue = _location;
                    _location = value;
                    OnPropertyChanged(nameof(Location), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }
    }
}