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
        private string _value;
        public override object Value => _value;
        public string GetValue()
        {
            return this._value;
        }
        /// <summary>
        /// bool类型（true or false）
        /// </summary>
        public BooleanType BooleanType { get; }
        public JsonBoolean(BooleanType type, string value, int position) : base(position)
        {
            this.Type = JsonType.Boolean;
            this.BooleanType = type;
            this._value = value;
        }

        public override string ToJsonString()
        {
            return _value;
        }
    }
}
