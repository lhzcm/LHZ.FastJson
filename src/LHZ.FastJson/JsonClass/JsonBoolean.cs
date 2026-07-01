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
        /// <inheritdoc/>
        public override object Value => ToJsonString();
        private static readonly StringView _true = new StringView("true");
        private static readonly StringView _false = new StringView("false");
        internal static StringView True => _true;
        internal static StringView False => _false;

        /// <summary>
        /// Get the boolean string (deprecated, use Value property instead)
        /// </summary>
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public string GetValue()
        {
            return this.ToJsonString();
        }
        /// <summary>
        /// Boolean type (true or false)
        /// </summary>
        public BooleanType BooleanType { get; }
        internal JsonBoolean(BooleanType type, int position) : base(position)
        {
            this.BooleanType = type;
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public JsonBoolean()
        {
        }
        /// <inheritdoc/>
        public override StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null)
        {
            return stringBuilder == null ? new StringBuilder(BooleanType == BooleanType.True ? "true" : "false")
            : stringBuilder.Append(BooleanType == BooleanType.True ? "true" : "false");
        }
        /// <inheritdoc/>
        public override string ToJsonString()
        {
            return BooleanType == BooleanType.True ? "true" : "false";
        }
        /// <summary>
        /// JSON object type
        /// </summary>
        public override JsonType Type => JsonType.Boolean;
    }
}
