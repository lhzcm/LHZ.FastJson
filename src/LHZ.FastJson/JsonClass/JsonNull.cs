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
    class JsonNull : JsonObject
    {
        private static readonly string _value = "null";
        private static readonly StringView _null = new StringView(_value);
        public static StringView Null => _null;
        public override object Value => _value;
        /// <summary>
        /// Get null value (deprecated, use Value property instead)
        /// </summary>
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public string GetValue()
        {
            return _value;
        }
        internal JsonNull(int position) : base(position)
        {
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public JsonNull()
        {
        }

        public override string ToJsonString()
        {
            return _value;
        }

        public override StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null)
        {
            return stringBuilder == null ? new StringBuilder(_value)
            : stringBuilder.Append(_value);
        }
        /// <summary>
        /// JSON object type
        /// </summary>
        public override JsonType Type => JsonType.Null;
    }
}
