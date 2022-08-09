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

        bool HasChildrenNode(string name);
        /// <summary>
        /// 把Json对象转化成Json字符串
        /// </summary>
        /// <returns>Json字符串</returns>
        string ToJsonString();

        int Position { get; }

        string ToString();

    }
}
