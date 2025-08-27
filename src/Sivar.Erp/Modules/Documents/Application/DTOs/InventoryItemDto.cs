using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Modules.Inventory;
using Sivar.Erp.Modules.Documents.Core.Entities;
using Sivar.Erp.Modules.Documents.Core.Enums;

namespace Sivar.Erp.Modules.Documents.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object implementation for IInventoryItem with change notification
    /// </summary>
    public class InventoryItemDto : ItemDto, IInventoryItem
    {
        private bool _isInventoryTracked;
        private string _unitOfMeasure = string.Empty;
        private decimal _reorderPoint;
        private decimal _reorderQuantity;
        private decimal _averageCost;
        private string _location = string.Empty;
        private InventoryValuationMethod _valuationMethod;

        /// <summary>
        /// Whether this item is tracked in inventory
        /// </summary>
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

        /// <summary>
        /// Unit of measure for the item
        /// </summary>
        public string UnitOfMeasure
        {
            get => _unitOfMeasure;
            set
            {
                if (_unitOfMeasure != value)
                {
                    var oldValue = _unitOfMeasure;
                    _unitOfMeasure = value ?? string.Empty;
                    OnPropertyChanged(nameof(UnitOfMeasure), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Minimum stock level that triggers reordering
        /// </summary>
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

        /// <summary>
        /// Quantity to order when reordering
        /// </summary>
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

        /// <summary>
        /// Average cost of the item
        /// </summary>
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

        /// <summary>
        /// Storage location for the item
        /// </summary>
        public string Location
        {
            get => _location;
            set
            {
                if (_location != value)
                {
                    var oldValue = _location;
                    _location = value ?? string.Empty;
                    OnPropertyChanged(nameof(Location), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }
        
        /// <summary>
        /// Inventory valuation method for the item
        /// </summary>
        public InventoryValuationMethod ValuationMethod
        {
            get => _valuationMethod;
            set
            {
                if (_valuationMethod != value)
                {
                    var oldValue = _valuationMethod;
                    _valuationMethod = value;
                    OnPropertyChanged(nameof(ValuationMethod), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }
    }
}
