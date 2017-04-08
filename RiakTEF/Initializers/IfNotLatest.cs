using System;
using System.Linq;
using RiakClient.Commands.TS;

namespace RiakTEF.Initializers
{
    public sealed class IfNotLatest : Delegate
    {
        class Description
        {
            public string Column     { get; set; }
            public string Type       { get; set; }
            public bool   IsNull     { get; set; }
            public long?  PrimaryKey { get; set; }
            public long?  LocalKey   { get; set; }
            public long?  Interval   { get; set; }
            public string Unit       { get; set; }
            public object Unknown    { get; set; }

            public ColumnType ColumnType() => (ColumnType) Enum.Parse(typeof(ColumnType), Type, true);

            public override string ToString()
            {
                return $"{Column} ({Type}, Nullable: {IsNull})";
            }
        }

        readonly static Serialization.Extractor Extractor;

        static IfNotLatest()
        {
            var entity = Schema.Create().Entity<Description>().Auto();

            Extractor = new Serialization.Extractor(entity.Type, entity.Columns.ToArray());
        }

        readonly Table _action;

        public IfNotLatest(Table action)
        {
            _action = action;
        }

        public IfNotLatest(IInitializer inner, Table action)
            : base(inner)
        {
            _action = action;
        }

        public override void Initialize(DbContext context)
        {
            base.Initialize(context);

            foreach (var entity in context.Schema.Entities)
            {
                var match = true;

                var query = new Query(new QueryOptions(entity.Table)
                {
                    Query = "DESCRIBE " + entity.Table
                });

                context.Database.Query(query);

                var columns = query.Response.Value.Select(Extractor.Deserialize).Cast<Description>().ToArray();

                match &= columns.Length == entity.Columns.Count;

                if (match)
                {
                    var zip = entity.Columns.Zip(columns, Tuple.Create);

                    foreach (var pair in zip)
                    {
                        match &= pair.Item1.Name == pair.Item2.Column;
                        match &= pair.Item1.Type == pair.Item2.ColumnType();
                    }
                }

                if (!match)
                {
                    _action.Initialize(context, entity);
                }
            }
        }
    }
}
