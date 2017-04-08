using System.Collections;
using System.Collections.Generic;

namespace RiakTEF.Models
{
    using PathMap   = Dictionary<Path, int>;
    using ColumnMap = SortedDictionary<int, IColumn>;

    class Columns : IColumns
    {
        readonly PathMap   _order   = new PathMap();
        readonly ColumnMap _columns = new ColumnMap();

        public IColumn this[Path path] =>_columns[_order[path]];

        public void Add(IColumn column)
        {
            int order;
            if (_order.TryGetValue(column.Path, out order))
            {
                _columns[order] = column;
            }
            else
            {
                _columns.Add(_order.Count, column);
                _order.Add(column.Path, _order.Count);
            }
        }

        public void Remove(Path path)
        {
            int order;
            if (!_order.TryGetValue(path, out order))
            {
                return;
            }
            _columns.Remove(order);
        }

        public int Count => _columns.Count;

        public IEnumerator<IColumn> GetEnumerator() => _columns.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
