using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json对象类
    /// </summary>
    public abstract class JsonObject : IJsonObject
    {
        private int _position;

        public JsonObject(int position)
        {
            this._position = position;
        }

        /// <summary>
        /// Json对象类型
        /// </summary>
        public JsonType Type { get; protected set; }

        /// <summary>
        /// 通过字符串索引获取对象
        /// </summary>
        /// <param name="index">字符串索引</param>
        /// <returns>Json对象</returns>
        public JsonObject this[string index]
        {
            get
            {
                JsonObject result = null;
                if (this.Type != JsonType.Content)
                {
                    return result;
                }
               ((Dictionary<string, JsonObject>)this.Value).TryGetValue(index, out result);
                return result;
            }
        }
        /// <summary>
        /// 通过下标索引获取对象
        /// </summary>
        /// <param name="index">下标串索引</param>
        /// <returns>Json对象</returns>
        public JsonObject this[int index]
        {
            get
            {
                JsonObject result = null;
                if (this.Type != JsonType.Array)
                {
                    return result;
                }
                result = ((List<JsonObject>)this.Value)[index];
                return result;
            }
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
        public abstract string ToJsonString();
    }
}
