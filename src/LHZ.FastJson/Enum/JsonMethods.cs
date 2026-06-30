using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Enum
{
    /// <summary>
    /// Json操作方法枚举
    /// </summary>
    public enum JsonMethods
    {
        /// <summary>
        /// 序列化
        /// </summary>
        Serialize = 1,
        /// <summary>
        /// 反序列化
        /// </summary>
        Deserialize = 2,
        /// <summary>
        /// 全部
        /// </summary>
        All = 3,
    }
}
