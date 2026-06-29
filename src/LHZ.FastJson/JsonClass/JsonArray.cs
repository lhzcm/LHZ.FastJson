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
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public List<IJsonObject> GetValue()
        {
            return this._value;
        }
        internal JsonArray(int position) : base(position)
        {
            this._value = new List<IJsonObject>();
        }
        public JsonArray()
        {
            this._value = new List<IJsonObject>();
        }

        public int Length => _value.Count;

        /// <summary>
        /// 向数组里添加Json对象
        /// </summary>
        /// <param name="obj">Json对象</param>
        public void AddJsonObject(JsonObject obj)
        {
            this._value.Add(obj);
        }

        public override StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            stringBuilder.Append("[");
            for (int i = 0; i < _value.Count; i++)
            {
                _value[i].ToJsonStringBuilder(stringBuilder);
                stringBuilder.Append(",");
            }
            if (_value.Count > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append("]");
            return stringBuilder;
        }

        public IEnumerator<IJsonObject> GetEnumerator()
        {
            return _value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _value.GetEnumerator();
        }
        /// <summary>
        /// 通过下标索引获取对象
        /// </summary>
        /// <param name="index">下标串索引</param>
        /// <returns>Json对象</returns>
        public override IJsonObject this[int index]
        {
            get
            {
                return _value[index];
            }
        }
        /// <summary>
        /// Json对象类型
        /// </summary>
        public override JsonType Type => JsonType.Array;
    }
}
