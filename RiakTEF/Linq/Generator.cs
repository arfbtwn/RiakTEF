using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using RiakTEF.Models;
using RiakTEF.Visitors;

namespace RiakTEF.Linq
{
    /// <summary>
    /// Generates a Riak SELECT statement for an entity definition
    /// </summary>
    public class Generator : ExpressionVisitor
    {
        static readonly Type[] tplain =
        {
            typeof(long?),
            typeof(int?),
            typeof(short?),
            typeof(bool?),
            typeof(double?),
            typeof(float?),
            typeof(decimal?)
        };

        static bool _Plain(Type type)
        {
            return tplain.Any(x => x.IsAssignableFrom(type));
        }

        protected readonly StringBuilder Builder = new StringBuilder();

        readonly string   _table;
        readonly IColumns _columns;

        public Generator(string table, IColumns columns)
        {
            _table   = table;
            _columns = columns;
        }

        public string Generate(Expression expression)
        {
            var analysis = new Analyzer().Analyze(expression);

            Builder.Append    ("SELECT ")
                   .Append    (analysis.Count ? "COUNT(*)" : "*")
                   .Append    (" FROM ")
                   .AppendLine(_table)
                   .AppendLine("WHERE");

            Visit(analysis.Where);

            if (analysis.Order.Any())
            {
                Builder.Append(" ORDER BY ");

                foreach (var order in analysis.Order)
                {
                    var column = _columns.Single(new PathExtractor().Extract(order.Item1));

                    Builder.Append(column.Column()).Append(' ').Append(order.Item2).Append(", ");
                }

                Builder.Remove(Builder.Length - 2, 2);
            }

            if (analysis.Offset.HasValue) Builder.Append(" OFFSET ").Append(analysis.Offset);
            if (analysis.Limit .HasValue) Builder.Append(" LIMIT ") .Append(analysis.Limit);

            return Builder.ToString();
        }

        protected virtual void Append(ConstantExpression node)
        {
            if (typeof(DateTime?).IsAssignableFrom(node.Type))
            {
                Builder.Date((DateTime?) node.Value);
            }
            else if (typeof(DateTimeOffset?).IsAssignableFrom(node.Type))
            {
                Builder.Date((DateTimeOffset?) node.Value);
            }
            else if (_Plain(node.Type))
            {
                Builder.Append(node.Value);
            }
            else
            {
                Builder.Escape(node.Value);
            }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (typeof(Expression).IsAssignableFrom(node.Type))
            {
                return Visit((Expression) node.Value);
            }

            Append(node);

            return base.VisitConstant(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var members = new PathExtractor().Extract(node);

            var column = _columns.SingleOrDefault(members);

            if (null == column)
            {
#if DEBUG
                Builder.Append(node);
#else
                throw new ArgumentException("Could not map node: " + node);
#endif
            }
            else
            {
                Builder.Append(column.Column());
            }

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch(node.NodeType)
            {
                case ExpressionType.NotEqual:
                case ExpressionType.Equal:
                    var @null = new NullComparison().Extract(node);

                    if (@null.Null)
                    {
                        var column = _columns.Single(@null.Path);

                        Builder.Append(column.Column()).Append(" IS ");

                        switch(node.NodeType)
                        {
                            case ExpressionType.NotEqual: Builder.Append("NOT NULL"); break;
                            case ExpressionType.Equal:    Builder.Append("NULL");     break;
                        }

                        return node;
                    }
                    break;
            }

            if (ExpressionType.OrElse == node.NodeType) Builder.Append('(');

            Visit(node.Left);

            Builder.Append(' ');

            /*
             * TODO: We should really be checking for valid comparisons
             *
             * There are some constraints for key fields, IIRC they can only use =
             */
            switch(node.NodeType)
            {
                case ExpressionType.LessThanOrEqual:    Builder.Append("<=");  break;
                case ExpressionType.LessThan:           Builder.Append("<");   break;
                case ExpressionType.GreaterThanOrEqual: Builder.Append(">=");  break;
                case ExpressionType.GreaterThan:        Builder.Append(">");   break;
                case ExpressionType.Equal:              Builder.Append("=");   break;
                case ExpressionType.NotEqual:           Builder.Append("!=");  break;
                case ExpressionType.AndAlso:            Builder.Append("AND"); break;
                case ExpressionType.OrElse:             Builder.Append("OR");  break;
                default:
                    throw new ArgumentException("NodeType: " + node.NodeType);
            }

            Builder.Append(' ');

            Visit(node.Right);

            if (ExpressionType.OrElse == node.NodeType) Builder.Append(')');

            return node;
        }

        class NullComparison : ExpressionVisitor
        {
            public Path Path;
            public bool Null;

            public NullComparison Extract(Expression expression)
            {
                Visit(expression);
                return this;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                Path = new PathExtractor().Extract(node);
                return node;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                Null = null == node.Value;
                return node;
            }
        }
    }
}