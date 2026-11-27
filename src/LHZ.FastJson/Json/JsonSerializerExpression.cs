using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.Utils;

namespace LHZ.FastJson.Json
{
    internal static class JsonSerializerExpression<T>
    {
        private static Action<JsonSerializer, T>[] _serializeActions = new Action<JsonSerializer, T>[4];
        private static Type _type = typeof(T);
        public static void Serialize(JsonSerializer jsonSerializer, T obj)
        {
            GetSerializeAction(jsonSerializer.JsonConvertOptions.CaseStyle)(jsonSerializer, obj);
        }
        private static Action<JsonSerializer, T> GetSerializeAction(CaseStyle caseStyle)
        {
            var action = _serializeActions[(int)caseStyle];
            if(action != null)
            {
                return action;
            }
            var expresion = CreateSerializeAction(caseStyle);
            action = expresion.Compile();
            _serializeActions[(int)caseStyle] = action;
            return action;
        }
        private static Expression<Action<JsonSerializer, T>> CreateSerializeAction(CaseStyle caseStyle)
        {
            //Current object
            var serializerObjParameter = Expression.Parameter(typeof(JsonSerializer), "serializerObjParameter");
            //Serialization object parameter
            var objParameter = Expression.Parameter(_type, "objParameter");
            //JSON node types are JSON already: write their text instead of reflecting over their members
            if (typeof(IJsonObject).IsAssignableFrom(_type))
            {
                return Expression.Lambda<Action<JsonSerializer, T>>(
                    Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeJsonObject", BindingFlags.NonPublic | BindingFlags.Instance), objParameter),
                    serializerObjParameter, objParameter);
            }
            //Expression
            Expression exp = CallBaseTypeMethodExpression(_type, serializerObjParameter, objParameter);
            if(exp != null)
            {
                return Expression.Lambda<Action<JsonSerializer, T>>(exp, serializerObjParameter, objParameter);
            }
            return CreateObjectSerializationExpression(_type, caseStyle);
        }
         /// <summary>
        /// Create a serialization expression for the given object type
        /// </summary>
        /// <returns>Serialization expression</returns>
        private static Expression<Action<JsonSerializer, T>> CreateObjectSerializationExpression(Type type, CaseStyle caseStyle)
        {
            //Current object
            var serializerObjParameter = Expression.Parameter(typeof(JsonSerializer), "serializerObjParameter");
            //Serialization object parameter
            var objParameter = Expression.Parameter(type, "objParameter");
            //Expression list
            List<Expression> expList = new List<Expression>();
            //Get properties
            var properties = type.GetProperties().Where(n => {
                if (!n.CanRead)
                    return false;
                //Static properties and indexers cannot be compiled as instance property accesses.
                var getter = n.GetGetMethod(false);
                if (getter == null || getter.IsStatic)
                    return false;
                if (n.GetIndexParameters().Length != 0)
                    return false;
                //Check if serialization should be ignored
                if (Attribute.GetCustomAttribute(n, typeof(JsonIgnoredAttribute)) is JsonIgnoredAttribute jsonIgnored && (jsonIgnored.JsonIgnoredMethod & JsonMethods.Serialize) == JsonMethods.Serialize)
                {
                    return false;
                }
                return true;
            }).ToList();

            //Check for duplicate property names
            if (JsonUtility.HasSameNameProperty(properties, caseStyle, out string samePropertyName))
            {
                throw new Exception($"序列化失败，类型{type.FullName}不同的属性存在相同的属性名\"{samePropertyName}\"");
            }
            //Add left brace for JSON object type
            expList.Add(Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("AppendCharToStringBuilder", BindingFlags.NonPublic | BindingFlags.Instance), Expression.Constant('{')));

