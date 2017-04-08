using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using RiakTEF.Low;
using RiakTEF.Serialization;

namespace RiakTEF
{
    using SerializerMap = ConcurrentDictionary<IEntity, IRowSerializer>;
    using Converters    = Collection<IColumnSerializer>;

    /// <summary>
    /// A thread safe collection of row serializers
    /// </summary>
    public partial class Serializers : ISerializers
    {
        readonly SerializerMap _serializers = new SerializerMap();
        readonly Converters    _converters  = new Converters();

        protected virtual IRowSerializer Build(IEntity entity)
        {
            return new Row(entity) { Converters = _converters };
        }

        public IRowSerializer Get(IEntity entity)
        {
            return _serializers.GetOrAdd(entity, Build);
        }

        public void Add(IColumnSerializer serializer)
        {
            _converters.Add(serializer);
        }

        public IEnumerator<IColumnSerializer> GetEnumerator() => _converters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
