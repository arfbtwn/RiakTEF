using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RiakTEF.Models;
using RiakTEF.Visitors;

namespace RiakTEF.Low
{
    class Property
    {
        static readonly MethodInfo _ChangeType = typeof(Property).GetMethod("ChangeType", BindingFlags.Static | BindingFlags.NonPublic);

        static object ChangeType(object value, Type type)
        {
            Type nullt;
            var nullable = type.IsNullable(out nullt);
            if (nullable)
            {
                if (null == value)
                {
                    return null;
                }

                type = nullt;
            }
            return Convert.ChangeType(value, type);
        }

        Func<object, object> _Get()
        {
            var first = Path.First;

            var p0    = Expression.Parameter(typeof(object), "obj");
            var @null = Expression.Constant(null, typeof(object));
            var label = Expression.Label(typeof(object));

            Expression call = Expression.Convert(p0, first.DeclaringType);

            Expression expr = null;

            foreach (var p in Path)
            {
                expr = Expression.IfThenElse(
                    Expression.ReferenceEqual(@null, call),
                    Expression.Return(label, @null),
                    call = Expression.MakeMemberAccess(call, p)
                );
            }

            var target = Expression.Label(label, Expression.Convert(call, typeof(object)));
            var block  = Expression.Block(expr, target);

            return (Func<object, object>) Expression.Lambda(block, p0).Compile();
        }

        Action<object, object> _Set()
        {
            var first = Path.First;
            var last  = Path.Last;

            var p0 = Expression.Parameter(typeof(object), "obj");
            var p1 = Expression.Parameter(typeof(object), "value");

            var pt = Expression.Constant(last.PropertyType);

            var c0 = Expression.Convert(p0, first.DeclaringType);
            var c1 = Expression.Convert(Expression.Call(_ChangeType, p1, pt), last.PropertyType);

            var @null = Expression.Constant(null);

            var block = new List<Expression>();

            var label = Expression.Label();

            Expression call = c0;

            foreach (var p in Path)
            {
                if (p == last) break;

                call = Expression.MakeMemberAccess(call, p);

                block.Add(Expression.IfThen(
                    Expression.ReferenceEqual(@null, call),
                    Expression.Assign(call, Expression.New(p.PropertyType))
                ));
            }

            block.Add(Expression.Call(call, last.SetMethod, c1));
            block.Add(Expression.Return(label));
            block.Add(Expression.Label(label));

            return (Action<object, object>) Expression.Lambda(Expression.Block(block), p0, p1).Compile();
        }

        public readonly Path Path;

        readonly Lazy<Func  <object, object>> _get;
        readonly Lazy<Action<object, object>> _set;

        public Func  <object, object> Get => _get.Value;
        public Action<object, object> Set => _set.Value;

        public Property(Path path)
        {
            Path = path;

            _get = new Lazy<Func  <object, object>>(_Get);
            _set = new Lazy<Action<object, object>>(_Set);
        }

        public Property(params PropertyInfo[] chain) : this(new Path(chain))
        {
        }

        public Property(Expression expr) : this(new PathExtractor().Extract(expr))
        {
        }

        public Type Source => Path.First.DeclaringType;
        public Type Target => Path.Last.PropertyType;

        public override string ToString()
        {
            return string.Join(".", Path.Select(x => x.Name));
        }
    }
}