using System;

namespace Sieve.Plus.Extensions
{
    public static partial class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
    }
}
