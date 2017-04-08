using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RiakTEF.Models;
using RiakTEF.Visitors;

namespace RiakTEF
{
    using EntityMap = Dictionary<Type, IEntity>;
    using Paths     = List<Path>;

    using IEntities = IEnumerable<IEntity>;
    using IPaths    = IEnumerable<Path>;

    class _Schema : ISchema
    {
        readonly EntityMap _entities = new EntityMap();

        public IEntities Entities => _entities.Values;

        public IEntity<T> Entity<T>(string table)
        {
            var key = typeof(T);

            IEntity e;
            if (!_entities.TryGetValue(key, out e))
            {
                e = _entities[key] = new _Entity<T>(table);
            }

            return (IEntity<T>) e;
        }
    }

    class _PrimaryKey<T> : IPrimaryKey<T>
    {
        Paths _local;
        Paths _partition;

        IPaths IPrimaryKey.Local     => _local     ?? new Paths();
        IPaths IPrimaryKey.Partition => _partition ?? new Paths();

        IPrimaryKey<T> IPrimaryKey<T>.Local<V>(Expression<Func<T, V>> expr)
        {
            _local = new KeyExtractor().Extract(expr);
            return this;
        }

        IPrimaryKey<T> IPrimaryKey<T>.Partition<V>(Expression<Func<T, V>> expr)
        {
            _partition = new KeyExtractor().Extract(expr);
            return this;
        }
    }

    class _Entity<T> : IEntity<T>
    {
        readonly Columns        _columns = new Columns();
        readonly _PrimaryKey<T> _key     = new _PrimaryKey<T>();

        string _table;

        public _Entity(string table = null)
        {
            _table = table ?? typeof(T).Name.ToLowerInvariant();
        }

        public IEntity<T> Table(string name)
        {
            _table = name;
            return this;
        }

        public void Add(IColumn column)
        {
            _columns.Add(column);
        }

        public void Ignore<V>(Expression<Func<T, V>> expr)
        {
            _columns.Remove(new PathExtractor().Extract(expr));
        }

        public IColumns       Columns => _columns;
        public IPrimaryKey<T> Key     => _key;

        Type        IEntity.Type  => typeof(T);
        string      IEntity.Table => _table;
        IPrimaryKey IEntity.Key   => _key;
    }
}
