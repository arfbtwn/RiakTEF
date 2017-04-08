using RiakClient.Commands.TS;

using RiakTEF.Low;

namespace RiakTEF
{
    static class Interop
    {
        internal static Column   AsRiak    (this IColumn column) => new Column(column.Name, column.Type);

        internal static Property AsProperty(this IColumn column) => new Property(column.Path);
    }
}
