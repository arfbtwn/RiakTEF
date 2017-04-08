using System.Collections.Generic;
using System.Linq.Expressions;

using RiakTEF.Models;

namespace RiakTEF.Visitors
{
    class KeyExtractor : ExpressionVisitor
    {
        readonly List<Path> _arguments = new List<Path>();

        public List<Path> Extract(Expression e)
        {
            Visit(e);
            return _arguments;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _arguments.Add(new PathExtractor().Extract(node));

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            foreach (var arg in node.Arguments)
            {
                _arguments.Add(new PathExtractor().Extract(arg));
            }

            return node;
        }
    }
}
