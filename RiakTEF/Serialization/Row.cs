using System;
using System.Collections.Generic;
using System.Linq;
using RiakClient.Commands.TS;

using RiakTEF.Low;

using _Column = RiakClient.Commands.TS.Column;
using _Row    = RiakClient.Commands.TS.Row;

namespace RiakTEF.Serialization
{
    using IColumns = ICollection<_Column>;

    class Row : Extractor, IRowSerializer
    {
        readonly IEntity _entity;

        public Row(IEntity entity) : base(entity.Type, entity.Columns.ToArray())
        {
            _entity = entity;

            Columns = entity.Columns.Select(Interop.AsRiak).ToArray();
        }

        public Row(IEntity entity, Func<object> constructor) : base(constructor, entity.Columns.ToArray())
        {
            _entity = entity;

            Columns = entity.Columns.Select(Interop.AsRiak).ToArray();
        }

        public new IColumns Columns { get; }

        protected virtual Cell Write(object obj, IColumn spec)
        {
            var converter = Converters.Last(x => x.Write(spec));

            return converter.Write(obj, spec);
        }

        public _Row Key(object item)
        {
            var key = _entity.Key.Partition.Union(_entity.Key.Local);

            return new _Row(key.Select(path =>
            {
                var spec = _entity.Columns[path];
                var axs  = Properties[path];
                var data = axs.Get(item);

                return Write(data, spec);
            }));
        }

        public _Row Serialize(object item)
        {
            var row = new _Row(_entity.Columns.Select(column =>
            {
                var spec = _entity.Columns[column.Path];
                var axs  = Properties[column.Path];
                var data = axs.Get(item);

                return Write(data, spec);
            }));

            _Deserialize(row, item);

            return row;
        }
    }
}
