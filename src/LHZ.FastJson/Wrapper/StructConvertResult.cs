using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Wrapper
{
    /// <summary>
    /// Struct conversion wrapper class
    /// </summary>
    /// <typeparam name="T">The target struct to convert to</typeparam>
    public struct StructConvertResult<T> where T : struct
    {
        /// <summary>
        /// Initialize conversion result
        /// </summary>
        /// <param name="success">Whether successful</param>
        /// <param name="result">Conversion result</param>
        public StructConvertResult(bool success, T result)
        {
            Success = success;
            Result = result;
        }
        /// <summary>
        /// Whether the conversion was successful
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Conversion result
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Convert string to enum type
        /// </summary>
        /// <param name="dist">Target string</param>
        /// <returns>Value type wrapper class</returns>
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
