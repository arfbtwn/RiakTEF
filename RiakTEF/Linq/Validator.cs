using System;
using System.Linq;
using System.Linq.Expressions;

namespace RiakTEF.Linq
{
    class Validator : ExpressionVisitor
    {
        Expression _root;

        ArgumentException _(string message)
        {
            return new ArgumentException($"Query error: ({_root})\n{message}");
        }

        public override Expression Visit(Expression node)
        {
            _root = node;

            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (typeof(Queryable) != node.Method.DeclaringType)
            {
                throw _("Unsupported method call: " + node);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch(node.NodeType)
            {
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    break;
                default:
                    throw _("Unsupported binary operator: " + node);
            }

            return base.VisitBinary(node);
        }
    }
}
