using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json布尔对象
    /// </summary>
    public class JsonBoolean : JsonObject
    {
        public override object Value => ToJsonString();
        private static readonly StringView _true = new StringView("true");
        private static readonly StringView _false = new StringView("false");
        internal static StringView True => _true;
        internal static StringView False => _false;

        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public string GetValue()
        {
            return this.ToJsonString();
        }
        /// <summary>
        /// bool类型（true or false）
        /// </summary>
        public BooleanType BooleanType { get; }
        internal JsonBoolean(BooleanType type, int position) : base(position)
        {
            this.BooleanType = type;
        }
        public JsonBoolean()
        {
        }
        public override StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null)
        {
            return stringBuilder == null ? new StringBuilder(BooleanType == BooleanType.True ? "true" : "false")
            : stringBuilder.Append(BooleanType == BooleanType.True ? "true" : "false");
        }
        public override string ToJsonString()
        {
            return BooleanType == BooleanType.True ? "true" : "false";
        }
        /// <summary>
        /// Json对象类型
        /// </summary>
        public override JsonType Type => JsonType.Boolean;
    }
}
