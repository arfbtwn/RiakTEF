using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RiakClient.Commands.TS;

using RiakTEF.Low;
using RiakTEF.Models;

using _Row = RiakClient.Commands.TS.Row;

namespace RiakTEF.Serialization
{
    using IColumns    = ICollection<IColumn>;
    using IProperties = IDictionary<Path, Property>;

    /// <summary>
    /// A component that deserializes a <see cref="Cell"/> into an object
    /// </summary>
    public interface IColumnSerializer
    {
        bool Read (IColumn column);
        bool Write(IColumn column);

        object Read (Cell   cell,  IColumn column);
        Cell   Write(object value, IColumn column);
    }

    class Extractor
    {
        readonly static Collection<IColumnSerializer> Default;

        static Func<object> _new(Type type) => () => Activator.CreateInstance(type);

        static Extractor()
        {
            Default = new Collection<IColumnSerializer>
            {
                new Serializers.Direct()
            };
        }

        public Extractor(Type type, IColumns columns) : this(_new(type), columns)
        {
        }

        public Extractor(Func<object> construct, IColumns columns)
        {
            Construct  = construct;
            Columns    = columns;
            Properties = _Properties();

            Converters = Default;
        }

        IProperties _Properties() => Columns.ToDictionary(x => x.Path, Interop.AsProperty);

        public Func<object> Construct  { get; }
        public IColumns     Columns    { get; }
        public IProperties  Properties { get; }

        public ICollection<IColumnSerializer> Converters { get; set; }

        protected virtual object Read(Cell cell, IColumn spec)
        {
            var converter = Converters.Last(x => x.Read(spec));

            return converter.Read(cell, spec);
        }

        protected void _Deserialize(_Row item, object instance)
        {
            var zip = Columns.Zip(item.Cells, Tuple.Create);

            foreach (var pair in zip)
            {
                var spec = pair.Item1;
                var cell = pair.Item2;

                if (cell.ValueType == ColumnType.Null) continue;

                var axs  = Properties[spec.Path];
                var data = Read(cell, spec);

                axs.Set(instance, data);
            }
        }

        public object Deserialize(_Row item)
        {
            var o = Construct();

            _Deserialize(item, o);

            return o;
        }
    }
}
