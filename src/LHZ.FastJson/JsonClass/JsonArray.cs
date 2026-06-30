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
        /// <inheritdoc/>
        public override object Value => _value;
        /// <summary>
        /// 获取数组列表（已废弃，请使用 Value 属性）
        /// </summary>
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public List<IJsonObject> GetValue()
        {
            return this._value;
        }
        internal JsonArray(int position) : base(position)
        {
            this._value = new List<IJsonObject>();
        }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public JsonArray()
        {
            this._value = new List<IJsonObject>();
        }

        /// <summary>
        /// 数组长度
        /// </summary>
        public int Length => _value.Count;

        /// <summary>
        /// 向数组里添加Json对象
        /// </summary>
        /// <param name="obj">Json对象</param>
        public void AddJsonObject(JsonObject obj)
        {
            this._value.Add(obj);
        }

        /// <inheritdoc/>
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
/// <summary>
        /// 获取Json数组的枚举器
        /// </summary>
        
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
