using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Attributes
{
    /// <summary>
    /// Json忽略属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class JsonIgnoredAttribute : Attribute
    {
        private JsonMethods _jsonIgnoredMethods = JsonMethods.All;
        /// <summary>
        /// 默认构造函数，序列化和反序列化均忽略
        /// </summary>
        public JsonIgnoredAttribute()
        { 
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ignoredMethod">忽略的Json方法</param>
        public JsonIgnoredAttribute(JsonMethods ignoredMethod)
        {
            this._jsonIgnoredMethods = ignoredMethod;
        }
        /// <summary>
        /// 获取Json忽略的方法
        /// </summary>
        public JsonMethods JsonIgnoredMethod => _jsonIgnoredMethods;
    }
}
