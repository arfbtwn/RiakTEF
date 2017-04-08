using System;
using System.Collections.Generic;
using RiakClient.Commands.TS;

namespace RiakTEF
{
    using Serialization;

    using _TimeSpan = TimeSpan;
    using _Guid     = Guid;
    using _DateTime = DateTime;

    public partial class Serializers
    {
        /// <summary>
        /// Base class for a column serializer that supports both read and write
        /// </summary>
        public abstract class Duplex : IColumnSerializer
        {
            protected abstract bool Supported(IColumn column);

            public bool Read (IColumn column) => Supported(column);
            public bool Write(IColumn column) => Supported(column);

            public abstract object Read (Cell   cell,  IColumn column);
            public abstract Cell   Write(object value, IColumn column);
        }

        /// <summary>
        /// The default serializer, supports 1:1 mapping from column
        /// to .NET <see cref="Type"/>
        /// </summary>
        public sealed class Direct : Duplex
        {
            readonly static IReadOnlyDictionary<Type, ColumnType> All;

            static Direct()
            {
                All = new Dictionary<Type, ColumnType>
                {
                    { typeof(string),     ColumnType.Varchar   },

                    { typeof(bool?),      ColumnType.Boolean   },
                    { typeof(int?),       ColumnType.SInt64    },
                    { typeof(long?),      ColumnType.SInt64    },
                    { typeof(short?),     ColumnType.SInt64    },
                    { typeof(double?),    ColumnType.Double    },
                    { typeof(float?),     ColumnType.Double    },
                    { typeof(decimal?),   ColumnType.Double    },
                    { typeof(_DateTime?), ColumnType.Timestamp },

                    { typeof(bool),       ColumnType.Boolean   },
                    { typeof(int),        ColumnType.SInt64    },
                    { typeof(long),       ColumnType.SInt64    },
                    { typeof(short),      ColumnType.SInt64    },
                    { typeof(double),     ColumnType.Double    },
                    { typeof(float),      ColumnType.Double    },
                    { typeof(decimal),    ColumnType.Double    },
                    { typeof(_DateTime),  ColumnType.Timestamp },

                    { typeof(byte[]),     ColumnType.Blob      }
                };
            }

            protected override bool Supported(IColumn column)
            {
                ColumnType type;

                if (All.TryGetValue(column.Target, out type))
                {
                    return type == column.Type;
                }

                return false;
            }

            public override object Read(Cell cell, IColumn column)
            {
                switch (cell.ValueType)
                {
                    case ColumnType.Varchar:   return cell.ValueAsString;
                    case ColumnType.Boolean:   return cell.ValueAsBoolean;
                    case ColumnType.Double:    return cell.ValueAsDouble;
                    case ColumnType.SInt64:    return cell.ValueAsLong;
                    case ColumnType.Blob:      return cell.ValueAsBytes;
                    case ColumnType.Timestamp: return cell.ValueAsDateTime;
                    case ColumnType.Null:      return null;
                    default:                   return cell.Value;
                }
            }

            public override Cell Write(object value, IColumn column)
            {
                return new Cell(value, column.Type);
            }
        }

        /// <summary>
        /// Converts <see cref="Nullable{Guid}"/> assignable types from
        /// <see cref="ColumnType.Blob"/> and <see cref="ColumnType.Varchar"/>
        /// columns
        /// </summary>
        public sealed class Guid : Duplex
        {
            protected override bool Supported(IColumn column)
            {
                switch(column.Type)
                {
                    case ColumnType.Blob:
                    case ColumnType.Varchar:
                        return typeof(_Guid?).IsAssignableFrom(column.Target);
                }

                return false;
            }

            public override object Read(Cell cell, IColumn column)
            {
                switch(cell.ValueType)
                {
                    case ColumnType.Varchar: return _Guid.Parse(cell.ValueAsString);
                    case ColumnType.Blob:    return new _Guid(cell.ValueAsBytes);
                }

                throw new ArgumentException("Unsupported read: " + cell.ValueType);
            }

            public override Cell Write(object value, IColumn column)
            {
                var guid = (_Guid?) value;

                switch(column.Type)
                {
                    case ColumnType.Varchar: return new Cell(guid?.ToString(),    ColumnType.Varchar);
                    case ColumnType.Blob:    return new Cell(guid?.ToByteArray(), ColumnType.Blob);
                }

                throw new ArgumentException("Unsupported write: " + column.Type);
            }
        }

        /// <summary>
        /// Reads and writes <see cref="Nullable{TimeSpan}"/> to and from <see cref="ColumnType.SInt64"/> columns
        /// </summary>
        public sealed class TimeSpan : Duplex
        {
            protected override bool Supported(IColumn column)
            {
                switch (column.Type)
                {
                    case ColumnType.SInt64:
                        return typeof(_TimeSpan?).IsAssignableFrom(column.Target);
                }

                return false;
            }

            public override object Read(Cell cell, IColumn column)
            {
                return _TimeSpan.FromTicks(cell.ValueAsLong);
            }

            public override Cell Write(object value, IColumn column)
            {
                var ts = (_TimeSpan?) value;
                return ts.HasValue ? new Cell(ts.Value.Ticks) : Cell.Null;
            }
        }
    }
}
