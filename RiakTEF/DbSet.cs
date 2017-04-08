using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using RiakTEF.Low;

namespace RiakTEF
{
    /// <summary>
    /// An object model interface to a particular table
    /// </summary>
    public interface ITable
    {
        IDatabase      Database   { get; }
        IEntity        Entity     { get; }
        IRowSerializer Serializer { get; }

        string Name { get; }

        IEnumerable Query (string sql);
        void        Store (params object[] items);
        void        Delete(object item);

        Task<IEnumerable> QueryAsync (string sql);
        Task              StoreAsync (params object[] items);
        Task              DeleteAsync(object item);

        IEnumerable All();
    }

    /// <summary>
    /// Provides a queryable object interface onto a database table
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class DbSet<T> : IQueryable<T>
    {
        protected readonly IUnit  Unit;
        protected readonly ITable Table;

        public Expression     Expression  { get; protected set; }

        public Type           ElementType => typeof(T);
        public IQueryProvider Provider    => this;

        public DbSet(ITable table, IUnit unit)
        {
            Table = table;
            Unit  = unit;

            Expression = Expression.Constant(this);
        }

        public void Add(params T[] items)
        {
            foreach (var item in items)
            {
                Unit[item] = State.Added;
            }
        }

        public void Remove(params T[] items)
        {
            foreach(var item in items)
            {
                Unit[item] = State.Deleted;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}