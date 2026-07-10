using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON null object
    /// </summary>
    public class JsonNull : JsonObject
    {
        private static readonly string _valueStr = "null";
        private static readonly JsonNull _value = new JsonNull();
        /// <summary>
        /// Null value
        /// </summary>
        public static JsonNull Null => _value;
        /// <summary>
        /// Default constructor
        /// </summary>
        internal JsonNull()
        {
        }
        ///<inheritdoc/>
        public override object Value => null;
        /// <inheritdoc/>
        public override string ToString()
        {
            return _valueStr;
        }
        /// <inheritdoc/>
        public override StringBuilder ToStringBuilder(StringBuilder stringBuilder = null)
        {
            return stringBuilder == null ? new StringBuilder(_valueStr)
            : stringBuilder.Append(_value);
        }
        /// <summary>
        /// JSON object type
        /// </summary>
        public override JsonType Type => JsonType.Null;
    }
}
