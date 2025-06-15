using Sivar.Erp.Documents;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sivar.Erp.BusinesEntities
{
    /// <summary>
    /// Data Transfer Object implementation for IBusinessEntity with change notification
    /// </summary>
    public class BusinessEntityDto : IBusinessEntity, INotifyPropertyChanged
    {
        Guid oid;
        private string _code;
        private string _name;
        private string _address;
        private string _city;
        private string _state;
        private string _zipCode;
        private string _country;
        private string _phoneNumber;
        private string _email;

        
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

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    var oldValue = _name;
                    _name = value;
                    OnPropertyChanged(nameof(Name), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    var oldValue = _address;
                    _address = value;
                    OnPropertyChanged(nameof(Address), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string City
        {
            get => _city;
            set
            {
                if (_city != value)
                {
                    var oldValue = _city;
                    _city = value;
                    OnPropertyChanged(nameof(City), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    var oldValue = _state;
                    _state = value;
                    OnPropertyChanged(nameof(State), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string ZipCode
        {
            get => _zipCode;
            set
            {
                if (_zipCode != value)
                {
                    var oldValue = _zipCode;
                    _zipCode = value;
                    OnPropertyChanged(nameof(ZipCode), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (_country != value)
                {
                    var oldValue = _country;
                    _country = value;
                    OnPropertyChanged(nameof(Country), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (_phoneNumber != value)
                {
                    var oldValue = _phoneNumber;
                    _phoneNumber = value;
                    OnPropertyChanged(nameof(PhoneNumber), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    var oldValue = _email;
                    _email = value;
                    OnPropertyChanged(nameof(Email), ChangeType.PropertyChanged, oldValue, value);
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