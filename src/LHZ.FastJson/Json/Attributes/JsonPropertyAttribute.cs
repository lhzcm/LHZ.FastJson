using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Attributes
{
    /// <summary>
    /// Json属性名称特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JsonPropertyAttribute : Attribute
    {
        private readonly string _name;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">自定义属性名称</param>
        public JsonPropertyAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName => _name;
    }
}
