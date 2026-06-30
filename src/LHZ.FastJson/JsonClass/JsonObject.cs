using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json对象类
    /// </summary>
    public abstract class JsonObject : IJsonObject
    {
        /// <summary>
        /// 解析字符串，对象在字符串的起始位置，-1表示并非字符串解析获得对象
        /// </summary>
        internal protected int _position;

        internal JsonObject(int position)
        {
            this._position = position;
        }
        internal JsonObject()
        {
            _position = -1;
        }
        /// <summary>
        /// Json对象类型
        /// </summary>
        public abstract JsonType Type { get;}
        /// <summary>
        /// 通过字符串索引获取对象
        /// </summary>
        /// <param name="index">字符串索引</param>
        /// <returns>Json对象</returns>
        public virtual IJsonObject this[string index]
        {
            get
            {
                throw new InvalidOperationException($"{this.Type}并非是{JsonType.Content}无法调用该索引方法！");
            }
        }
        /// <summary>
        /// 通过下标索引获取对象
        /// </summary>
        /// <param name="index">下标串索引</param>
        /// <returns>Json对象</returns>
        public virtual IJsonObject this[int index]
        {
            get
            {
                throw new InvalidOperationException($"{this.Type}并非是{JsonType.Array}或{JsonType.Content}无法调用该索引方法！");
            }
        }
        /// <summary>
        /// 通过JsonPropertyName索引获取对象
        /// </summary>
        /// <param name="index">属性名索引</param>
        /// <returns>Json对象</returns>
        public virtual IJsonObject this[JsonPropertyName index]
        {
            get
            {
                throw new InvalidOperationException($"{this.Type}并非是{JsonType.Content}无法调用该索引方法！");
            }
        }

        /// <summary>
        /// 判断是否存在指定名称的子节点
        /// </summary>
        /// <param name="name">节点名称</param>
        /// <returns>是否存在</returns>
        public bool HasChildrenNode(string name)
        {
            Dictionary<string, IJsonObject> obj = Value as Dictionary<string, IJsonObject>;
            if (obj == null)
            {
                return false;
            }
            return obj.ContainsKey(name);
        }

        /// <summary>
        /// Json对象值
        /// </summary>
        public virtual object Value => null;
        /// <summary>
        ///字符串位置
        /// </summary>
        public int Position
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// 把对象转换成字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// 把对象转化成Json字符串
        /// </summary>
        /// <returns>Json字符串</returns>
        public virtual string ToJsonString()
        {
            return ToJsonStringBuilder().ToString();
        }
        /// <summary>
        /// 将Json对象序列化为StringBuilder
        /// </summary>
        public abstract StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null);
    }
}
