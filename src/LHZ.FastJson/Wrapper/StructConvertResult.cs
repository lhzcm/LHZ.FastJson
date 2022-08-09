using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Wrapper
{
    public struct StructConvertResult<T> where T : struct
    {
        public StructConvertResult(bool success, T result)
        {
            Success = success;
            Result = result;
        }
        public bool Success { get; set; }
        public T Result { get; set; }
    }
}
