using System.Collections.Generic;
using System.Linq;
using RiakClient.Commands.TS;

namespace RiakTEF.Initializers
{
    public class IfNotExists : Delegate
    {
        class _Table
        {
            public string Table  { get; set; }
            public string Active { get; set; }
        }

        static readonly Serialization.Extractor Extractor;

        static IfNotExists()
        {
            var entity = Schema.Create().Entity<_Table>().Auto();

            Extractor = new Serialization.Extractor(entity.Type, entity.Columns.ToArray());
        }

        readonly Table _action;

        public IfNotExists(IInitializer inner, Table action)
            : base(inner)
        {
            _action = action;
        }

        public IfNotExists(Table action)
        {
            _action = action;
        }

        public override void Initialize(DbContext context)
        {
            base.Initialize(context);

            var query = new Query(new QueryOptions("_all")
            {
                Query = "SHOW TABLES"
            });

            context.Database.Query(query);

            var tables = new Dictionary<string, _Table>();

            foreach (var row in query.Response.Value)
            {
                var table = (_Table) Extractor.Deserialize(row);

                tables.Add(table.Table, table);
            }

            foreach (var entity in context.Schema.Entities)
            {
                if (tables.ContainsKey(entity.Table))
                {
                    continue;
                }

                _action.Initialize(context, entity);
            }
        }
    }
}
