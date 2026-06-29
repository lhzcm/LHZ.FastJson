using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json容器对象
    /// </summary>
    public class JsonContent : JsonObject, IEnumerable<KeyValuePair<JsonPropertyName, IJsonObject>>
    {
        private readonly Dictionary<JsonPropertyName, IJsonObject> _value;
        public override object Value => _value;

        /// <summary>
        /// 获取Dictionary类型的值
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public Dictionary<string, IJsonObject> GetValue()
        {
            var dic = new Dictionary<string, IJsonObject>();
            foreach (var item in this)
            {
                dic.Add(item.Key.Name.ToString(), item.Value);
            }
            return dic;
        }
        internal JsonContent(int position) : base(position)
        {
            this._value = new Dictionary<JsonPropertyName, IJsonObject>();
        }
        /// <summary>
        /// 向Json容器里添加Json对象属性
        /// </summary>
        /// <param name="attrName">属性名称</param>
        /// <param name="obj">Json对象</param>
        [Obsolete("This method is deprecated and will be removed in the next official release. Please use the AddJsonProperty method instead.")]
        public void AddJsonAttr(string attrName, IJsonObject obj)
        {
            AddJsonProperty(attrName, obj);
        }
        /// <summary>
        /// 向Json容器里添加Json对象属性
        /// </summary>
        /// <param name="jsonPropertyName">属性名称</param>
        /// <param name="value">Json对象</param>
        internal void AddJsonProperty(JsonPropertyName jsonPropertyName, IJsonObject value)
        {
            #if NET5_0_OR_GREATER
            if (!_value.TryAdd(jsonPropertyName, value))
            {
                throw new Exception($"Json对象中已经存在属性名为[{jsonPropertyName}]的属性，无法添加重复属性！");
            }
            #else
            if(_value.ContainsKey(jsonPropertyName))
            {
                throw new Exception($"Json对象中已经存在属性名为[{jsonPropertyName}]的属性，无法添加重复属性！");
            }
            _value.Add(jsonPropertyName, value);
            #endif
        }
         /// <summary>
        /// 向Json容器里添加Json对象属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="value">属性值</param>

        public void AddJsonProperty(string name, IJsonObject value)
        {
            AddJsonProperty(new JsonPropertyName(name), value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            stringBuilder.Append("{");
            foreach (var item in this)
            {
                stringBuilder.Append('"');
                item.Key.Name.AppendToStringBuilder(stringBuilder);
                stringBuilder.Append('"');
                stringBuilder.Append(":");
                item.Value.ToJsonStringBuilder(stringBuilder);
                stringBuilder.Append(",");
            }
            if (_value.Count > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append("}");
            return stringBuilder;
        }
        public IEnumerator<KeyValuePair<JsonPropertyName, IJsonObject>> GetEnumerator()
        {
            return _value.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._value.GetEnumerator();
        }

        /// <summary>
        /// 通过字符串索引获取对象
        /// </summary>
        /// <param name="index">字符串索引</param>
        /// <returns>Json对象</returns>
        public override IJsonObject this[string index]
        {
            get
            {
               return this[new JsonPropertyName(index)];
            }
        }
        public override IJsonObject this[JsonPropertyName jsonPropertyName]
        {
            get
            {  
                if(!_value.TryGetValue(jsonPropertyName, out var value))
                {
                    return null;
                }
                return value;     
                // #if NET5_0_OR_GREATER     
                // if(!_value.TryGetValue(jsonPropertyName, out var value))
                // {
                //     return null;
                // }
                // return value;
                
                // #else
                // if(!_value.ContainsKey(jsonPropertyName))
                //     return null;
                // return _value[jsonPropertyName] as IJsonObject;
                // #endif
                
               
            }
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
                int i = 0;
                foreach (var item in this)
                {
                    if (index == i)
                    {
                        return item.Value;
                    }
                    i++;
                }
                throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// Json对象类型
        /// </summary>
        public override JsonType Type => JsonType.Content;
    }
}
