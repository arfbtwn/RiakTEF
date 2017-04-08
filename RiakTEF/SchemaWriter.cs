using System.Linq;
using System.Text;

namespace RiakTEF
{
    /// <summary>
    /// Writes CREATE TABLE statements for entities
    /// </summary>
    public class SchemaWriter : ISchemaWriter
    {
        static void Write(IEntity e, StringBuilder sb)
        {
            sb.Append("CREATE TABLE ")
              .Append(e.Table)
              .AppendLine(" (");

            foreach (var column in e.Columns)
            {
                var type = column.Type.ToString().ToUpper();

                sb.Indent(1)
                  .Append(column.Column())
                  .Append(' ')
                  .Append(type);

                if (!column.Nullable) sb.Append(" NOT NULL");

                sb.AppendLine(",");
            }

            sb.AppendLine();

            var partk  = e.Key.Partition.ToList();
            var localk = partk.Union(e.Key.Local).ToList();

            var parts  = partk .Select(x => e.Columns.Single(x));
            var locals = localk.Select(x => e.Columns.Single(x));

            var pstr = string.Join(", ", parts .Select(x => x.Partition()));
            var lstr = string.Join(", ", locals.Select(x => x.Local()));

            sb.Indent(1).AppendLine("PRIMARY KEY (");

            sb.Indent(2).Append("(").Append(pstr).AppendLine("),");

            sb.Indent(2).AppendLine(lstr)
              .Indent(1).AppendLine(")");

            sb.AppendLine(");");
        }

        public string Write(ISchema schema)
        {
            var sb = new StringBuilder();

            foreach (var e in schema.Entities)
            {
                Write(e, sb);
            }

            return sb.ToString();
        }

        public string Write(IEntity entity)
        {
            var sb = new StringBuilder();

            Write(entity, sb);

            return sb.ToString();
        }
    }
}