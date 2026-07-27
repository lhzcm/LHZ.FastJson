using System;
using System.Collections.Generic;
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
                                        if (!IsHexDigit(uchar))
                                        {
                                            throw new JsonReadException((valueIndex - 1), "字符位置[" + (valueIndex - 1) + "]处，Json字符串解析错误，Unicode转义字符格式错误");
                                        }
                                        value = (value << 4) + HexToInt(uchar);
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
        private static bool IsHexDigit(char value)
        {
            return (value >= '0' && value <= '9') ||
                   (value >= 'a' && value <= 'f') ||
                   (value >= 'A' && value <= 'F');
        }
        private static int HexToInt(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return value - '0';
            }
            if (value >= 'a' && value <= 'f')
            {
                return value - 'a' + 10;
            }
            return value - 'A' + 10;
        }
    }
}