            foreach (var item in properties)
            {
                var propertyType = item.PropertyType;
                ObjectType objectType = GetObjectType(propertyType);
                Expression exp = null;

                //Custom property name handling
                string jsonPropertyName = JsonUtility.GetPropertyName(item, caseStyle);
                //Add property name
                expList.Add(Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("AppendStringToStringBuilder", BindingFlags.NonPublic | BindingFlags.Instance), Expression.Constant($"\"{jsonPropertyName}\":")));
                exp  = CallBaseTypeMethodExpression(propertyType, serializerObjParameter, Expression.Property(objParameter, item));
                if (exp == null)
                {
                    if (propertyType.IsValueType || propertyType.IsSealed)
                    {
                        exp = Expression.Call(typeof(JsonSerializerExpression<>).MakeGenericType(propertyType).GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static), serializerObjParameter, Expression.Property(objParameter, item));
                    }
                    else
                    {
                        exp = Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeAny", BindingFlags.NonPublic | BindingFlags.Instance), Expression.Property(objParameter, item));
                    }
                }
                expList.Add(exp);
                //Check if it is the last field; if not, add ","
                if (properties.IndexOf(item) < properties.Count - 1)
                {
                    expList.Add(Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("AppendCharToStringBuilder", BindingFlags.NonPublic | BindingFlags.Instance), Expression.Constant(',')));
                }
            }
            //Circular reference check
            if (!type.IsValueType)
            {
                expList.Insert(0, Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("ReferenceStackPush", BindingFlags.NonPublic | BindingFlags.Instance), objParameter));
                expList.Add(Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("ReferenceStackPop", BindingFlags.NonPublic | BindingFlags.Instance)));
            }

           expList.Add(Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("AppendCharToStringBuilder", BindingFlags.NonPublic | BindingFlags.Instance), Expression.Constant('}')));
           return  Expression.Lambda<Action<JsonSerializer, T>>(Expression.Block(expList.ToArray()), serializerObjParameter, objParameter);
        }
        private static Expression CallBaseTypeMethodExpression(Type type, ParameterExpression serializerObjParameter, Expression objParameter)
        {
            ObjectType objectType = GetObjectType(type);
            switch (objectType)
            {
                case ObjectType.Boolean: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeBoolean", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Int32: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeInt32", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Int64: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeInt64", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.UInt32: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeUInt32", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.UInt64: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeUInt64", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Int16: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeInt16", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.UInt16: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeUInt16", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Byte: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeByte", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Char: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeChar", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Float: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeFloat", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Double: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeDouble", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Decimal: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeDecimal", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.DateTime: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeDateTime", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Enum: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeEnum", BindingFlags.NonPublic | BindingFlags.Instance), Expression.Convert(objParameter, typeof(System.Enum))); 
                case ObjectType.Guid: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeGuid", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.String: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeString", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                case ObjectType.Nullable:
                    { 
                        var nullExp = Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeAny", BindingFlags.NonPublic | BindingFlags.Instance), Expression.Constant(null));
                        var valueExp = Expression.Call(typeof(JsonSerializerExpression<>).MakeGenericType(type.GetGenericArguments().First()).GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static), serializerObjParameter, Expression.Property(objParameter, "Value"));
                        return Expression.IfThenElse(Expression.Property(objParameter, type, "HasValue"), valueExp, nullExp);
                    } 
                case ObjectType.Dictionary: return CreateDictionarySerializationExpression(serializerObjParameter, type, objParameter);
                case ObjectType.Enumerable: return Expression.Call(serializerObjParameter, typeof(JsonSerializer).GetMethod("SerializeEnumerable", BindingFlags.NonPublic | BindingFlags.Instance), objParameter); 
                default: return null;
            }
        }
        /// <summary>
        /// Get the type of the T
        /// </summary>
        /// <returns>Object type</returns>
        private static ObjectType GetObjectType(Type type)
        {
            ObjectType objType;
            if (JsonObjectType.GetObjectTypes().TryGetValue(type, out objType))
            {
                return objType;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return ObjectType.Nullable;
            else if (type.IsEnum)
                return ObjectType.Enum;
            else if (type == typeof(Guid))
                return ObjectType.Guid;
            else if (typeof(IDictionary).IsAssignableFrom(type) || TryGetDictionaryTypes(type, out _, out _))
                return ObjectType.Dictionary;
            else if (typeof(IEnumerable).IsAssignableFrom(type))
                return ObjectType.Enumerable;
            else
                return ObjectType.Object;
        }

        /// <summary>
        /// Get the key/value types of a type that is, or implements, a generic dictionary interface.
        /// </summary>
        private static bool TryGetDictionaryTypes(Type type, out Type keyType, out Type valueType)
        {
            if (IsGenericDictionaryInterface(type))
            {
                var arguments = type.GetGenericArguments();
                keyType = arguments[0];
                valueType = arguments[1];
                return true;
            }
            foreach (var item in type.GetInterfaces())
            {
                if (IsGenericDictionaryInterface(item))
                {
                    var arguments = item.GetGenericArguments();
                    keyType = arguments[0];
                    valueType = arguments[1];
                    return true;
                }
            }
            keyType = null;
            valueType = null;
            return false;
        }

        private static bool IsGenericDictionaryInterface(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }
            var definition = type.GetGenericTypeDefinition();
            if (definition == typeof(IDictionary<,>))
            {
                return true;
            }
#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            if (definition == typeof(IReadOnlyDictionary<,>))
            {
                return true;
            }
#endif
            return false;
        }
        /// <summary>
        /// Create the expression that serializes a dictionary value.
        /// Types that are, or implement, a generic dictionary interface (a property declared
        /// as IDictionary&lt;TKey,TValue&gt; for example) cannot be passed to the non-generic
        /// overload, so the generic serializer is closed over the dictionary's key/value types.
        /// </summary>
        private static Expression CreateDictionarySerializationExpression(ParameterExpression thisObjParameter, Type type, Expression value)
        {
            Type keyType, valueType;
            if (TryGetDictionaryTypes(type, out keyType, out valueType))
            {
                var pairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(pairType);
                return Expression.Call(thisObjParameter, JsonSerializer.GenericDictionarySerializer.MakeGenericMethod(keyType, valueType), Expression.Convert(value, enumerableType));
            }
            return Expression.Call(thisObjParameter, JsonSerializer.DictionarySerializer, value);
        }
    }
}
