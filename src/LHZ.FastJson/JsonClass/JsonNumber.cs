using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json数字对象
    /// </summary>
    public class JsonNumber : JsonObject
    {
        private StringView _value;
        /// <inheritdoc/>
        public override object Value => _value;
        /// <summary>
        /// 获取数值字符串（已废弃，请使用 Value 属性）
        /// </summary>
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public string GetValue()
        {
            return this._value.ToString();
        }
        /// <summary>
        /// 数字类型
        /// </summary>
        public NumberType NumberType { get; }
        internal JsonNumber(NumberType type, StringView value, int position) : base(position)
        {
            this.NumberType = type;
            this._value = value;
        }
        /// <summary>
        /// 使用long值初始化
        /// </summary>
        public JsonNumber(long value)
        {
            this.NumberType = NumberType.Long;
            _value = new StringView(value.ToString());
        }
        /// <summary>
        /// 使用ulong值初始化
        /// </summary>
        public JsonNumber(ulong value)
        {
            this.NumberType = NumberType.Long;
            _value = new StringView(value.ToString());
        }
        /// <summary>
        /// 使用double值初始化
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
        /// Json对象类型
        /// </summary>
        public override JsonType Type => JsonType.Number;
    }
}
