using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;

namespace LHZ.FastJson.JsonClass.Internal
{
    /// <summary>
    /// Lightweight string view that avoids extra allocations from string slicing
    /// </summary>
    internal struct StringView : IConvertible
    {
        private static readonly char[] _charArrayEmpty = new char[0];
        /// <summary>
        /// Initialize with a specified range
        /// </summary>
        /// <param name="sourceString">Source string</param>
        /// <param name="offset">offset index</param>
        /// <param name="length">string length</param>
        public StringView(string sourceString, int offset, int length)
        {
            SourceString = sourceString;
            Offset = offset;
            Length = length;
        }
        /// <summary>
        /// Initialize with the full string
        /// </summary>
        /// <param name="sourceString">Source string</param>
        public StringView(string sourceString)
        {
            SourceString = sourceString;
            Offset = 0;
            Length = sourceString.Length;
        }
        /// <summary>
        /// Source string
        /// </summary>
        public string SourceString { get; }
        /// <summary>
        /// Start index
        /// </summary>
        public int Offset { get; }
        /// <summary>
        /// View length
        /// </summary>
        public int Length {get;}
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public override string ToString()
        {
            if (Length == 0)
                return string.Empty;
            if (SourceString.Length == Length)
                return SourceString;
            return SourceString.Substring(Offset, Length);
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public static bool operator ==(StringView left, StringView right)
        {
            if (left.Length != right.Length)
                return false;
            for (var i = 0; i < left.Length; i++)
            {
                if (left.SourceString[left.Offset + i] != right.SourceString[right.Offset + i])
                    return false;
            }
            return true;
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public static bool operator !=(StringView left, StringView right)
        {
            return !(left == right);
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public override bool Equals(object obj)
        {
            if (!(obj is StringView other))
                return false;
            return this == other;
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public override int GetHashCode()
        {
            uint hash = 5381;
            if (Length == 0)
                return (int)hash;
            for (int i = Offset; i < Offset + Length; i++)
            {
                unchecked
                {
                    hash = (hash << 5) + hash + SourceString[i];
                }
            }
            return (int)hash;
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public StringBuilder ToStringBuilder()
        {
            if (Length == 0)
                return new StringBuilder();
            if (SourceString.Length == Length)
                return new StringBuilder(SourceString);
            return new StringBuilder(SourceString, Offset, Length, Length);
        }
        /// <summary>
        /// Convert To Char Array
        /// </summary>
        /// <returns>Char Array</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public char[] ToCharArray()
        {
            if (Length == 0)
                return _charArrayEmpty;
            if (SourceString.Length == Length)
                return SourceString.ToCharArray();
            return SourceString.ToCharArray(Offset, Length);
        }
        /// <summary>
        /// Append into stringBuilder 
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public void AppendToStringBuilder(StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                throw new ArgumentNullException(nameof(stringBuilder));
            }
            if (Length == 0)
                return;
            if (SourceString.Length == Length)
            {
                stringBuilder.Append(SourceString);
            }
            else
            {
                stringBuilder.Append(SourceString, Offset, Length);
            }
        }
        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(ToString(), provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(ToString(), provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(ToString(), provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(ToString(), provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(ToString(), provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(ToString(), provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(ToString(), provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(ToString(), provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(ToString(), provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(ToString(), provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(ToString(), provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(ToString(), provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(ToString(), provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(ToString(), provider);
        }
        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(ToString(), provider);
        }
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
    }
}