using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;
using Sivar.Erp.Modules.Documents.Core.Interfaces;

namespace Sivar.Erp.Modules.Documents.Application.DTOs
{
    /// <summary>
    /// Represents a document type in the system
    /// </summary>
    public class DocumentTypeDto : IDocumentType
    {
        private Guid _oid;
        private string _code = string.Empty;
        private string _name = string.Empty;
        private bool _isEnabled = true;

        /// <summary>
        /// Unique identifier for the document type
        /// </summary>
        public Guid ID
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
        /// Unique code for the document type (e.g., "INV", "PO")
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
        /// Display name of the document type
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
        /// Whether this document type is currently active
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

        /// <summary>
        /// Category or operation of the document type (e.g., "SalesInvoice", "PurchaseOrder")
        /// </summary>
        string documentOperation = string.Empty;
        /// <summary>
        /// Category or operation of the document type (e.g., "SalesInvoice", "PurchaseOrder")
        /// </summary>
        public string DocumentOperation
        {
            get => documentOperation;
            set
            {
                if (documentOperation == value)
                {
                    return;
                }

                documentOperation = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
