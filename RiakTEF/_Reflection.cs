using System;

namespace RiakTEF
{
    static class _Reflection
    {
        public static bool IsNullAssignable(this Type type)
        {
            return type.IsClass || type.IsNullable();
        }

        public static bool IsNullable(this Type type)
        {
            Type nullable;
            return type.IsNullable(out nullable);
        }

        public static bool IsNullable(this Type type, out Type nullable)
        {
            if (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition())
            {
                nullable = type.GetGenericArguments()[0];

                return true;
            }

            nullable = null;

            return false;
        }
    }
}
