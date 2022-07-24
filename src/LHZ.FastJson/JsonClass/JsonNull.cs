using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json空对象
    /// </summary>
    class JsonNull : JsonObject
    {
        private string _value;
        public override object Value => _value;
        public string GetValue()
        {
            return this._value;
        }
        public JsonNull(string value, int position) : base(position)
        {
            this.Type = JsonType.Null;
            this._value = value;
        }

        public override string ToJsonString()
        {
            return _value;
        }
    }
}
