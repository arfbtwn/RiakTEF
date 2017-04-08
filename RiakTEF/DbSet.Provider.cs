using System.Collections;
using System.Linq;
using System.Linq.Expressions;

using RiakTEF.Linq;
using RiakTEF.Visitors;

namespace RiakTEF
{
    public partial class DbSet<T> : IQueryProvider
    {
        protected virtual DbSet<T> Clone() => (DbSet<T>) MemberwiseClone();

        protected virtual string Generate(Expression expression)
        {
            return new Parser(Table).Parse(expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var clone = Clone();
            clone.Expression = expression;
            return clone;
        }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>) Provider.CreateQuery(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            Assertion.IsValidFor<T>(expression);

            return Execute(expression, false);
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            Assertion.IsValidFor<T>(expression);

            var ienum = typeof(IEnumerable).IsAssignableFrom(typeof(TResult));

            return (TResult) Execute(expression, ienum);
        }

        object Execute(Expression expression, bool isEnum)
        {
            if (expression is ConstantExpression)
            {
                return Table.All().Cast<T>();
            }

            var select = Generate(expression);
            var result = Table.Query(select).AsQueryable().Cast<T>();

            var tree = new NodeReplace(this, result).Visit(expression);

            return isEnum ? result.Provider.CreateQuery(tree)
                          : result.Provider.Execute    (tree);
        }
    }
}