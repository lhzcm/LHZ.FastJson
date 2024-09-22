using LHZ.FastJson.Enum;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// Json对象类
    /// </summary>
    public interface IJsonObject
    {
        /// <summary>
        /// Json对象类型
        /// </summary>
        JsonType Type { get;}

        /// <summary>
        /// Json对象值
        /// </summary>
        object Value { get; }
        IJsonObject this[string index] { get; }
        IJsonObject this[int index] { get; }
        /// <summary>
        /// 判断是否存在指定名称的子节点
        /// </summary>
        /// <param name="name">节点名称</param>
        /// <returns>是否存在</returns>
        bool HasChildrenNode(string name);
        /// <summary>
        /// 把Json对象转化成Json字符串
        /// </summary>
        /// <returns>Json字符串</returns>
        string ToJsonString();
        /// <summary>
        ///字符串起始位置
        /// </summary>
        int Position { get; }

        string ToString();

    }
}
