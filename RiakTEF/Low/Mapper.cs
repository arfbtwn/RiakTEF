using System;
using RiakClient.Commands.TS;

namespace RiakTEF.Low
{
    /*
     * TODO: Consider a dynamic system
     */
    class Mapper
    {
        static readonly Type tstring   = typeof(string);
        static readonly Type tnlong    = typeof(long?);
        static readonly Type tnint     = typeof(int?);
        static readonly Type tnshort   = typeof(short?);
        static readonly Type tnbool    = typeof(bool?);
        static readonly Type tndouble  = typeof(double?);
        static readonly Type tnfloat   = typeof(float?);
        static readonly Type tndecimal = typeof(decimal?);
        static readonly Type tndate    = typeof(DateTime?);
        static readonly Type tntime    = typeof(TimeSpan?);
        static readonly Type tnguid    = typeof(Guid?);
        static readonly Type tabyte    = typeof(byte[]);

        static bool _(Type a, Type b) => a == b || b.IsAssignableFrom(a);

        public ColumnType Map(Type type)
        {
            if      (_(type, tstring))   return ColumnType.Varchar;
            else if (_(type, tnlong))    return ColumnType.SInt64;
            else if (_(type, tnint))     return ColumnType.SInt64;
            else if (_(type, tnshort))   return ColumnType.SInt64;
            else if (_(type, tntime))    return ColumnType.SInt64;
            else if (_(type, tnbool))    return ColumnType.Boolean;
            else if (_(type, tndouble))  return ColumnType.Double;
            else if (_(type, tnfloat))   return ColumnType.Double;
            else if (_(type, tndecimal)) return ColumnType.Double;
            else if (_(type, tnguid))    return ColumnType.Varchar;
            else if (_(type, tndate))    return ColumnType.Timestamp;
            else if (_(type, tabyte))    return ColumnType.Blob;
            else                         return ColumnType.Null;
        }
    }
}