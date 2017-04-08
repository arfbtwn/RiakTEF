using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using RiakTEF.Models;

namespace RiakTEF.Linq
{
    using IOrder = IReadOnlyCollection<Tuple<Expression, Sort>>;
    using Order  = List<Tuple<Expression, Sort>>;
    using All    = List<BinaryExpression>;
    using Where  = BinaryExpression;
    using Group  = Expression;
    using Sum    = Expression;
    using Avg    = Expression;
    using Min    = Expression;
    using Max    = Expression;

    class Analyzer : ExpressionVisitor
    {
        Expression _root;

        readonly Order _order = new Order();
        readonly All   _where = new All();

        public Where  Where => _where.Aggregate(Expression.AndAlso);
        public IOrder Order => _order;

        public Group  Group  { get; private set; }
        public int?   Offset { get; private set; }
        public int?   Limit  { get; private set; }

        public bool   Count  { get; private set; }
        public Sum    Sum    { get; private set; }
        public Avg    Avg    { get; private set; }
        public Min    Min    { get; private set; }
        public Max    Max    { get; private set; }

        public Analyzer Analyze(Expression expression)
        {
            _root = expression;

            Reset();

            Visit(expression);

            return this;
        }

        void Reset()
        {
            _where.Clear();
            _order.Clear();

            Count = false;

            Offset = Limit = null;
            Sum = Avg = Min = Max = null;
        }

        ArgumentException _(string message)
        {
            return new ArgumentException($"Query error: ({_root})\n{message}");
        }

        Expression _Queryable(MethodCallExpression node)
        {
            switch(node.Method.Name)
            {
                case "Where":
                    break;
                case "First":
                case "FirstOrDefault":
                    Limit = 1;
                    break;
                case "Single":
                case "SingleOrDefault":
                    break;
                case "OrderBy":
                case "ThenBy":
                    _order.Add(Tuple.Create(node.Arguments[1], new Sort(Models.Order.Asc)));
                    break;
                case "OrderByDescending":
                case "ThenByDescending":
                    _order.Add(Tuple.Create(node.Arguments[1], new Sort(Models.Order.Desc)));
                    break;
                case "Skip":
                    Offset = (int) ((ConstantExpression) node.Arguments[1]).Value;
                    break;
                case "Take":
                    Limit  = (int) ((ConstantExpression) node.Arguments[1]).Value;
                    break;
                case "GroupBy":
                    Group = node.Arguments[1];
                    break;
                case "Count":
                    Count = true;
                    break;
                case "Sum":
                    Sum = node.Arguments[1];
                    break;
                case "Average":
                    Avg = node.Arguments[1];
                    break;
                case "Min":
                    Min = node.Arguments[1];
                    break;
                case "Max":
                    Max = node.Arguments[1];
                    break;
                default:
                    throw _("Unsupported method call: " + node);
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            base.VisitMethodCall(node);

            if (typeof(Queryable) == node.Method.DeclaringType)
            {
                return _Queryable(node);
            }

            throw _("Unsupported method call: " + node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _where.Add(node);

            return node;
        }
    }
}