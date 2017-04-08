using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiakClient.Commands.TS;

using _Row    = RiakClient.Commands.TS.Row;
using _Column = RiakClient.Commands.TS.Column;

namespace RiakTEF.Low
{
    using Serialization;

    using IColumns = ICollection<_Column>;

    /// <summary>
    /// A component that de/serializes a <see cref="_Row"/> into an entity
    /// </summary>
    public interface IRowSerializer
    {
        IColumns Columns { get; }

        _Row   Key        (object item);
        _Row   Serialize  (object item);
        object Deserialize(_Row   row);
    }

    class Table : ITable
    {
        string _name;

        public Table(IDatabase database, IEntity entity, IRowSerializer serializer = null)
        {
            Database   = database;
            Entity     = entity;
            Serializer = serializer ?? new Row(entity);
        }

        public IDatabase      Database   { get; }
        public IEntity        Entity     { get; }
        public IRowSerializer Serializer { get; }

        public string Name => _name ?? Entity.Table;

        public Table Remap(string table)
        {
            _name = table;
            return this;
        }

        IColumns Columns => Serializer.Columns;

        Store _Store(params object[] rows)
        {
            var b = new Store.Builder()
                             .WithTable(Name)
                             .WithColumns(Columns);

            foreach (var item in rows)
            {
                b.WithRow(Serializer.Serialize(item));
            }

            return b.Build();
        }

        Query _Query(string sql)
        {
            return new Query.Builder()
                            .WithTable(Name)
                            .WithQuery(sql)
                            .Build();
        }

        Delete _Delete(object item)
        {
            var c = new Delete.Builder();
            var k = Serializer.Key(item);

            c.WithTable(Name).WithKey(k);

            return c.Build();
        }

        IEnumerable Extract(IEnumerable<_Row> rows) => rows.Select(Extract);
        object      Extract(_Row row)               => Serializer.Deserialize(row);

        public IEnumerable Query(string sql)
        {
            var c = _Query(sql);

            Database.Query(c);

            return Extract(c.Response.Value);
        }

        public async Task<IEnumerable> QueryAsync(string sql)
        {
            var c = _Query(sql);

            await Database.QueryAsync(c).ConfigureAwait(false);

            return Extract(c.Response.Value);
        }

        public void Store(params object[] items)
        {
            var c = _Store(items);

            Database.Store(c);
        }

        public Task StoreAsync(params object[] items)
        {
            var c = _Store(items);

            return Database.StoreAsync(c);
        }

        public void Delete(object item)
        {
            var c = _Delete(item);

            Database.Execute(c);
        }

        public Task DeleteAsync(object item)
        {
            var c = _Delete(item);

            return Database.ExecuteAsync(c);
        }

        public IEnumerable All()
        {
            var c = new ListKeys(new ListKeysOptions(Name));

            Database.Execute(c);

            foreach (var key in c.Response.Value)
            {
                var get = new Get.Builder()
                                 .WithTable(Name)
                                 .WithKey(key)
                                 .Build();

                Database.Execute(get);

                yield return Extract(get.Response.Value.First());
            }
        }
    }
}
