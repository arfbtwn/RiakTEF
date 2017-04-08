using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RiakTEF.Visitors
{
    static class Eval
    {
        public static Expression Evaluate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return expression;
            }

            var lambda = Expression.Lambda(Expression.Convert(expression, typeof(object)));
            var fn = (Func<object>) lambda.Compile();

            if (typeof(Expression).IsAssignableFrom(expression.Type))
            {
                return Expression.Quote((Expression) fn());
            }
            return Expression.Constant(fn(), expression.Type);
        }

        public static Expression Local(Expression expression)
        {
            return new SubTree(new Nomination().Nominate(expression)).Eval(expression);
        }

        class SubTree : ExpressionVisitor
        {
            readonly HashSet<Expression> _targets;

            public SubTree(HashSet<Expression> targets)
            {
                _targets = targets;
            }

            public Expression Eval(Expression target)
            {
                return Visit(target);
            }

            public override Expression Visit(Expression node)
            {
                if (_targets.Contains(node))
                {
                    return Evaluate(node);
                }
                else
                {
                    return base.Visit(node);
                }
            }
        }

        class Nomination : ExpressionVisitor
        {
            static bool Local(Expression e) => ExpressionType.Parameter != e.NodeType;

            readonly HashSet<Expression>    _targets = new HashSet<Expression>();
            readonly Func<Expression, bool> _matcher;

            bool _match;

            public Nomination() : this(Local) { }

            public Nomination(Func<Expression, bool> matcher)
            {
                _matcher = matcher;
            }

            public HashSet<Expression> Nominate(Expression target)
            {
                Visit(target);
                return _targets;
            }

            public override Expression Visit(Expression node)
            {
                if (node == null) return null;

                var match = _match;
                _match = true;
                base.Visit(node);

                if (_match)
                {
                    if (_matcher(node)) _targets.Add(node);
                    else                _match = false;

                    _match &= match;
                }

                return node;
            }
        }
    }
}