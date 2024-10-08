﻿using LHZ.FastJson.Enum;
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
        public IJsonObject this[string index]
        {
            get
            {
                IJsonObject result = null;
                if (this.Type != JsonType.Content)
                {
                    throw new InvalidOperationException($"{this.Type}并非是{JsonType.Content}无法调用该索引方法！");
                }
               ((Dictionary<string, IJsonObject>)this.Value).TryGetValue(index, out result);
                return result;
            }
        }
        /// <summary>
        /// 通过下标索引获取对象
        /// </summary>
        /// <param name="index">下标串索引</param>
        /// <returns>Json对象</returns>
        public IJsonObject this[int index]
        {
            get
            {
                IJsonObject result = null;
                if (this.Type == JsonType.Array)
                {
                    result = ((List<IJsonObject>)this.Value)[index];
                    return result;
                }
                else if (this.Type == JsonType.Content)
                {
                    int i = 0;
                    foreach (var item in (Dictionary<string, IJsonObject>)this.Value)
                    {
                        if (index == i)
                        {
                            return item.Value;
                        }
                        i++;
                    }
                    throw new ArgumentOutOfRangeException();
                }
                throw new InvalidOperationException($"{this.Type}并非是{JsonType.Array}或{JsonType.Content}无法调用该索引方法！");
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
        public abstract string ToJsonString();
    }
}
