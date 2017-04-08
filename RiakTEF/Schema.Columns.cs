using System.Linq.Expressions;

using RiakTEF.Models;

namespace RiakTEF
{
    public abstract class Sortable : Column, ISortable
    {
        protected Sortable(Path       path) : base(path) { }
        protected Sortable(Expression expr) : base(expr) { }

        void ISortable.Sort(Sort order) => _sort = order;
    }

    public sealed class Timestamp : Sortable, ITimestamp
    {
        public Timestamp(Path       path) : base(path) { }
        public Timestamp(Expression expr) : base(expr) { }

        void ITimestamp.Quantum(Quantum quantum) => _quantum = quantum;
    }

    public sealed class Varchar : Sortable
    {
        public Varchar(Path       path) : base(path) { }
        public Varchar(Expression expr) : base(expr) { }
    }

    public sealed class SInt64 : Sortable
    {
        public SInt64(Path       path) : base(path) { }
        public SInt64(Expression expr) : base(expr) { }
    }

    public sealed class Boolean : Column
    {
        public Boolean(Path       path) : base(path) { }
        public Boolean(Expression expr) : base(expr) { }
    }

    public sealed class Double : Column
    {
        public Double(Path       path) : base(path) { }
        public Double(Expression expr) : base(expr) { }
    }

    public sealed class Blob : Column
    {
        public Blob(Path       path) : base(path) { }
        public Blob(Expression expr) : base(expr) { }
    }
}
