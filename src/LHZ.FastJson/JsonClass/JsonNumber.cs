using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON number object
    /// </summary>
    public class JsonNumber: JsonObject, IConvertible
    {
        protected IConvertible _value; 
        /// <summary>
        /// Number type
        /// </summary>
        public NumberType NumberType { get; }
        ///<inheritdoc/>
        public override object Value => _value;
        /// <summary>
        /// Initialize with long value
        /// </summary>
        public JsonNumber(long value)
        {
            this.NumberType = NumberType.Long;
            _value = value;
        }
        /// <summary>
        /// Initialize with ulong value
        /// </summary>
        public JsonNumber(ulong value)
        {
            this.NumberType = NumberType.Long;
            _value = value;
        }
        /// <summary>
        /// Initialize with double value
        /// </summary>
        public JsonNumber(double value)
        {
            this.NumberType = NumberType.Double;
            _value = value;
        }
        internal JsonNumber(StringView value, NumberType numberType)
        {
            this.NumberType = numberType;
            _value = value;
        }
        /// <inheritdoc/>
        public override StringBuilder ToStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            stringBuilder.Append(_value);
            return stringBuilder;
        
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return _value.ToString();
        }
        /// <inheritdoc/>
        public TypeCode GetTypeCode()
        {
            return _value.GetTypeCode();
        }
        /// <inheritdoc/>
        public bool ToBoolean(IFormatProvider provider)
        {
            return _value.ToBoolean(provider);
        }
        /// <inheritdoc/>
        public char ToChar(IFormatProvider provider)
        {
            return _value.ToChar(provider);
        }
        /// <inheritdoc/>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return _value.ToSByte(provider);
        }
        /// <inheritdoc/>
        public byte ToByte(IFormatProvider provider)
        {
            return _value.ToByte(provider);
        }
        /// <inheritdoc/>
        public short ToInt16(IFormatProvider provider)
        {
            return  _value.ToInt16(provider);
        }
        /// <inheritdoc/>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return _value.ToUInt16(provider);
        }
        /// <inheritdoc/>
        public int ToInt32(IFormatProvider provider)
        {
            return _value.ToInt32(provider);
        }
        /// <inheritdoc/>
        public uint ToUInt32(IFormatProvider provider)
        {
            return _value.ToUInt32(provider);
        }
        /// <inheritdoc/>
        public long ToInt64(IFormatProvider provider)
        {
            return _value.ToInt64(provider);
        }
        /// <inheritdoc/>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return _value.ToUInt64(provider);
        }
        /// <inheritdoc/>
        public float ToSingle(IFormatProvider provider)
        {
            return _value.ToSingle(provider);
        }
        /// <inheritdoc/>
        public double ToDouble(IFormatProvider provider)
        {
            return _value.ToDouble(provider);
        }
        /// <inheritdoc/>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return _value.ToDecimal(provider);
        }
        /// <inheritdoc/>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            return _value.ToDateTime(provider);
        }
        /// <inheritdoc/>
        public string ToString(IFormatProvider provider)
        {
            return _value.ToString(provider);
        }
        /// <inheritdoc/>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _value.ToType(conversionType, provider);
        }
        /// <inheritdoc/>
        public override JsonType Type => JsonType.Number;
    }
}
