using System;
using System.Linq.Expressions;
using RiakClient.Commands.TS;

using RiakTEF.Models;
using RiakTEF.Profiles;

using Column = RiakTEF.Models.Column;

namespace RiakTEF
{
    public static class _SchemaSyntax
    {
        public static ISchema Register<T>(this ISchema schema, T profile) where T : IProfile
        {
            profile.Register(schema);
            return schema;
        }

        public static ISchema Register<T>(this ISchema schema) where T : IProfile, new()
        {
            return schema.Register(new T());
        }

        static T With<T>(this IEntity entity, T column) where T : IColumn
        {
            entity.Add(column);
            return column;
        }

        public static IEntity<T> Auto<T>(this IEntity<T> entity)
        {
            var mapper  = Default.Mapper;
            var grapher = Default.Grapher;

            foreach (var list in grapher.Graph(typeof(T)))
            {
                var path = new Path(list);
                var type = mapper.Map(path.Last.PropertyType);

                entity.Add(new Column(path, type));
            }
            return entity;
        }

        public static T Name<T>(this T column, string name) where T : IFluent
        {
            column.Name(name);
            return column;
        }

        public static T Nullable<T>(this T column, bool nullable = true) where T : IFluent
        {
            column.Nullable(nullable);
            return column;
        }

        public static T Sort<T>(this T column, Order order, Nulls? nulls = null) where T : ISortable
        {
            column.Sort(new Sort(order, nulls));
            return column;
        }

        public static T Quantum<T>(this T column, uint interval, Unit unit) where T : ITimestamp
        {
            column.Quantum(new Quantum(interval, unit));
            return column;
        }

        public static T Quantum<T>(this T column, uint interval, char unit) where T : ITimestamp
        {
            column.Quantum(new Quantum(interval, (Unit) unit));
            return column;
        }

        public static Column Column<T>(this IEntity<T> entity, Expression<Func<T, object>> expr, ColumnType type)
        {
            return entity.With(new Column(expr, type));
        }

        public static Varchar Column<T>(this IEntity<T> entity, Expression<Func<T, string>> expr)
        {
            return entity.With(new Varchar(expr));
        }

        public static SInt64 Column<T>(this IEntity<T> entity, Expression<Func<T, long>> expr)
        {
            return entity.With(new SInt64(expr));
        }

        public static SInt64 Column<T>(this IEntity<T> entity, Expression<Func<T, long?>> expr)
        {
            return entity.With(new SInt64(expr));
        }

        public static SInt64 Column<T>(this IEntity<T> entity, Expression<Func<T, int>> expr)
        {
            return entity.With(new SInt64(expr));
        }

        public static SInt64 Column<T>(this IEntity<T> entity, Expression<Func<T, int?>> expr)
        {
            return entity.With(new SInt64(expr));
        }

        public static Double Column<T>(this IEntity<T> entity, Expression<Func<T, double>> expr)
        {
            return entity.With(new Double(expr));
        }

        public static Double Column<T>(this IEntity<T> entity, Expression<Func<T, double?>> expr)
        {
            return entity.With(new Double(expr));
        }

        public static Double Column<T>(this IEntity<T> entity, Expression<Func<T, decimal>> expr)
        {
            return entity.With(new Double(expr));
        }

        public static Double Column<T>(this IEntity<T> entity, Expression<Func<T, decimal?>> expr)
        {
            return entity.With(new Double(expr));
        }

        public static Varchar Column<T>(this IEntity<T> entity, Expression<Func<T, Guid>> expr)
        {
            return entity.With(new Varchar(expr));
        }

        public static Varchar Column<T>(this IEntity<T> entity, Expression<Func<T, Guid?>> expr)
        {
            return entity.With(new Varchar(expr));
        }

        public static Timestamp Column<T>(this IEntity<T> entity, Expression<Func<T, DateTime>> expr)
        {
            return entity.With(new Timestamp(expr));
        }

        public static Timestamp Column<T>(this IEntity<T> entity, Expression<Func<T, DateTime?>> expr)
        {
            return entity.With(new Timestamp(expr));
        }

        public static Blob Column<T>(this IEntity<T> entity, Expression<Func<T, byte[]>> expr)
        {
            return entity.With(new Blob(expr));
        }
    }
}