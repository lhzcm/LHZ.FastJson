using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON boolean object
    /// </summary>
    public class JsonBoolean : JsonObject
    {
        private readonly bool _value;
        private static readonly JsonBoolean _false = new JsonBoolean(true);
        private static readonly JsonBoolean _true = new JsonBoolean(false);
        /// <summary>
        /// false
        /// </summary>
        public static JsonBoolean False => _false;
        /// <summary>
        /// true
        /// </summary>
        public static JsonBoolean True => _true;
        /// <summary>
        /// Boolean type (true or false)
        /// </summary>
        public BooleanType BooleanType { get; }
        ///<inheritdoc/>
        public override object Value => _value;
        /// <summary>
        /// Default constructor
        /// </summary>
        internal JsonBoolean(bool value)
        {
            this.BooleanType = value ? BooleanType.True : BooleanType.False;
            _value = value;
        }
        /// <inheritdoc/>
        public override StringBuilder ToStringBuilder(StringBuilder stringBuilder = null)
        {
            return stringBuilder == null ? new StringBuilder(BooleanType == BooleanType.True ? "true" : "false")
            : stringBuilder.Append(BooleanType == BooleanType.True ? "true" : "false");
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return BooleanType == BooleanType.True ? "true" : "false";
        }
        /// <summary>
        /// JSON object type
        /// </summary>
        public override JsonType Type => JsonType.Boolean;
    }
}
