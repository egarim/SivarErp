using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Data Transfer Object implementation for ITotal with change notification
    /// </summary>
    public class TotalDto : ITotal, INotifyPropertyChanged
    {
        private System.Guid _oid;
        private string _concept;
        private decimal _total;

        public System.Guid Oid
        {
            get => _oid;
            set
            {
                if (_oid != value)
                {
                    var oldValue = _oid;
                    _oid = value;
                    OnPropertyChanged(nameof(Oid), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public string Concept
        {
            get => _concept;
            set
            {
                if (_concept != value)
                {
                    var oldValue = _concept;
                    _concept = value;
                    OnPropertyChanged(nameof(Concept), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public decimal Total
        {
            get => _total;
            set
            {
                if (_total != value)
                {
                    var oldValue = _total;
                    _total = value;
                    OnPropertyChanged(nameof(Total), ChangeType.PropertyChanged, oldValue, value);
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