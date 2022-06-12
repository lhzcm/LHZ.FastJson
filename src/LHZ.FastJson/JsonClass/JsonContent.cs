using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json容器对象
    /// </summary>
    public class JsonContent : JsonObject, IEnumerable<KeyValuePair<string, JsonObject>>
    {
        private Dictionary<string, JsonObject> _value;
        public override object Value
        {
            get
            {
                return this._value;
            }
        }
        public JsonContent(int position) : base(position)
        {
            this.Type = Enum.JsonType.Content;
            this._value = new Dictionary<string, JsonObject>();
        }
        /// <summary>
        /// 向Json容器里添加Json对象属性
        /// </summary>
        /// <param name="attrName">属性名称</param>
        /// <param name="obj">Json对象</param>
        public void AddJsonAttr(string attrName, JsonObject obj)
        {
            this._value.Add(attrName, obj);
        }

        public override string ToJsonString()
        {
            StringBuilder strBuilder = new StringBuilder("{");
            foreach (var item in _value)
            {
                strBuilder.Append("\"" + item.Key + "\":");
                strBuilder.Append(item.Value.ToJsonString() + ",");
            }
            if (_value.Count > 0)
            {
                strBuilder.Remove(strBuilder.Length - 1, 1);
            }
            strBuilder.Append("}");
            return strBuilder.ToString();
        }

        public IEnumerator<KeyValuePair<string, JsonObject>> GetEnumerator()
        {
            return this._value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._value.GetEnumerator();
        }
    }
}
