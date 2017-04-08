using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RiakTEF
{
    class _Unit : IUnit
    {
        readonly ConcurrentDictionary<object, State> _state = new ConcurrentDictionary<object, State>();

        public State this[object key]
        {
            get { return _state[key];  }
            set
            {
                if (!_state.TryAdd(key, value))
                {
                    _state[key] |= value;
                }
            }
        }

        public void Flush() => _state.Clear();

        public IEnumerator<Tuple<State, object>> GetEnumerator()
        {
            return _state.Select(x => Tuple.Create(x.Value, x.Key)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
