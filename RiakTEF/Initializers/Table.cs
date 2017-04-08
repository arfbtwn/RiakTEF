using System;
using RiakClient.Commands.TS;

namespace RiakTEF.Initializers
{
    public abstract class Table
    {
        public sealed class Create : Table
        {
            public override void Initialize(DbContext context, IEntity entity)
            {
                var writer = new SchemaWriter();
                var cmd    = new Query(new QueryOptions(entity.Table)
                {
                    Query  = writer.Write(entity)
                });

                context.Database.Execute(cmd);
            }
        }

        public sealed class Throw : Table
        {
            public override void Initialize(DbContext context, IEntity entity)
            {
                throw new NotSupportedException();
            }
        }

        public abstract void Initialize(DbContext context, IEntity entity);
    }
}