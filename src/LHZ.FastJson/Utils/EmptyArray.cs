using System;
using System.Runtime.CompilerServices;

namespace LHZ.FastJson.Utils
{
    /// <summary>
    /// EmptyArray Utility class to provide a cached empty array for any type.
    /// </summary>
    public static class EmptyArray<T>
    {
        #if NET46_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        private readonly static T[] _value = Array.Empty<T>();
        #else
        private readonly static T[] _value = new T[0];
        #endif
        /// <summary>
        /// Gets the cached empty array for the type T.
        /// </summary>
        public static T[] Value => _value;
    }
}
