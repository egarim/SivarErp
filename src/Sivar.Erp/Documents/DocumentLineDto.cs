using Sivar.Erp.Documents;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class DocumentLineDto : IDocumentLine, INotifyPropertyChanged
{
    private IItem _item;
    private string _lineItem;
    private decimal _amount;
    private ObservableCollection<ITotal> _lineTotals = new();
    
    public IItem Item
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
    
    public string LineItem
    {
        get => _lineItem;
        set
        {
            if (_lineItem != value)
            {
                _lineItem = value;
                OnPropertyChanged();
            }
        }
    }
    
    public decimal Amount
    {
        get => _amount;
        set
        {
            if (_amount != value)
            {
                _amount = value;
                OnPropertyChanged();
            }
        }
    }
    
    public IList<ITotal> LineTotals
    {
        get => _lineTotals;
        set
        {
            // Same pattern as in DocumentDto
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}