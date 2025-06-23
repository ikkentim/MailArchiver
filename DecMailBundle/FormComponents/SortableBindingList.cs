using System.ComponentModel;

namespace DecMailBundle.FormComponents;

public class SortableBindingList<T> : BindingList<T> where T : class
{
    private bool _isSorted;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;
    private PropertyDescriptor? _sortProperty;
 
    public SortableBindingList()
    {
    }
 
    public SortableBindingList(IList<T> list)
        :base(list)
    {
    }
 
    protected override bool SupportsSortingCore => true;

    protected override bool IsSortedCore => _isSorted;

    protected override ListSortDirection SortDirectionCore => _sortDirection;

    protected override PropertyDescriptor? SortPropertyCore => _sortProperty;

    protected override void RemoveSortCore()
    {
        _sortDirection = ListSortDirection.Ascending;
        _sortProperty = null;
        _isSorted = false; 
    }
 
    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
    {
        _sortProperty = prop;
        _sortDirection = direction;

        if (Items is not List<T> list) return;
 
        list.Sort(Compare);
 
        _isSorted = true;
        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }
 
 
    private int Compare(T lhs, T rhs)
    {
        var result = OnComparison(lhs, rhs);
        //invert if descending
        if (_sortDirection == ListSortDirection.Descending)
            result = -result;
        return result;
    }

    private int OnComparison(T? lhs, T? rhs)
    {
        if (_sortProperty == null)
        {
            //no property to sort by, treat as equal
            return 0;
        }

        var lhsValue = lhs == null ? null : _sortProperty.GetValue(lhs);
        var rhsValue = rhs == null ? null : _sortProperty.GetValue(rhs);
        if (lhsValue == null)
        {
            return (rhsValue == null) ? 0 : -1; //nulls are equal
        }

        if (rhsValue == null)
        {
            return 1; //first has value, second doesn't
        }

        if (lhsValue is IComparable comparable)
        {
            return comparable.CompareTo(rhsValue);
        }

        if (lhsValue.Equals(rhsValue))
        {
            return 0; //both are the same
        }

        //not comparable, compare ToString
        return string.Compare(lhsValue.ToString(), rhsValue.ToString(), StringComparison.Ordinal);
    }
}