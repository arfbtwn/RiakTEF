using System;
using System.Linq.Expressions;

namespace RiakTEF.Linq
{
    class Syntax : ExpressionVisitor
    {
        static bool IsRelative(MethodCallExpression node)
        {
            return typeof(TimeSpan) == node.Arguments[2].Type;
        }

        static Expression Start(MethodCallExpression node)
        {
            return IsRelative(node) ? Expression.SubtractChecked(node.Arguments[1], node.Arguments[2])
                                    : node.Arguments[2];
        }

        static Expression End(MethodCallExpression node)
        {
            return IsRelative(node) ? node.Arguments[1] : node.Arguments[2];
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch(node.Method.Name)
            {
                case "InRange":
                    var x     = node.Arguments[0];
                    var start = Start(node);
                    var end   = End  (node);

                    return Expression.AndAlso(
                        Expression.LessThanOrEqual(start, x),
                        Expression.LessThan       (x,     end)
                    );
                default:
                    return base.VisitMethodCall(node);
            }
        }
    }
}