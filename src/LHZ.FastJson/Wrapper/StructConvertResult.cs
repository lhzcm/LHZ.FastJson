using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Wrapper
{
    /// <summary>
    /// 结构体转化封装类
    /// </summary>
    /// <typeparam name="T">转换成目标的结构体</typeparam>
    public struct StructConvertResult<T> where T : struct
    {
        public StructConvertResult(bool success, T result)
        {
            Success = success;
            Result = result;
        }
        public bool Success { get; set; }
        public T Result { get; set; }

        /// <summary>
        /// 把字符串转换成枚举类型
        /// </summary>
        /// <param name="dist">目标字符串</param>
        /// <returns>值类型包装类</returns>
        public static StructConvertResult<T> ConvertToEnum(string dist)
        {
            T result;
            if (System.Enum.TryParse<T>(dist, out result))
            {
                return new StructConvertResult<T>(true, result);
            }
            return new StructConvertResult<T>(false, result);
        }
    }
}
