using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RiakClient.Commands.TS;

namespace RiakTEF.Low
{
    using TableMap = ConcurrentDictionary<IEntity, ITable>;

    class Batcher
    {
        readonly TableMap     _tables;
        readonly ISerializers _serializers;

        readonly ConcurrentDictionary<IEntity, Store.Builder>  _store  = new ConcurrentDictionary<IEntity, Store.Builder>();
        readonly ConcurrentDictionary<object,  Delete.Builder> _delete = new ConcurrentDictionary<object,  Delete.Builder>();

        public Batcher(TableMap tables, ISerializers serializers)
        {
            _tables      = tables;
            _serializers = serializers;
        }

        IEntity _Entity(object item) => _tables.Keys.First(x => x.Type == item.GetType());

        Store.Builder _Store(IEntity e)
        {
            var t = _tables[e];
            var s = _serializers.Get(e);

            return new Store.Builder().WithTable(t.Name).WithColumns(s.Columns);
        }

        void _Store(object item)
        {
            var e = _Entity(item);
            var c = _store.GetOrAdd(e, _Store);
            var s = _serializers.Get(e);
            var r = s.Serialize(item);

            c.WithRow(r);
        }

        void _Delete(object item)
        {
            var e = _Entity(item);
            var t = _tables[e];
            var s = _serializers.Get(e);
            var c = _delete[item] = new Delete.Builder();
            var k = s.Key(item);

            c.WithTable(t.Name).WithKey(k);
        }

        public void Add(State state, object item)
        {
            if (state.HasFlag(State.Transient)) return;

            if (state.HasFlag(State.Added) || state.HasFlag(State.Updated))
            {
                _Store(item);
            }

            if (state.HasFlag(State.Deleted))
            {
                _Delete(item);
            }
        }

        public IEnumerable<Store>  Store  => _store .Values.Select(x => x.Build());
        public IEnumerable<Delete> Delete => _delete.Values.Select(x => x.Build());
    }
}
