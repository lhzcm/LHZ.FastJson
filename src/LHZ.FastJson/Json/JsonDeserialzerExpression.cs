﻿using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.CustomConverter;
using LHZ.FastJson.JsonClass;
using LHZ.FastJson.Wrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LHZ.FastJson.Json
{
    internal static class JsonDeserialzerExpression<T>
    {
        private static readonly Dictionary<Type, ObjectType> _objectTypes = JsonObjectType.GetObjectTypes();

        private static readonly Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T> _funcDeserialize = null;

        static JsonDeserialzerExpression()
        {
            var funcDeserializeExpression = CreateExpression();
            _funcDeserialize = funcDeserializeExpression.Compile();
        }

        // public static T Deserialzer(IJsonObject jsonObject)
        // {
        //     return _funcDeserialize(jsonObject);
        // }


        public static T Deserialzer(IJsonObject jsonObject, params IJsonCustomConverter[] converters)
        {
            Dictionary<Type, IJsonCustomConverter> _customConverters = null;
            if(_customConverters != null)
            {
                _customConverters = new Dictionary<Type, IJsonCustomConverter>(converters.Length);
                foreach(var item in converters)
                {
                    if(_customConverters.ContainsKey(item.ConvertType))
                    {
                        _customConverters[item.ConvertType] = item;
                    }
                    else
                    {
                        _customConverters.Add(item.ConvertType, item);
                    }
                }
            }
            return _funcDeserialize(jsonObject, _customConverters);
        }

        /// <summary>
        /// 获取反序列化表达树
        /// </summary>
        /// <returns></returns>
        private static Expression<Func<IJsonObject, T>> CreateExpression()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObject");
            ParameterExpression customConverterParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "customConverterParameter");

            var curType = typeof(T);
            ObjectType objectType = GetObjectType(curType);
            switch (objectType)
            {
                case ObjectType.Boolean:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, bool>)ConvertToBoolean).Method, jsonObjectParameter), jsonObjectParameter);
                case ObjectType.Byte:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, byte>)ConvertToByte).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.Char:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, char>)ConvertToChar).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.Int16:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, short>)ConvertToInt16).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.UInt16:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, ushort>)ConvertToUInt16).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.Int32:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, int>)ConvertToInt32).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.UInt32:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, uint>)ConvertToUInt32).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.Int64:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, long>)ConvertToInt64).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.UInt64:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, ulong>)ConvertToUInt64).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.Float:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, float>)ConvertToFloat).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.Double:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, double>)ConvertToDouble).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.Decimal:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, decimal>)ConvertToDecimal).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.DateTime:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call(
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, DateTime>)ConvertToDateTime).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.String:
                    return Expression.Lambda<Func<IJsonObject, T>>(Expression.Call( 
                        ((Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, string>)ConvertToString).Method, jsonObjectParameter),  jsonObjectParameter);
                case ObjectType.Enum: return ConvertToEnum();
                case ObjectType.Nullable: return ConvertToNullable();
                case ObjectType.Dictionary: return ConvertToDictionary();
                case ObjectType.Array: return ConvertToArray();
                case ObjectType.List: return ConvertToList();
                //case ObjectType.Enumerable:  
                default:
                    return ConvertToObject();
            }
        }

        

        /// <summary>
        /// 获取对象的类型
        /// </summary>
        /// <param name="type">反序列化对象类型</param>
        /// <returns></returns>
        private static ObjectType GetObjectType(Type type)
        {
            ObjectType objType;
            if (_objectTypes.TryGetValue(type, out objType))
            {
                return objType;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return ObjectType.Nullable;
            else if (type.IsEnum)
                return ObjectType.Enum;
            else if (typeof(IDictionary).IsAssignableFrom(type))
                return ObjectType.Dictionary;
            else if (type.IsArray)
                return ObjectType.Array;
            else if (typeof(IList).IsAssignableFrom(type))
                return ObjectType.List;
            //else if (typeof(IEnumerable).IsAssignableFrom(type))
            //    return ObjectType.Enumerable;
            else
                return ObjectType.Object;

        }

        /// <summary>
        /// 解析成bool类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static bool ConvertToBoolean(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(bool), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<bool>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Boolean)
            {
                throw new JsonDeserializationException(jsonObject, typeof(bool), "Json对象不为Boolean类型不能解析成Boolean类型");
            }
            return Boolean.Parse((string)jsonObject.Value);
        }

        /// <summary>
        /// 解析成byte类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static byte ConvertToByte(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(byte), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<byte>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Byte), "Json对象不为Number类型不能解析成Byte类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Byte), "Json对象Number类型不为Long不能解析成Byte类型");
            }
            return Byte.Parse((string)jsonObject.Value);
        }

        /// <summary>
        /// 解析成char类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static char ConvertToChar(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(char), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<char>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.String)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Char), "Json对象不为String类型不能解析成Char类型");
            }
            if (((string)jsonObject.Value).Length != 1)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Char), "Json对象字符串长度不等于1，不能解析成Char类型");
            }
            return ((string)jsonObject.Value)[0];
        }

        /// <summary>
        /// 解析成int16类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static short ConvertToInt16(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(byte), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<byte>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Int16), "Json对象不为Number类型不能解析成Int16类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Int16), "Json对象Number类型不为Long不能解析成Int16类型");
            }
            return Int16.Parse((string)jsonObject.Value);
        }

        /// <summary>
        /// 解析成uint16类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static ushort ConvertToUInt16(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(ushort), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<ushort>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(UInt16), "Json对象不为Number类型不能解析成UInt16类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(UInt16), "Json对象Number类型不为Long不能解析成UInt16类型");
            }
            return UInt16.Parse((string)jsonObject.Value);
        }


        /// <summary>
        /// 解析成int32类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static int ConvertToInt32(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(int), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<int>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(int), "Json对象不为Number类型不能解析成Int32类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(int), "Json对象Number类型不为Long不能解析成Int32类型");
            }
            return int.Parse((string)jsonObject.Value);
        }
        /// <summary>
        /// 解析成uint32类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param>  
        /// <returns></returns>
        private static uint ConvertToUInt32(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(uint), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<uint>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(uint), "Json对象不为Number类型不能解析成UInt32类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(uint), "Json对象Number类型不为Long不能解析成UInt32类型");
            }
            return uint.Parse((string)jsonObject.Value);
        }

        /// <summary>
        /// 解析成int64类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static long ConvertToInt64(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(long), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<long>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(long), "Json对象不为Number类型不能解析成Int64类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(long), "Json对象Number类型不为Long不能解析成Int64类型");
            }
            return long.Parse((string)jsonObject.Value);
        }

        /// <summary>
        /// 解析成uint64类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static ulong ConvertToUInt64(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(ulong), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<ulong>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(ulong), "Json对象不为Number类型不能解析成UInt64类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(ulong), "Json对象Number类型不为Long不能解析成UInt64类型");
            }
            return ulong.Parse((string)jsonObject.Value);
        }

        /// <summary>
        /// 解析成单精度浮点类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static float ConvertToFloat(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(float), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<float>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(float), "Json对象不为Number类型不能解析成Float类型");
            }
            return float.Parse((string)jsonObject.Value);
        }
        /// <summary>
        /// 解析成双精度浮点类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static double ConvertToDouble(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(double), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<double>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(double), "Json对象不为Number类型不能解析成Double类型");
            }
            return double.Parse((string)jsonObject.Value);
        }
        /// <summary>
        /// 解析成数字类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static decimal ConvertToDecimal(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(decimal), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<decimal>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(decimal), "Json对象不为Number类型不能解析成Decimal类型");
            }
            return decimal.Parse((string)jsonObject.Value);
        }

        /// <summary>
        /// 解析成时间类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static DateTime ConvertToDateTime(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(DateTime), out IJsonCustomConverter converter) ?? false)
            {
                return ((JsonCustomConvert<DateTime>)converter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type != JsonType.String)
            {
                throw new JsonDeserializationException(jsonObject, typeof(DateTime), "Json对象不为String类型不能解析成DateTime类型");
            }
            return DateTime.Parse((string)jsonObject.Value);
        }

        // /// <summary>
        // /// 解析成枚举
        // /// </summary>
        // /// <param name="type"></param>
        // /// <param name="jsonObject">Json对象</param>
        // /// <param name="customConverters">自定义反序列化转换</param> 
        // /// <returns></returns>
        // private static object ConvertToEnum(Type type, IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        // {
        //     //自定义反序列化
        //     if(customConverters?.TryGetValue(type, out IJsonCustomConverter customConverter) ?? false)
        //     {
        //         return customConverter.Deserialize(jsonObject);
        //     }
        //     //默认反序列化
        //     if (jsonObject.Type == JsonType.String)
        //     {
        //         EnumConverter converter = new EnumConverter(type);
        //         return converter.ConvertFromString((string)jsonObject.Value);
        //     }
        //     else if (jsonObject.Type == JsonType.Number && ((JsonNumber)jsonObject).NumberType == NumberType.Long)
        //     {
        //         int value = int.Parse((string)jsonObject.Value);
        //         if (!System.Enum.IsDefined(type, value))
        //         {
        //             throw new JsonDeserializationException(jsonObject, type, "当前Number数字，在枚举中未定义");
        //         }
        //         return System.Enum.ToObject(type, value);
        //     }
        //     else
        //     {
        //         throw new JsonDeserializationException(jsonObject, type, "Json对象的" + jsonObject.Type.ToString() + "类型不能解析成Enum类型");
        //     }
        // }
        
        /// <summary>
        /// 解析成枚举
        /// </summary>
        /// <returns>枚举</returns>
        private static Expression<Func<IJsonObject, T>> ConvertToEnum()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            var curType = typeof(T);
            List<Expression> expres = new List<Expression>();
            var result = Expression.Variable(typeof(StructConvertResult<>).MakeGenericType(curType));

            //判断是否是JsonContent 如果不是抛出异常
            expres.Add(Expression.IfThen(Expression.AndAlso(Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.String)),
                    Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Number))),
                Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不为string，number不能解析成枚举类型")))));

            var method = typeof(StructConvertResult<>).MakeGenericType(curType).GetMethod("ConvertToEnum", new Type[] { typeof(string) });
            expres.Add(Expression.Assign(result, Expression.Call(method, Expression.Call(jsonObjectParameter, "ToString", new Type[] { }))));
            expres.Add(Expression.IfThen(Expression.IsFalse(Expression.Property(result, "Success")), 
                Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不能解析成枚举类型")))));
            expres.Add(Expression.Property(result, "Result"));
            return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter);
        }




        /// <summary>
        /// 解析成字符类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="customConverters">自定义反序列化转换</param> 
        /// <returns></returns>
        private static string ConvertToString(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> customConverters)
        {
            //自定义反序列化
            if(customConverters?.TryGetValue(typeof(string), out IJsonCustomConverter customConverter) ?? false)
            {
                return ((JsonCustomConvert<string>)customConverter).Deserialize(jsonObject);
            }
            //默认反序列化
            if (jsonObject.Type == JsonType.String)
            {
                return (string)jsonObject.Value;
            }
            throw new JsonDeserializationException(jsonObject, typeof(string), "Json对象不为String类型不能解析成String类型");
        }

        /// <summary>
        /// 解析成字典类型
        /// </summary>
        /// <returns>解析数字典达式</returns>
        private static Expression<Func<IJsonObject, T>> ConvertToDictionary()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            var curType = typeof(T);
            var genericType = typeof(object);
