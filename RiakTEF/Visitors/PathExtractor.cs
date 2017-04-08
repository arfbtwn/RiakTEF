using System;
using System.Linq.Expressions;
using System.Reflection;

using RiakTEF.Models;

namespace RiakTEF.Visitors
{
    class PathExtractor : ExpressionVisitor
    {
        readonly Path _path = new Path();

        public Path Extract(Expression e)
        {
            Visit(e);
            return _path;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var pi = node.Member as PropertyInfo;

            if (null == pi) throw new ArgumentException();

            base.VisitMember(node);

            _path.Add(pi);

            return node;
        }
    }
}