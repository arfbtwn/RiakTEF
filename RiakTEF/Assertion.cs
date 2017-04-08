using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RiakClient;
using RiakClient.Commands.TS;

namespace RiakTEF
{
    static class Assertion
    {
        static Type _telem(Type type)
        {
            var ienum = _ienum(type);

            return null == ienum ? type : ienum.GetGenericArguments()[0];
        }

        static Type _ienum(Type type)
        {
            if (type == null || type == typeof(string))
            {
                return null;
            }

            if (type.IsArray) return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    var target = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (target.IsAssignableFrom(type)) return target;
                }
            }

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                return _ienum(type.BaseType);
            }

            return null;
        }

        static bool _test(RiakResult result)
        {
#if DEBUG
            return result?.IsSuccess ?? true;
#else
            return result.IsSuccess;
#endif
        }

        public static void IsSuccess(RiakResult result)
        {
            if (_test(result)) return;

            throw new GeneralException(result.ErrorMessage, result.ResultCode);
        }

        public static void IsSuccess(Store cmd, RiakResult result)
        {
            if (_test(result)) return;

            throw new StoreException(result);
        }

        public static void IsSuccess(Query cmd, RiakResult result)
        {
            if (_test(result)) return;

            throw new QueryException(result, cmd);
        }

        public static void IsValidFor<T>(Expression expression)
        {
            var type = _telem(expression.Type);

            if (typeof(T) == type) return;

            throw new ArgumentException("Invalid expression type: " + type);
        }
    }
}
