using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON number object
    /// </summary>
    public class JsonNumber : JsonObject
    {
        private StringView _value;
        /// <inheritdoc/>
        public override object Value => _value;
        /// <summary>
        /// Get numeric string (deprecated, use Value property instead)
        /// </summary>
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public string GetValue()
        {
            return this._value.ToString();
        }
        /// <summary>
        /// Number type
        /// </summary>
        public NumberType NumberType { get; }
        internal JsonNumber(NumberType type, StringView value, int position) : base(position)
        {
            this.NumberType = type;
            this._value = value;
        }
        /// <summary>
        /// Initialize with long value
        /// </summary>
        public JsonNumber(long value)
        {
            this.NumberType = NumberType.Long;
            _value = new StringView(value.ToString());
        }
        /// <summary>
        /// Initialize with ulong value
        /// </summary>
        public JsonNumber(ulong value)
        {
            this.NumberType = NumberType.Long;
            _value = new StringView(value.ToString());
        }
        /// <summary>
        /// Initialize with double value
        /// </summary>
        public JsonNumber(double value)
        {
            this.NumberType = NumberType.Double;
            _value = new StringView(value.ToString());
        }
        /// <inheritdoc/>
        public override StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            _value.AppendToStringBuilder(stringBuilder);
            return stringBuilder;
        
        }
        /// <inheritdoc/>
        public override string ToJsonString()
        {
            return _value.ToString();
        }
        /// <summary>
        /// JSON object type
        /// </summary>
        public override JsonType Type => JsonType.Number;
    }
}
