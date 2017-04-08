using System.Linq.Expressions;

namespace RiakTEF.Visitors
{
    class NodeReplace : ExpressionVisitor
    {
        readonly object _target;
        readonly object _value;

        public NodeReplace(object target, object value)
        {
            _target = target;
            _value  = value;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (_target == node.Value)
            {
                return Expression.Constant(_value, _value.GetType());
            }

            return node;
        }
    }
}