#if NET40

            Type[] genericTypes = curType.GetGenericArguments();
#else
            Type[] genericTypes = curType.GenericTypeArguments;
#endif
            if (genericTypes.Length == 2)
            {
                genericType = genericTypes[1];
            }

            var jsonDictionaryValue = Expression.Variable(typeof(Dictionary<string, IJsonObject>), "jsonDictionaryValue");
            var result = Expression.Variable(curType, "result");

            var enumerator = Expression.Variable(typeof(Dictionary<string, IJsonObject>.Enumerator), "enumerator");
            var keyValue = Expression.Variable(typeof(KeyValuePair<string, IJsonObject>), "keyValue");
            var returnLabel = Expression.Label("returnLable");
            var loopLabel = Expression.Label("loopLabel");

            List<Expression> expres = new List<Expression>();
            List<Expression> loopexpres = new List<Expression>();

            //判断json是否为null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));

            //判断是否是JsonContent 如果不是抛出异常
            expres.Add(Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Content)),
                Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不为Content类型不能解析成Dictionary类型")))));

            //判断是否能够分配给Dictionary<string, IJsonObject>类型
            if (curType.IsAssignableFrom(typeof(Dictionary<string, IJsonObject>)))
            {
                expres.Add(Expression.Assign(result, Expression.Call(Expression.Convert(jsonObjectParameter, typeof(JsonContent)), "GetValue", new Type[] { })));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter);
            }
            //判断是否可以实例化对象
            if (curType.GetConstructor(new Type[] { }) == null)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("反序列化类型没有默认的构造函数，无法创建该类型对象"))));
                return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(expres), jsonObjectParameter);
            }
            expres.Add(Expression.Assign(result, Expression.New(curType)));
            expres.Add(Expression.Assign(jsonDictionaryValue, Expression.Call(Expression.Convert(jsonObjectParameter, typeof(JsonContent)), "GetValue", new Type[] { })));
            expres.Add(Expression.Assign(enumerator, Expression.Call(jsonDictionaryValue, "GetEnumerator", new Type[] { })));

            loopexpres.Add(Expression.IfThen(Expression.IsFalse(Expression.Call(enumerator, "MoveNext", new Type[] { })), Expression.Break(loopLabel)));
            loopexpres.Add(Expression.Assign(keyValue, Expression.Property(enumerator, "Current")));

            //TODO 后期可以优化，如果是特殊类型如Dictionary类型，可直接调用Add<Tkey, Tvalue>()方法避免拆箱装箱和类型转换操作提升性能

            //值类型需要装箱操作
            if (genericType.IsValueType)
            {
                loopexpres.Add(Expression.Call(result, typeof(IDictionary).GetMethod("Add", new Type[] { typeof(object), typeof(object) }), Expression.Property(keyValue, "Key"),
                    Expression.Convert(Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject) }), Expression.Property(keyValue, "Value")), typeof(object))));
            }
            else 
            {
                loopexpres.Add(Expression.Call(result, typeof(IDictionary).GetMethod("Add", new Type[] { typeof(object), typeof(object) }), Expression.Property(keyValue, "Key"),
                    Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject) }), Expression.Property(keyValue, "Value"))));
            }

            expres.Add(Expression.Loop(Expression.Block(loopexpres), loopLabel));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);

            return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { jsonDictionaryValue, result, enumerator, keyValue }, expres), jsonObjectParameter);
        }

        /// <summary>
        /// 解析成数组
        /// </summary>
        /// <returns>解析数组表达式</returns>
        private static Expression<Func<IJsonObject, T>> ConvertToArray()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            var curType = typeof(T);
            var elementType = curType.GetElementType();


            var jsonArray = Expression.Variable(typeof(JsonArray), "jsonArra");
            var jsonArrayValue = Expression.Variable(typeof(List<IJsonObject>), "jsonArrayValue");
            var result = Expression.Variable(curType, "result");

            var i = Expression.Variable(typeof(int), "i");
            var length = Expression.Variable(typeof(int), "length");
            var returnLabel = Expression.Label("returnLable");
            var loopLabel = Expression.Label("loopLabel");

            List<Expression> expres = new List<Expression>();
            List<Expression> loopexpres = new List<Expression>();
            //判断json是否为null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));
            //判断Json对象是否是数组类型
            expres.Add(
            Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Array)),
                Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不为Array类型不能解析成Array类型")))));
            //初始化数组
            expres.Add(Expression.Assign(jsonArray, Expression.Convert(jsonObjectParameter, typeof(JsonArray))));
            expres.Add(Expression.Assign(length, Expression.Property(jsonArray, "Length")));
            expres.Add(Expression.Assign(jsonArrayValue, Expression.Call(jsonArray, "GetValue", new Type[] { })));
            expres.Add(Expression.Assign(result, Expression.NewArrayBounds(elementType, length)));
            expres.Add(Expression.Assign(i, Expression.Constant(0)));
            //循环数组赋值
            loopexpres.Add(Expression.IfThen(Expression.GreaterThanOrEqual(i, length), Expression.Break(loopLabel)));
            var item = Expression.Call(jsonArrayValue, typeof(List<IJsonObject>).GetMethod("get_Item"), i);
            loopexpres.Add(Expression.Call(result, curType.GetMethod("Set"), i, Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(elementType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject) }), item)));
            loopexpres.Add(Expression.Assign(i, Expression.Increment(i)));
            //loopexpres.Add(Expression.AddAssign(Expression.ArrayIndex(result, i),
            //    Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(elementType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject) }), item)));
            expres.Add(Expression.Loop(Expression.Block(loopexpres), loopLabel));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);
            return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { jsonArray, i, length, jsonArrayValue, result },expres), jsonObjectParameter);
        }
        /// <summary>
        /// 解析成列表集合
        /// </summary>
        /// <returns>列表集合</returns>
        private static Expression<Func<IJsonObject, T>> ConvertToList()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            var curType = typeof(T);
            //获取泛型类型
            var genericType = curType.GetGenericArguments().FirstOrDefault();
            

            var jsonArray = Expression.Variable(typeof(JsonArray), "jsonArra");
            var jsonArrayValue = Expression.Variable(typeof(List<IJsonObject>), "jsonArrayValue");
            var result = Expression.Variable(curType, "result");

            var i = Expression.Variable(typeof(int), "i");
            var length = Expression.Variable(typeof(int), "length");
            var returnLabel = Expression.Label("returnLable");
            var label = Expression.Label();

            List<Expression> expres = new List<Expression>();
            List<Expression> loopexpres = new List<Expression>();

            if (genericType == null)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject),
                    typeof(Type), typeof(string) }), jsonObjectParameter, Expression.Constant(curType), Expression.Constant("泛型列化出错获取List泛型出错！"))));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter);
            }

            //判断json是否为null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));

            //判断Json对象是否是数组类型
            expres.Add(
            Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Array)),
                Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不为Array类型不能解析成List类型")))));
            //初始化List
            expres.Add(Expression.Assign(jsonArray, Expression.Convert(jsonObjectParameter, typeof(JsonArray))));
            expres.Add(Expression.Assign(length, Expression.Property(jsonArray, "Length")));
            expres.Add(Expression.Assign(jsonArrayValue, Expression.Call(jsonArray, "GetValue", new Type[] { })));
            expres.Add(Expression.Assign(result, Expression.New(curType)));
            expres.Add(Expression.Assign(i, Expression.Constant(0)));
            //循环List赋值
            loopexpres.Add(Expression.IfThen(Expression.GreaterThanOrEqual(i, length), Expression.Break(label)));
            var item = Expression.Call(jsonArrayValue, typeof(List<IJsonObject>).GetMethod("get_Item"), i);
            loopexpres.Add(Expression.Call(result, curType.GetMethod("Add", new Type[] { genericType }), 
                Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject) }), item)));
            loopexpres.Add(Expression.Assign(i, Expression.Increment(i)));
            expres.Add(Expression.Loop(Expression.Block(loopexpres), label));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);
            return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { jsonArray, i, length, jsonArrayValue, result }, expres), jsonObjectParameter);

        }

        /// <summary>
        /// 解析成对象
        /// </summary>
        /// <returns>对象</returns>
        private static Expression<Func<IJsonObject, T>> ConvertToObject()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            var curType = typeof(T);

            List<Expression> expres = new List<Expression>();
            List<MemberBinding> members = new List<MemberBinding>();
            List<ParameterExpression> parameters = new List<ParameterExpression>();

            var result = Expression.Variable(curType, "result");
            parameters.Add(result);

            var returnLabel = Expression.Label("returnLable");

            //判断json是否为null 值类型不能为null
            if (curType.IsValueType)
            {
               expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
               Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
               jsonObjectParameter, Expression.Constant(curType), Expression.Constant("反序列化失败，无法将json的null类型转换成值类型")))));
            }
            else
            {
                expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));
            }

            ////判断是否是JsonContent 如果不是抛出异常
            //expres.Add(Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Content)),
            //    Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
            //    jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不为Content类型不能解析成object类型")))));

            if (curType.IsAssignableFrom(typeof(JsonContent)))
            {
                expres.Add(Expression.Assign(result, jsonObjectParameter));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter);
            }
            if (curType.IsAssignableFrom(typeof(Dictionary<string, IJsonObject>)))
            {
                expres.Add(Expression.Assign(result, Expression.Call(Expression.Convert(jsonObjectParameter, typeof(JsonContent)), "GetValue", new Type[] { })));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter);
            }
            //判断是否是接口或者抽象类型
            if (curType.IsInterface || curType.IsAbstract)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("类型为接口或者是抽象接口，无法创建该类型对象"))));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter);
            }
            //判断是否有默认的构造函数
            if (curType.GetConstructor(new Type[] { }) == null)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("反序列化类型没有默认的构造函数，无法创建该类型对象"))));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter);
            }
            //初始化赋值每个属性
            foreach (var item in curType.GetProperties().Where(n=>n.CanWrite))
            {
                //判断是否忽略反序列化
                var jsonIgnored = Attribute.GetCustomAttribute(item, typeof(JsonIgnoredAttribute)) as JsonIgnoredAttribute;
                if (jsonIgnored != null && (jsonIgnored.JsonIgnoredMethod & JsonMethods.Deserialize) == JsonMethods.Deserialize)
                {
                    break;
                }

                var getJsonObjItem = Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(item.PropertyType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject) }),
                        Expression.Call(jsonObjectParameter, typeof(IJsonObject).GetMethod("get_Item", new Type[] { typeof(string) }), Expression.Constant(item.Name)));
                var hasChildrenNode = Expression.Call(jsonObjectParameter, typeof(IJsonObject).GetMethod("HasChildrenNode", new Type[] { typeof(string) }), Expression.Constant(item.Name));

                var propertyValue = Expression.Variable(item.PropertyType, item.Name + "propertyValue");
                parameters.Add(propertyValue);

                expres.Add(Expression.IfThenElse(Expression.IsTrue(hasChildrenNode), Expression.Assign(propertyValue, getJsonObjItem), Expression.Assign(propertyValue, Expression.Default(item.PropertyType))));
                //members.Add()
                members.Add(Expression.Bind(item, propertyValue));
                //TODO 如有三元运算表达式，可替换成三元表达式
                //members.Add(Expression.Bind(item, Expression.IfThenElse(Expression.IsTrue(hasChildrenNode), getJsonObjItem, Expression.Default(item.PropertyType))));


            }
            expres.Add(Expression.Assign(result, Expression.MemberInit(Expression.New(curType), members)));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);

            return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(parameters, expres), jsonObjectParameter);
        }

        private static Expression<Func<IJsonObject, T>> ConvertToNullable()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            var curType = typeof(T);

            List<Expression> expres = new List<Expression>();

            var result = Expression.Variable(curType, "result");
            var returnLabel = Expression.Label("returnLable");

            //获取泛型类型
            var genericType = curType.GetGenericArguments().FirstOrDefault();
            if (genericType == null)
            {
                return Expression.Lambda<Func<IJsonObject, T>>(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject),
                    typeof(Type), typeof(string) }), jsonObjectParameter, Expression.Constant(curType), Expression.Constant("反序列化出错获取，获取Nullable泛型出错！"))), jsonObjectParameter);
            }
            //判断json是否为null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));
            expres.Add(Expression.Assign(result, Expression.New(curType.GetConstructor(new Type[] { genericType }),
                Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject) }), jsonObjectParameter))));

            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);

            return Expression.Lambda<Func<IJsonObject, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter);
        }
    }
}
