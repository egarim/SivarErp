using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sivar.Erp.Services.Taxes.TaxGroup
{
    /// <summary>
    /// Represents a group for tax purposes
    /// </summary>
    public class TaxGroupDto : ITaxGroup
    {
        private Guid _oid;
        private string _code;
        private string _name;
        private string _description;
        private bool _isEnabled = true;

        /// <summary>
        /// Unique identifier for the tax group
        /// </summary>
        public Guid Oid
        {
            get => _oid;
            set
            {
                if (_oid != value)
                {
                    _oid = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Unique code for the tax group
        /// </summary>
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Display name of the tax group
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Description of the tax group
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the tax group is currently active
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}