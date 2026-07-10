using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Enum
{
    /// <summary>
    /// Enumeration of serializable and deserializable object types
    /// </summary>
    [Flags]
    public enum ObjectType : uint
    {
        /// <summary>Boolean</summary>
        Boolean = 1,
        /// <summary>Int32</summary>
        Int32 = 2,
        /// <summary>Int64</summary>
        Int64 = 4,
        /// <summary>Int16</summary>
        Int16 = 8,
        /// <summary>UInt32</summary>
        UInt32 = 16,
        /// <summary>UInt64</summary>
        UInt64 = 32,
        /// <summary>UInt16</summary>
        UInt16 = 64,
        /// <summary>Byte</summary>
        Byte = 128,
        /// <summary>Float</summary>
        Float = 256,
        /// <summary>Double</summary>
        Double = 512,
        /// <summary>Decimal</summary>
        Decimal = 1024,
        /// <summary>DateTime</summary>
        DateTime = 2048,
        /// <summary>Char</summary>
        Char = 4096,
        /// <summary>String</summary>
        String = 8192,
        /// <summary>Enum</summary>
        Enum = 16384,
        /// <summary>Guid</summary>
        Guid = 32768,
        /// <summary>Dictionary</summary>
        Dictionary = 65536,
        /// <summary>Array</summary>
        Array = 131072,
        /// <summary>List</summary>
        List = 262144,
        /// <summary>Enumerable</summary>
        Enumerable = 524288,
        /// <summary>Object</summary>
        Object = 1048576,
        /// <summary>Nullable</summary>
        Nullable = 2097152,
    }
}
