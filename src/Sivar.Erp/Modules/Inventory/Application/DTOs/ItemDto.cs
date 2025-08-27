using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;
using Sivar.Erp.Core.Contracts;
using Sivar.Erp.Modules.Documents.Core.Enums;
using Sivar.Erp.Modules.Documents.Core.ValueObjects;

namespace Sivar.Erp.Modules.Inventory.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object implementation for IItem with change notification
    /// </summary>
    public class ItemDto : Modules.Inventory.Core.Interfaces.IItem, Sivar.Erp.Core.Contracts.IItem, INotifyPropertyChanged
    {
        Guid oid;
        private string _code = string.Empty;
        private string _type = string.Empty;
        private string _description = string.Empty;
        private decimal _basePrice;

        /// <summary>
        /// Unique identifier for the item
        /// </summary>
        public Guid Oid
        {
            get => oid;
            set
            {
                if (oid == value)
                    return;
                oid = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Item code or SKU
        /// </summary>
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    var oldValue = _code;
                    _code = value ?? string.Empty;
                    OnPropertyChanged(nameof(Code), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Type or category of the item
        /// </summary>
        public string Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    var oldValue = _type;
                    _type = value ?? string.Empty;
                    OnPropertyChanged(nameof(Type), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    var oldValue = _description;
                    _description = value ?? string.Empty;
                    OnPropertyChanged(nameof(Description), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Base price of the item
        /// </summary>
        public decimal BasePrice
        {
            get => _basePrice;
            set
            {
                if (_basePrice != value)
                {
                    var oldValue = _basePrice;
                    _basePrice = value;
                    OnPropertyChanged(nameof(BasePrice), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        private string? _category;
        private bool _isActive = true;
        private DateTime _createdDate = DateTime.UtcNow;
        private string _createdBy = string.Empty;

        /// <summary>
        /// Category of the item
        /// </summary>
        public string? Category
        {
            get => _category;
            set
            {
                if (_category != value)
                {
                    var oldValue = _category;
                    _category = value;
                    OnPropertyChanged(nameof(Category), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Whether the item is active
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    var oldValue = _isActive;
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Date when the item was created
        /// </summary>
        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                if (_createdDate != value)
                {
                    var oldValue = _createdDate;
                    _createdDate = value;
                    OnPropertyChanged(nameof(CreatedDate), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        /// <summary>
        /// User who created the item
        /// </summary>
        public string CreatedBy
        {
            get => _createdBy;
            set
            {
                if (_createdBy != value)
                {
                    var oldValue = _createdBy;
                    _createdBy = value ?? string.Empty;
                    OnPropertyChanged(nameof(CreatedBy), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        /// <summary>
        /// Event raised when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the PropertyChanged event with change details
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        /// <param name="changeType">Type of change</param>
        /// <param name="oldValue">Previous value</param>
        /// <param name="newValue">New value</param>
        /// <param name="propertyPath">Path to the property</param>
        protected virtual void OnPropertyChanged(string propertyName, ChangeType changeType, object? oldValue = null, object? newValue = null, string? propertyPath = null)
        {
            PropertyChanged?.Invoke(this, new DocumentPropertyChangedEventArgs(
                propertyName,
                this,
                changeType,
                oldValue,
                newValue,
                propertyPath));
        }
    }
}
