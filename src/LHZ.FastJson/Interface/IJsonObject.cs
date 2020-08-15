using LHZ.FastJson.Enum;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Interface
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
        JsonObject this[string index] { get; }
        JsonObject this[int index] { get; }

        /// <summary>
        /// 把Json对象转化成Json字符串
        /// </summary>
        /// <returns></returns>
        string ToJsonString();
    }
}
