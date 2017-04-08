using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RiakTEF.Models;

namespace RiakTEF
{
    static class _Writer
    {
        static readonly DateTimeOffset Epoch = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.Zero);

        internal static StringBuilder Indent(this StringBuilder sb, int level, int indent = 4)
        {
            return sb.Append(new string(' ', indent * level));
        }

        internal static StringBuilder Date(this StringBuilder sb, DateTime? date)
        {
            return sb.Date((DateTimeOffset?) date);
        }

        internal static StringBuilder Date(this StringBuilder sb, DateTimeOffset? date)
        {
            if (!date.HasValue)
            {
                throw new ArgumentNullException(nameof(date));
            }

            var val = date.Value;

            var trunc = new DateTimeOffset(
                val.Year,
                val.Month,
                val.Day,
                val.Hour,
                val.Minute,
                val.Second,
                val.Millisecond,
                val.Offset
            );

#if SELECT_ISO8601
            return sb.Append('\'').Append(trunc.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append('\'');
#else
            return sb.Append((long) (trunc - Epoch).TotalMilliseconds);
#endif
        }

        internal static StringBuilder Escape(this StringBuilder sb, object value)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return sb.Append('\'').Append(value.ToString().Replace("'", "''")).Append('\'');
        }

        internal static IColumn Single(this IEnumerable<IColumn> columns, Path path)
        {
            return columns.Single(x => path == x.Path);
        }

        internal static IColumn SingleOrDefault(this IEnumerable<IColumn> columns, Path path)
        {
            return columns.SingleOrDefault(x => path == x.Path);
        }

        internal static IEntity Single(this IEnumerable<IEntity> entities, Type type)
        {
            return entities.Single(x => type == x.Type);
        }

        internal static string Column(this IColumn column)
        {
            return column.Name.Contains(' ') ? '"' + column.Name + '"' : column.Name;
        }

        internal static string Partition(this IColumn column)
        {
            if (!column.Quantum.HasValue)
            {
                return Column(column);
            }

            var q = column.Quantum.Value;

            return $"QUANTUM({Column(column)}, {q.Interval}, '{(char) q.Unit}')";
        }

        internal static string Local(this IColumn column)
        {
            if (!column.Sort.HasValue)
            {
                return Column(column);
            }

            var sort = column.Sort.Value;

            return Column(column) + " " + sort;
        }
    }
}
