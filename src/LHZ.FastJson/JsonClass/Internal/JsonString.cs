using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;

namespace LHZ.FastJson.JsonClass.Internal
{
    /// <summary>
    /// JSON string type
    /// </summary>
    internal sealed class JsonString : JsonClass.JsonString
    {
        private int _position;
        private int _realLength;
        internal override int Position => _position;
        internal JsonString(StringView value, int realLength) : base(value)
        {
            this._position = value.Offset;
            this._realLength = realLength;
        }
        /// <inheritdoc/>
        public override StringBuilder ToStringBuilder(StringBuilder stringBuilder = null)
        {
            if (stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            stringBuilder.Append('\"');
            _value.AppendToStringBuilder(stringBuilder);
            stringBuilder.Append('\"');
            return stringBuilder;
        }
        ///<inheritdoc/>
        public override object Value
        {
            get
            {
                if (_realLength == _value.Length)
                {
                    return _value.ToString();
                }
                char[] chars = new char[_realLength];
                int charsIndex = 0, valueIndex = _value.Offset;
                while (valueIndex < _value.Offset + _value.Length) 
                {
                    char curChar = _value.SourceString[valueIndex++];
                    if (curChar == '\\')
                    {
                        switch (_value.SourceString[valueIndex++])
                        {
                            case '"': curChar = '\"'; break;
                            case '\\': curChar = '\\'; break;
                            case '/': curChar = '/'; break;
                            case 'b': curChar = '\b'; break;
                            case 'f': curChar = '\f'; break;
                            case 'n': curChar = '\n'; break;
                            case 'r': curChar = '\r'; break;
                            case 't': curChar = '\t'; break;
                            case 'u': 
                                {
                                    int value = 0;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        var uchar = _value.SourceString[valueIndex++];
                                        if (!TryHexToInt(uchar, out int hexValue))
                                        {
                                            throw new JsonReadException((valueIndex - 1), "字符位置[" + (valueIndex - 1) + "]处，Json字符串解析错误，Unicode转义字符格式错误");
                                        }
                                        value = (value << 4) + hexValue;
                                    }
                                    curChar = (char)value;
                                }; break;
                        }
                    }
                    chars[charsIndex++] = curChar;
                }
                return new String(chars);
            }
        }
        /// <summary>
        /// Convert hexadecimal character to integer
        /// </summary>
        /// <param name="value">Hexadecimal character to convert</param>
        /// <param name="result">Output integer value</param>
        /// <returns>True if conversion is successful, false otherwise</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public static bool TryHexToInt(char value, out int result)
        {
            if (value >= '0' && value <= '9')
            {
                result = value - '0';
                return true;
            }
            if (value >= 'a' && value <= 'f')
            {
                result = value - 'a' + 10;
                return true;
            }
            if (value >= 'A' && value <= 'F')
            {
                result = value - 'A' + 10;
                return true;
            }
            result = 0;
            return false;
        }
    }
}
