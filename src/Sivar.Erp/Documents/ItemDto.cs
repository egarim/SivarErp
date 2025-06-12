using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Data Transfer Object implementation for IItem with change notification
    /// </summary>
    public class ItemDto : IItem, INotifyPropertyChanged
    {
        Guid oid;
        private string _code;
        private string _type;
        private string _description;
        private decimal _basePrice;

        
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
        
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    var oldValue = _code;
                    _code = value;
                    OnPropertyChanged(nameof(Code), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    var oldValue = _type;
                    _type = value;
                    OnPropertyChanged(nameof(Type), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    var oldValue = _description;
                    _description = value;
                    OnPropertyChanged(nameof(Description), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName, ChangeType changeType, object oldValue = null, object newValue = null, string propertyPath = null)
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