using LHZ.FastJson.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json
{
    internal static class JsonObjectType
    {
        private static readonly Dictionary<Type, ObjectType> _objectTypes = new Dictionary<Type, ObjectType>(Byte.MaxValue);
        static JsonObjectType()
        {
            _objectTypes.Add(typeof(Boolean), ObjectType.Boolean);
            _objectTypes.Add(typeof(Byte), ObjectType.Byte);
            _objectTypes.Add(typeof(Char), ObjectType.Char);
            _objectTypes.Add(typeof(Int16), ObjectType.Int16);
            _objectTypes.Add(typeof(UInt16), ObjectType.UInt16);
            _objectTypes.Add(typeof(Int32), ObjectType.Int32);
            _objectTypes.Add(typeof(UInt32), ObjectType.UInt32);
            _objectTypes.Add(typeof(Int64), ObjectType.Int64);
            _objectTypes.Add(typeof(UInt64), ObjectType.UInt64);
            _objectTypes.Add(typeof(Single), ObjectType.Float);
            _objectTypes.Add(typeof(Double), ObjectType.Double);
            _objectTypes.Add(typeof(Decimal), ObjectType.Decimal);
            _objectTypes.Add(typeof(DateTime), ObjectType.DateTime);
            _objectTypes.Add(typeof(String), ObjectType.String);
            _objectTypes.Add(typeof(System.Enum), ObjectType.Enum);
            _objectTypes.Add(typeof(IDictionary), ObjectType.Dictionary);
            _objectTypes.Add(typeof(IEnumerable), ObjectType.Enumerable);
            _objectTypes.Add(typeof(Object), ObjectType.Object);
            _objectTypes.Add(typeof(IList), ObjectType.List);
            _objectTypes.Add(typeof(Array), ObjectType.Array);
        }

        internal static Dictionary<Type, ObjectType> GetObjectTypes()
        {
            return _objectTypes;
        }
    }
}
