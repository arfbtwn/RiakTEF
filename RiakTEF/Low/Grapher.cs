using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace RiakTEF.Low
{
    class Grapher : IEnumerable<Type>
    {
        readonly ISet<Type> _end;

        public Grapher()
        {
            _end = new HashSet<Type>();
        }

        public Grapher(IEnumerable<Type> ends)
        {
            _end = new HashSet<Type>(ends);
        }

        public void Add(Type type) => _end.Add(type);

        public virtual IEnumerable<IReadOnlyList<PropertyInfo>> Graph(Type type, IReadOnlyList<PropertyInfo> prefix = null)
        {
            Type nullt;

            if      (type.IsPrimitive)    yield return prefix;

            else if (_end.Contains(type)) yield return prefix;

            else if (type.IsNullable(out nullt))
            {
                foreach (var recursion in Graph(nullt, prefix))
                {
                    yield return recursion;
                }
            }

            else
            {
                foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var sub = new List<PropertyInfo>(prefix ?? new List<PropertyInfo>()) { p };

                    foreach (var recursion in Graph(p.PropertyType, sub))
                    {
                        yield return recursion;
                    }
                }
            }
        }

        public IEnumerator<Type> GetEnumerator() => _end.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}