using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Format
{
    /// <summary>
    /// Json格式化接口（已废弃）
    /// </summary>
    [Obsolete("This interface is obsolete. Use alternative formatting methods instead.")]
    public interface IJsonFormat
    {
        /// <summary>
        /// 格式化对象类型
        /// </summary>
        ObjectType Type { get; }
    }
}
