using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// Json读取器接口
    /// </summary>
    public interface IJsonReader
    {
        /// <summary>
        /// 读取json
        /// </summary>
        /// <returns></returns>
        JsonObject JsonRead();

        /// <summary>
        /// 是否是有效的Json字符串
        /// </summary>
        bool IsValidJson { get; }
    }
}
