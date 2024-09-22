using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Enum
{
    public enum JsonType
    {
        /// <summary>
        /// Json的对象类型
        /// </summary>
        Content,
        /// <summary>
        /// Json的数组类型
        /// </summary>
        Array,
        /// <summary>
        /// json的字符串类型
        /// </summary>
        String,
        /// <summary>
        /// json的数值类型
        /// </summary>
        Number,
        /// <summary>
        /// json的布尔类型
        /// </summary>
        Boolean,
        /// <summary>
        /// json的空类型
        /// </summary>
        Null
    }
}
