using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace RiakTEF.Models
{
    /// <summary>
    /// An object <see cref="PropertyInfo"/> path
    /// </summary>
    public sealed class Path : IReadOnlyList<PropertyInfo>
    {
        public static bool operator==(Path lhs, Path rhs)
        {
            return lhs?.Equals(rhs) ?? rhs?.Equals(lhs) ?? true;
        }

        public static bool operator!=(Path lhs, Path rhs) => !(lhs == rhs);

        public static implicit operator Path(Collection<PropertyInfo> chain) => new Path(chain);
        public static implicit operator Path(List<PropertyInfo> chain)       => new Path(chain);

        readonly List<PropertyInfo> _chain = new List<PropertyInfo>();

        readonly Lazy<int> _hash;

        PropertyInfo _last;

        public PropertyInfo First => _chain.FirstOrDefault();
        public PropertyInfo Last  => _last;

        internal Path()
        {
            _hash = new Lazy<int>(_Hash);
        }

        public Path(IEnumerable<PropertyInfo> chain) : this()
        {
            foreach (var link in chain)
            {
                Add(link);
            }
        }

        int _Hash()
        {
            return this.Aggregate(0, (curr, item) => curr ^ item.GetHashCode());
        }

        internal void Add(PropertyInfo link)
        {
            if (null == link)
            {
                throw new ArgumentNullException(nameof(link));
            }

            var expected = _last?.PropertyType ?? link.DeclaringType;

            if (expected != link.DeclaringType)
            {
                throw new ArgumentException($"{expected} != {link.DeclaringType}", nameof(link));
            }

            _chain.Add(link);

            _last = link;
        }

        public override string ToString()
        {
            return string.Join(".", _chain.Select(x => x.Name));
        }

        public override bool Equals(object obj)
        {
            return obj is IEnumerable<PropertyInfo> && this.SequenceEqual((IEnumerable<PropertyInfo>) obj);
        }

        public override int GetHashCode() => _hash.Value;

        public int Count => _chain.Count;
        public PropertyInfo this[int index] => _chain[index];

        public IEnumerator<PropertyInfo> GetEnumerator() => _chain.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}