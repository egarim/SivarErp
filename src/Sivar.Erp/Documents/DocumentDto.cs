using Sivar.Erp.BusinesEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Data Transfer Object implementation for IDocument with enhanced change notification
    /// </summary>
    public class DocumentDto : IDocument, INotifyPropertyChanged
    {
        string documentNumber;
        Guid oid;
        private DateOnly _date;
        private TimeOnly _time;
        private IBusinessEntity _businessEntity;
        private IDocumentType _documentType;
        private ObservableCollection<IDocumentLine> _lines;
        private ObservableCollection<ITotal> _documentTotals;

        public DocumentDto()
        {
            _lines = new ObservableCollection<IDocumentLine>();
            _documentTotals = new ObservableCollection<ITotal>();

            // Subscribe to collection change notifications
            _lines.CollectionChanged += Lines_CollectionChanged;
            _documentTotals.CollectionChanged += DocumentTotals_CollectionChanged;
        }

        
        public string DocumentNumber
        {
            get => documentNumber;
            set
            {
                if (documentNumber == value)
                    return;
                documentNumber = value;
                OnPropertyChanged();
            }
        }
        
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
        
        public DateOnly Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    var oldValue = _date;
                    _date = value;
                    OnPropertyChanged(nameof(Date), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public TimeOnly Time
        {
            get => _time;
            set
            {
                if (_time != value)
                {
                    var oldValue = _time;
                    _time = value;
                    OnPropertyChanged(nameof(Time), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public IBusinessEntity BusinessEntity
        {
            get => _businessEntity;
            set
            {
                if (_businessEntity != value)
                {
                    // Unsubscribe from old business entity if it supports change notification
                    if (_businessEntity is INotifyPropertyChanged oldEntity)
                    {
                        oldEntity.PropertyChanged -= SubObject_PropertyChanged;
                    }

                    var oldValue = _businessEntity;
                    _businessEntity = value;

                    // Subscribe to new business entity if it supports change notification
                    if (_businessEntity is INotifyPropertyChanged newEntity)
                    {
                        newEntity.PropertyChanged += SubObject_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(BusinessEntity), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }
        
        public IDocumentType DocumentType
        {
            get => _documentType;
            set
            {
                if (_documentType != value)
                {
                    // Unsubscribe from old document type if it supports change notification
                    if (_documentType is INotifyPropertyChanged oldDocType)
                    {
                        oldDocType.PropertyChanged -= DocumentType_PropertyChanged;
                    }

                    var oldValue = _documentType;
                    _documentType = value;

                    // Subscribe to new document type if it supports change notification
                    if (_documentType is INotifyPropertyChanged newDocType)
                    {
                        newDocType.PropertyChanged += DocumentType_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(DocumentType), ChangeType.PropertyChanged, oldValue, value);
                }
            }
        }

        public IList<IDocumentLine> Lines
        {
            get => _lines;
            set
            {
                // Unsubscribe from old collection
                if (_lines != null)
                {
                    _lines.CollectionChanged -= Lines_CollectionChanged;
                    UnsubscribeFromDocumentLines(_lines);
                }

                var oldCollection = _lines;
                if (value is ObservableCollection<IDocumentLine> collection)
                {
                    _lines = collection;
                }
                else
                {
                    _lines = new ObservableCollection<IDocumentLine>(value ?? new List<IDocumentLine>());
                }

                // Subscribe to new collection
                _lines.CollectionChanged += Lines_CollectionChanged;
                SubscribeToDocumentLines(_lines);

                OnPropertyChanged(nameof(Lines), ChangeType.CollectionReplaced, oldCollection, _lines);
            }
        }

        public IList<ITotal> DocumentTotals
        {
            get => _documentTotals;
            set
            {
                if (_documentTotals != null && _documentTotals is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= DocumentTotals_CollectionChanged;
                    UnsubscribeFromTotals(_documentTotals);
                }

                var oldTotals = _documentTotals;
                if (value is ObservableCollection<ITotal> collection)
                {
                    _documentTotals = collection;
                }
                else
                {
                    _documentTotals = new ObservableCollection<ITotal>(value ?? new List<ITotal>());
                }

                if (_documentTotals is INotifyCollectionChanged newCollection)
                {
                    newCollection.CollectionChanged += DocumentTotals_CollectionChanged;
                    SubscribeToTotals(_documentTotals);
                }

                OnPropertyChanged(nameof(DocumentTotals), ChangeType.CollectionReplaced, oldTotals, _documentTotals);
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

        private void DocumentType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // When a property in the document type changes, include path information
            string propertyPath = $"DocumentType.{e.PropertyName}";
            object newValue = null;

            // Try to get the new value
            var property = _documentType.GetType().GetProperty(e.PropertyName);
            if (property != null)
            {
                newValue = property.GetValue(_documentType);
            }

            OnPropertyChanged(nameof(DocumentType), ChangeType.NestedPropertyChanged, null, newValue, propertyPath);
        }

        private void Lines_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Handle items that were added to the collection
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is INotifyPropertyChanged notifyItem)
                    {
                        notifyItem.PropertyChanged += DocumentLine_PropertyChanged;
                    }

                    // Subscribe to LineTotals changes if the document line has totals
                    if (item is IDocumentLine line && line.LineTotals is INotifyCollectionChanged notifyCollection)
                    {
                        notifyCollection.CollectionChanged += LineTotals_CollectionChanged;
                    }
                }

                // Notify about the new items added
                OnPropertyChanged(nameof(Lines), ChangeType.CollectionItemAdded, null, e.NewItems);
            }

            // Handle items that were removed from the collection
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyPropertyChanged notifyItem)
                    {
                        notifyItem.PropertyChanged -= DocumentLine_PropertyChanged;
                    }

                    // Unsubscribe from LineTotals changes
                    if (item is IDocumentLine line && line.LineTotals is INotifyCollectionChanged notifyCollection)
                    {
                        notifyCollection.CollectionChanged -= LineTotals_CollectionChanged;
                    }
                }

                // Notify about items removed
                OnPropertyChanged(nameof(Lines), ChangeType.CollectionItemRemoved, e.OldItems, null);
            }
        }

        private void DocumentTotals_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ChangeType changeType = ChangeType.PropertyChanged;
            object oldItems = null;
            object newItems = null;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    changeType = ChangeType.CollectionItemAdded;
                    newItems = e.NewItems;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    changeType = ChangeType.CollectionItemRemoved;
                    oldItems = e.OldItems;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    changeType = ChangeType.CollectionReplaced;
                    oldItems = e.OldItems;
                    newItems = e.NewItems;
                    break;
            }

            OnPropertyChanged(nameof(DocumentTotals), changeType, oldItems, newItems);
        }

        private void DocumentLine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Add more context about which line changed
            string propertyPath = $"Lines[{_lines.IndexOf(sender as IDocumentLine)}].{e.PropertyName}";
            object newValue = null;
            
            // Try to get the new value if possible
            if (sender is IDocumentLine line && e.PropertyName != null)
            {
                var property = line.GetType().GetProperty(e.PropertyName);
                if (property != null)
                {
                    newValue = property.GetValue(line);
                }
            }

            OnPropertyChanged(nameof(Lines), ChangeType.NestedPropertyChanged, null, newValue, propertyPath);
        }

        private void LineTotals_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Find which line this belongs to
            int lineIndex = -1;
            for (int i = 0; i < _lines.Count; i++)
            {
                if (_lines[i].LineTotals == sender)
                {
                    lineIndex = i;
                    break;
                }
            }

            string propertyPath = lineIndex >= 0 ? $"Lines[{lineIndex}].LineTotals" : "LineTotals";
            
            ChangeType changeType = ChangeType.PropertyChanged;
            if (e.Action == NotifyCollectionChangedAction.Add)
                changeType = ChangeType.CollectionItemAdded;
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                changeType = ChangeType.CollectionItemRemoved;
            else if (e.Action == NotifyCollectionChangedAction.Replace)
                changeType = ChangeType.CollectionReplaced;

            OnPropertyChanged(nameof(Lines), changeType, e.OldItems, e.NewItems, propertyPath);
        }

        private void SubObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == _businessEntity)
            {
                // When a property in the business entity changes, include path information
                string propertyPath = $"BusinessEntity.{e.PropertyName}";
                object newValue = null;

                // Try to get the new value
                var property = _businessEntity.GetType().GetProperty(e.PropertyName);
                if (property != null)
                {
                    newValue = property.GetValue(_businessEntity);
                }

                OnPropertyChanged(nameof(BusinessEntity), ChangeType.NestedPropertyChanged, null, newValue, propertyPath);
            }
        }

        private void Total_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // When a property in any total changes
            int totalIndex = -1;
            for (int i = 0; i < _documentTotals.Count; i++)
            {
                if (_documentTotals[i] == sender)
                {
                    totalIndex = i;
                    break;
                }
            }

            string propertyPath = totalIndex >= 0 ? $"DocumentTotals[{totalIndex}].{e.PropertyName}" : null;
            object newValue = null;
            
            // Try to get the new value
            var property = sender.GetType().GetProperty(e.PropertyName);
            if (property != null)
            {
                newValue = property.GetValue(sender);
            }

            OnPropertyChanged(nameof(DocumentTotals), ChangeType.NestedPropertyChanged, null, newValue, propertyPath);
        }

        private void SubscribeToDocumentLines(IEnumerable<IDocumentLine> lines)
        {
            foreach (var line in lines)
            {
                if (line is INotifyPropertyChanged notifyLine)
                {
                    notifyLine.PropertyChanged += DocumentLine_PropertyChanged;
                }

                // Also subscribe to line totals if they implement collection changed
                if (line.LineTotals is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += LineTotals_CollectionChanged;
                }

                // Subscribe to individual totals in each line
                SubscribeToTotals(line.LineTotals);
            }
        }

        private void UnsubscribeFromDocumentLines(IEnumerable<IDocumentLine> lines)
        {
            foreach (var line in lines)
            {
                if (line is INotifyPropertyChanged notifyLine)
                {
                    notifyLine.PropertyChanged -= DocumentLine_PropertyChanged;
                }

                if (line.LineTotals is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged -= LineTotals_CollectionChanged;
                }

                // Unsubscribe from individual totals in each line
                UnsubscribeFromTotals(line.LineTotals);
            }
        }

        private void SubscribeToTotals(IEnumerable<ITotal> totals)
        {
            foreach (var total in totals)
            {
                if (total is INotifyPropertyChanged notifyTotal)
                {
                    notifyTotal.PropertyChanged += Total_PropertyChanged;
                }
            }
        }

        private void UnsubscribeFromTotals(IEnumerable<ITotal> totals)
        {
            foreach (var total in totals)
            {
                if (total is INotifyPropertyChanged notifyTotal)
                {
                    notifyTotal.PropertyChanged -= Total_PropertyChanged;
                }
            }
        }
    }
}