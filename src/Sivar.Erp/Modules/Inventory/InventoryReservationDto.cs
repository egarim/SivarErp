using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Represents an inventory reservation
    /// </summary>
    public class InventoryReservationDto : IInventoryReservation, INotifyPropertyChanged
    {
        private string _reservationId = string.Empty;
        private IInventoryItem _item = null!;
        private decimal _quantity;
        private string _warehouseCode = string.Empty;
        private string _sourceDocumentNumber = string.Empty;
        private ReservationStatus _status;
        private string _createdBy = string.Empty;
        private DateTime _createdAt;
        private DateTime _expiresAt;
        private DateTime _lastUpdated;
        private string _notes = string.Empty;

        /// <summary>
        /// Gets or sets the reservation ID
        /// </summary>
        public string ReservationId
        {
            get => _reservationId;
            set
            {
                if (_reservationId != value)
                {
                    _reservationId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the inventory item being reserved
        /// </summary>
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

        /// <summary>
        /// Gets or sets the quantity being reserved
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
                }
            }
        }

        /// <summary>
        /// Gets or sets the warehouse code where the item is reserved
        /// </summary>
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

        /// <summary>
        /// Gets or sets the source document number (e.g., sales order number)
        /// </summary>
        public string SourceDocumentNumber
        {
            get => _sourceDocumentNumber;
            set
            {
                if (_sourceDocumentNumber != value)
                {
                    _sourceDocumentNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the reservation status
        /// </summary>
        public ReservationStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the user who created the reservation
        /// </summary>
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

        /// <summary>
        /// Gets or sets when the reservation was created
        /// </summary>
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

        /// <summary>
        /// Gets or sets when the reservation expires
        /// </summary>
        public DateTime ExpiresAt
        {
            get => _expiresAt;
            set
            {
                if (_expiresAt != value)
                {
                    _expiresAt = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets when the reservation was last updated
        /// </summary>
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

        /// <summary>
        /// Gets or sets notes or comments
        /// </summary>
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

        /// <summary>
        /// Gets whether the reservation is expired based on current time
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > ExpiresAt && Status == ReservationStatus.Active;

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when a property changes
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}