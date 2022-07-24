using LHZ.FastJson.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json数组对象
    /// </summary>
    public class JsonArray : JsonObject, IEnumerable<IJsonObject>
    {
        private List<IJsonObject> _value;
        public override object Value => _value;
        public List<IJsonObject> GetValue()
        {
            return this._value;
        }
        public JsonArray(int position) : base(position)
        {
            this.Type = JsonType.Array;
            this._value = new List<IJsonObject>();
        }

        public int Length => _value.Count;

        /// <summary>
        /// 像数组里添加Json对象
        /// </summary>
        /// <param name="obj">Json对象</param>
        public void AddJsonObject(JsonObject obj)
        {
            this._value.Add(obj);
        }

        public override string ToJsonString()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("[");
            for (int i = 0; i < _value.Count; i++)
            {
                strBuilder.Append(_value[i].ToJsonString() + ",");
            }
            if (_value.Count > 0)
            {
                strBuilder.Remove(strBuilder.Length - 1, 1);
            }
            strBuilder.Append("]");
            return strBuilder.ToString();
        }

        public IEnumerator<IJsonObject> GetEnumerator()
        {
            return _value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _value.GetEnumerator();
        }
    }
}
