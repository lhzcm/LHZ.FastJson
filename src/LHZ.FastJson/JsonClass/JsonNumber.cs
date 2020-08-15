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
        private string _value;
        public override object Value
        {
            get
            {
                return this._value;
            }
        }
        /// <summary>
        /// 数字类型
        /// </summary>
        public NumberType NumberType { get; }
        public JsonNumber(NumberType type, string value, int position) : base(position)
        {
            this.Type = JsonType.Number;
            this.NumberType = type;
            this._value = value;
        }

        public override string ToJsonString()
        {
            return _value;
        }
    }
}
