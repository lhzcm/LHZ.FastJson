using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Enum.CustomConverter
{
    public enum JsonCustomConvertItem
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 自定义序列化
        /// </summary>
        CustomSerialize = 1,
        /// <summary>
        /// 自定义反序列化
        /// </summary>
        CustomDeSerialize = 2,
        /// <summary>
        /// 全部
        /// </summary>
        All = 3,
    }
}
