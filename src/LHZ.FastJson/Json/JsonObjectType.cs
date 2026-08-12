using LHZ.FastJson.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json
{
    /// <summary>
    /// Object type mapping table
    /// </summary>
    internal static class JsonObjectType
    {
        public static readonly Dictionary<Type, ObjectType> ObjectTypes = new Dictionary<Type, ObjectType>(Byte.MaxValue);
        static JsonObjectType()
        {
            ObjectTypes.Add(typeof(Boolean), ObjectType.Boolean);
            ObjectTypes.Add(typeof(Byte), ObjectType.Byte);
            ObjectTypes.Add(typeof(Char), ObjectType.Char);
            ObjectTypes.Add(typeof(Int16), ObjectType.Int16);
            ObjectTypes.Add(typeof(UInt16), ObjectType.UInt16);
            ObjectTypes.Add(typeof(Int32), ObjectType.Int32);
            ObjectTypes.Add(typeof(UInt32), ObjectType.UInt32);
            ObjectTypes.Add(typeof(Int64), ObjectType.Int64);
            ObjectTypes.Add(typeof(UInt64), ObjectType.UInt64);
            ObjectTypes.Add(typeof(Single), ObjectType.Float);
            ObjectTypes.Add(typeof(Double), ObjectType.Double);
            ObjectTypes.Add(typeof(Decimal), ObjectType.Decimal);
            ObjectTypes.Add(typeof(DateTime), ObjectType.DateTime);
            ObjectTypes.Add(typeof(String), ObjectType.String);
            ObjectTypes.Add(typeof(System.Enum), ObjectType.Enum);
            ObjectTypes.Add(typeof(IDictionary), ObjectType.Dictionary);
            ObjectTypes.Add(typeof(IEnumerable), ObjectType.Enumerable);
            ObjectTypes.Add(typeof(Object), ObjectType.Object);
            ObjectTypes.Add(typeof(IList), ObjectType.List);
            ObjectTypes.Add(typeof(Array), ObjectType.Array);
        }

        /// <summary>
        /// Get the mapping dictionary from type to ObjectType
        /// </summary>
        internal static Dictionary<Type, ObjectType> GetObjectTypes()
        {
            return ObjectTypes;
        }
    }
}
