using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using RiakClient.Commands.TS;

using RiakTEF.Visitors;

namespace RiakTEF.Models
{
    [DebuggerDisplay("{_name}: {Target}")]
    public class Column : IColumn, IFluent
    {
        static ColumnType Parse(Type type) => (ColumnType) Enum.Parse(typeof(ColumnType), type.Name);
        static string     Snake(Path path) => string.Join("_", path.Select(x => x.Name.ToLower()));
        static bool       Break(Path path) => path.Any(x => x.PropertyType.IsNullAssignable());

        protected string     _name;
        protected ColumnType _type;
        protected bool       _nullable;
        protected Sort?      _sort;
        protected Quantum?   _quantum;

        public Column(Path path, ColumnType? type = null)
        {
            Path = path;

            _type     = type ?? Parse(GetType());
            _name     = Snake(path);
            _nullable = Break(path);
        }

        public Column(Expression expr, ColumnType? type = null)
            : this(new PathExtractor().Extract(expr), type)
        {
        }

        protected Path Path { get; }
        protected Type Source => Path.First.DeclaringType;
        protected Type Target => Path.Last.PropertyType;

        protected virtual string     Name     => _name;
        protected virtual ColumnType Type     => _type;
        protected virtual bool       Nullable => _nullable;
        protected virtual Sort?      Sort     => _sort;
        protected virtual Quantum?   Quantum  => _quantum;

        Type       IColumn.Source   => Source;
        Type       IColumn.Target   => Target;
        Path       IColumn.Path     => Path;

        string     IColumn.Name     => Name;
        ColumnType IColumn.Type     => Type;
        bool       IColumn.Nullable => Nullable;
        Sort?      IColumn.Sort     => Sort;
        Quantum?   IColumn.Quantum  => Quantum;

        void IFluent.Name(string name)
        {
            _name = name;
        }

        void IFluent.Nullable(bool nullable)
        {
            _nullable = nullable;
        }

        public override string ToString()
        {
            return $"{Name} ({Type}, Nullable: {Nullable})";
        }
    }
}
