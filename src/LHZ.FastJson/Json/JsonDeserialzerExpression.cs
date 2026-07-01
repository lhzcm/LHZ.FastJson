using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LHZ.FastJson;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Interface;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.Utils;
using LHZ.FastJson.JsonClass;
using LHZ.FastJson.Wrapper;

namespace LHZ.FastJson.Json
{
    internal static class JsonDeserialzerExpression<T>
    {
        private static readonly Dictionary<Type, ObjectType> _objectTypes = JsonObjectType.GetObjectTypes();

        private static readonly Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T> _funcDeserialize = null;
        private static readonly Type _type = typeof(T);

        static JsonDeserialzerExpression()
        {
            var funcDeserializeExpression = CreateExpression();
            _funcDeserialize = funcDeserializeExpression.Compile();
        }

        public static T Deserialzer(IJsonObject jsonObject)
        {
            if(jsonObject == null)
                return default(T);
            return _funcDeserialize(jsonObject, null);
        }
        public static T Deserialzer(IJsonObject jsonObject, Dictionary<Type, IJsonCustomConverter> jsonCustomConverters)
        {
            if(jsonObject == null)
                return default(T);
            if (jsonCustomConverters != null && jsonCustomConverters.TryGetValue(_type, out IJsonCustomConverter customConverter))
            {
                return (T)customConverter.Deserialize(jsonObject);
            }
            return _funcDeserialize(jsonObject, jsonCustomConverters);
        }

        /// <summary>
        /// Get deserialization expression tree
        /// </summary>
        /// <returns></returns>
        private static Expression<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>> CreateExpression()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObject");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");
            var curType = _type;
            ObjectType objectType = GetObjectType(curType);
            switch (objectType)
            {
                case ObjectType.Boolean:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, bool>)ConvertToBoolean).Method, jsonObjectParameter), jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Byte:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, byte>)ConvertToByte).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Char:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, char>)ConvertToChar).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Int16:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, short>)ConvertToInt16).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.UInt16:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, ushort>)ConvertToUInt16).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Int32:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, int>)ConvertToInt32).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.UInt32:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, uint>)ConvertToUInt32).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Int64:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, long>)ConvertToInt64).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.UInt64:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, ulong>)ConvertToUInt64).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Float:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, float>)ConvertToFloat).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Double:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, double>)ConvertToDouble).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Decimal:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, decimal>)ConvertToDecimal).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.DateTime:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, DateTime>)ConvertToDateTime).Method, jsonObjectParameter), jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Guid:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<IJsonObject, Guid>)ConvertToGuid).Method, jsonObjectParameter), jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.String:
                    return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call( 
                        ((Func<IJsonObject, string>)ConvertToString).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
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
        /// Get the type of the object
        /// </summary>
        /// <param name="type">Deserialization object type</param>
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
            else if (type == typeof(Guid))
                return ObjectType.Guid;
            //else if (typeof(IEnumerable).IsAssignableFrom(type))
            //    return ObjectType.Enumerable;
            else
                return ObjectType.Object;

        }

        /// <summary>
        /// Parse to bool type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static bool ConvertToBoolean(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Boolean)
            {
                throw new JsonDeserializationException(jsonObject, typeof(bool), "Json对象不为Boolean类型不能解析成Boolean类型");
            }
            return Boolean.Parse(jsonObject.Value.ToString());
        }

        /// <summary>
        /// Parse to byte type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static byte ConvertToByte(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Byte), "Json对象不为Number类型不能解析成Byte类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Byte), "Json对象Number类型不为Long不能解析成Byte类型");
            }
            return Byte.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to char type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static char ConvertToChar(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.String)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Char), "Json对象不为String类型不能解析成Char类型");
            }
            if ((jsonObject.Value.ToString()).Length != 1)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Char), "Json对象字符串长度不等于1，不能解析成Char类型");
            }
            return (jsonObject.Value.ToString())[0];
        }

        /// <summary>
        /// Parse to int16 type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static short ConvertToInt16(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Int16), "Json对象不为Number类型不能解析成Int16类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Int16), "Json对象Number类型不为Long不能解析成Int16类型");
            }
            return Int16.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to uint16 type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static ushort ConvertToUInt16(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(UInt16), "Json对象不为Number类型不能解析成UInt16类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(UInt16), "Json对象Number类型不为Long不能解析成UInt16类型");
            }
            return UInt16.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Parse to int32 type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static int ConvertToInt32(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(int), "Json对象不为Number类型不能解析成Int32类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(int), "Json对象Number类型不为Long不能解析成Int32类型");
            }
            return int.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Parse to uint32 type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static uint ConvertToUInt32(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(uint), "Json对象不为Number类型不能解析成UInt32类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(uint), "Json对象Number类型不为Long不能解析成UInt32类型");
            }
            return uint.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to int64 type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static long ConvertToInt64(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(long), "Json对象不为Number类型不能解析成Int64类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(long), "Json对象Number类型不为Long不能解析成Int64类型");
            }
            return long.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to uint64 type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static ulong ConvertToUInt64(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(ulong), "Json对象不为Number类型不能解析成UInt64类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(ulong), "Json对象Number类型不为Long不能解析成UInt64类型");
            }
            return ulong.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to single-precision floating-point type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static float ConvertToFloat(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(float), "Json对象不为Number类型不能解析成Float类型");
            }
            return float.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Parse to double-precision floating-point type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static double ConvertToDouble(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(double), "Json对象不为Number类型不能解析成Double类型");
            }
            return double.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Parse to decimal type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static decimal ConvertToDecimal(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(decimal), "Json对象不为Number类型不能解析成Decimal类型");
            }
            return decimal.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to DateTime type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static DateTime ConvertToDateTime(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.String)
            {
                throw new JsonDeserializationException(jsonObject, typeof(DateTime), "Json对象不为String类型不能解析成DateTime类型");
            }
            return DateTime.Parse(jsonObject.Value.ToString());
        }

        /// <summary>
        /// Parse to enum
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static object ConvertToEnum(Type type, IJsonObject jsonObject)
        {
            if (jsonObject.Type == JsonType.String)
            {
                EnumConverter converter = new EnumConverter(type);
                return converter.ConvertFromString(jsonObject.Value.ToString());
            }
            else if (jsonObject.Type == JsonType.Number && ((JsonNumber)jsonObject).NumberType == NumberType.Long)
            {
                int value = int.Parse(jsonObject.Value.ToString(), CultureInfo.InvariantCulture);
                if (!System.Enum.IsDefined(type, value))
                {
                    throw new JsonDeserializationException(jsonObject, type, "当前Number数字，在枚举中未定义");
                }
                return System.Enum.ToObject(type, value);
            }
            else
            {
                throw new JsonDeserializationException(jsonObject, type, "Json对象的" + jsonObject.Type.ToString() + "类型不能解析成Enum类型");
            }
        }
        /// <summary>
        /// Parse to GUID type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static Guid ConvertToGuid(IJsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.String)
            {
                throw new JsonDeserializationException(jsonObject, typeof(Guid), "Json对象不为String类型不能解析成Guid类型");
            }
            return Guid.Parse(jsonObject.Value.ToString());
        }
        /// <summary>
        /// Parse to enum
        /// </summary>
        /// <returns>Enum</returns>
        private static Expression<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToEnum()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");

            var curType = _type;
            List<Expression> expres = new List<Expression>();
            var result = Expression.Variable(typeof(StructConvertResult<>).MakeGenericType(curType));

            //Determine if it is JsonContent; if not, throw an exception
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
            return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }




        /// <summary>
        /// Parse to string type
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns></returns>
        private static string ConvertToString(IJsonObject jsonObject)
        {
            if (jsonObject.Type == JsonType.String)
            {
                return jsonObject.Value.ToString();
            }
            if(jsonObject.Type == JsonType.Null)
            {
                return null;
            }
            throw new JsonDeserializationException(jsonObject, typeof(string), "Json对象不为String类型不能解析成String类型");
        }

        /// <summary>
        /// Parse to dictionary type
        /// </summary>
        /// <returns>Deserialized dictionary expression</returns>
        private static Expression<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToDictionary()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");
            var curType = _type;
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

            //Determine if the JSON object is null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));

            //Determine if it is JsonContent; if not, throw an exception
            expres.Add(Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Content)),
                Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不为Content类型不能解析成Dictionary类型")))));

            //Determine if it can be assigned to Dictionary<string, IJsonObject> type
            if (curType.IsAssignableFrom(typeof(Dictionary<string, IJsonObject>)))
            {
                expres.Add(Expression.Assign(result, Expression.Call(Expression.Convert(jsonObjectParameter, typeof(JsonContent)), "GetValue", new Type[] { })));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            //Determine if the object can be instantiated
            if (curType.GetConstructor(new Type[] { }) == null)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("反序列化类型没有默认的构造函数，无法创建该类型对象"))));
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            expres.Add(Expression.Assign(result, Expression.New(curType)));
            expres.Add(Expression.Assign(jsonDictionaryValue, Expression.Call(Expression.Convert(jsonObjectParameter, typeof(JsonContent)), "GetValue", new Type[] { })));
            expres.Add(Expression.Assign(enumerator, Expression.Call(jsonDictionaryValue, "GetEnumerator", new Type[] { })));

            loopexpres.Add(Expression.IfThen(Expression.IsFalse(Expression.Call(enumerator, "MoveNext", new Type[] { })), Expression.Break(loopLabel)));
            loopexpres.Add(Expression.Assign(keyValue, Expression.Property(enumerator, "Current")));

            //TODO: This can be optimized later. For special types like Dictionary, you can directly call Add<Tkey, Tvalue>() method to avoid boxing/unboxing and type conversion operations for better performance

            //Value types require boxing
            if (genericType.IsValueType)
            {
                loopexpres.Add(Expression.Call(result, typeof(IDictionary).GetMethod("Add", new Type[] { typeof(object), typeof(object) }), Expression.Property(keyValue, "Key"),
                    Expression.Convert(Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject), typeof(Dictionary<Type, IJsonCustomConverter>) }), Expression.Property(keyValue, "Value"), jsonCustomConvertersParameter), typeof(object))));
            }
            else
            {
                loopexpres.Add(Expression.Call(result, typeof(IDictionary).GetMethod("Add", new Type[] { typeof(object), typeof(object) }), Expression.Property(keyValue, "Key"),
                    Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject), typeof(Dictionary<Type, IJsonCustomConverter>) }), Expression.Property(keyValue, "Value"), jsonCustomConvertersParameter)));
            }

            expres.Add(Expression.Loop(Expression.Block(loopexpres), loopLabel));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);

            return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { jsonDictionaryValue, result, enumerator, keyValue }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }

        /// <summary>
        /// Parse to array
        /// </summary>
        /// <returns>Deserialized array expression</returns>
        private static Expression<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToArray()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");

            var curType = _type;
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
            //Determine if the JSON object is null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));
            //Determine if the JSON object is an array type
            expres.Add(
            Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Array)),
                Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不为Array类型不能解析成Array类型")))));
            //Initialize the array
            expres.Add(Expression.Assign(jsonArray, Expression.Convert(jsonObjectParameter, typeof(JsonArray))));
            expres.Add(Expression.Assign(length, Expression.Property(jsonArray, "Length")));
            expres.Add(Expression.Assign(jsonArrayValue, Expression.Call(jsonArray, "GetValue", new Type[] { })));
            expres.Add(Expression.Assign(result, Expression.NewArrayBounds(elementType, length)));
            expres.Add(Expression.Assign(i, Expression.Constant(0)));
            //Loop to assign array values
            loopexpres.Add(Expression.IfThen(Expression.GreaterThanOrEqual(i, length), Expression.Break(loopLabel)));
            var item = Expression.Call(jsonArrayValue, typeof(List<IJsonObject>).GetMethod("get_Item"), i);
            loopexpres.Add(Expression.Call(result, curType.GetMethod("Set"), i, Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(elementType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject), typeof(Dictionary<Type, IJsonCustomConverter>) }), item, jsonCustomConvertersParameter)));
            loopexpres.Add(Expression.Assign(i, Expression.Increment(i)));
            //loopexpres.Add(Expression.AddAssign(Expression.ArrayIndex(result, i),
            //    Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(elementType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject) }), item)));
            expres.Add(Expression.Loop(Expression.Block(loopexpres), loopLabel));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);
            return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { jsonArray, i, length, jsonArrayValue, result },expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }
        /// <summary>
        /// Parse to list collection
        /// </summary>
        /// <returns>List collection</returns>
        private static Expression<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToList()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");

            var curType = _type;
            //Get the generic type
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
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }

            //Determine if the JSON object is null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));

            //Determine if the JSON object is an array type
            expres.Add(
            Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Array)),
                Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("Json对象不为Array类型不能解析成List类型")))));
            //Initialize List
            expres.Add(Expression.Assign(jsonArray, Expression.Convert(jsonObjectParameter, typeof(JsonArray))));
            expres.Add(Expression.Assign(length, Expression.Property(jsonArray, "Length")));
            expres.Add(Expression.Assign(jsonArrayValue, Expression.Call(jsonArray, "GetValue", new Type[] { })));
            expres.Add(Expression.Assign(result, Expression.New(curType)));
            expres.Add(Expression.Assign(i, Expression.Constant(0)));
            //Loop to assign List values
            loopexpres.Add(Expression.IfThen(Expression.GreaterThanOrEqual(i, length), Expression.Break(label)));
            var item = Expression.Call(jsonArrayValue, typeof(List<IJsonObject>).GetMethod("get_Item"), i);
            loopexpres.Add(Expression.Call(result, curType.GetMethod("Add", new Type[] { genericType }),
                Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject), typeof(Dictionary<Type, IJsonCustomConverter>) }), item, jsonCustomConvertersParameter)));
            loopexpres.Add(Expression.Assign(i, Expression.Increment(i)));
            expres.Add(Expression.Loop(Expression.Block(loopexpres), label));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);
            return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { jsonArray, i, length, jsonArrayValue, result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);

        }

        /// <summary>
        /// Parse to object
        /// </summary>
        /// <returns>Object</returns>
        private static Expression<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToObject()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");

            var curType = _type;

            List<Expression> expres = new List<Expression>();
            List<MemberBinding> members = new List<MemberBinding>();
            List<ParameterExpression> parameters = new List<ParameterExpression>();

            var result = Expression.Variable(curType, "result");
            parameters.Add(result);

            var returnLabel = Expression.Label("returnLable");

            //Determine if the JSON object is null; value types cannot be null
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
            //The object can be assigned to an IJsonObject type variable 
            if (curType.IsAssignableFrom(typeof(IJsonObject)))
            {
                expres.Add(Expression.Assign(result, jsonObjectParameter));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            if (typeof(IJsonObject).IsAssignableFrom(curType))
            {
                expres.Add(Expression.Assign(result, Expression.Convert(jsonObjectParameter, curType)));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            if (curType.IsAssignableFrom(typeof(Dictionary<string, IJsonObject>)))
            {
                expres.Add(Expression.Assign(result, Expression.Call(Expression.Convert(jsonObjectParameter, typeof(JsonContent)), "GetValue", new Type[] { })));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            //Check if the type is an interface or abstract type
            if (curType.IsInterface || curType.IsAbstract)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("类型为接口或者是抽象接口，无法创建该类型对象"))));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            //Check if the type has a default constructor
            if (curType.GetConstructor(new Type[] { }) == null)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject), typeof(Type), typeof(string) }),
                jsonObjectParameter, Expression.Constant(curType), Expression.Constant("反序列化类型没有默认的构造函数，无法创建该类型对象"))));
                expres.Add(Expression.Label(returnLabel));
                expres.Add(result);
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            //Initialize and assign each property
            foreach (var item in curType.GetProperties().Where(n=>n.CanWrite))
            {
                //Check if deserialization should be ignored
                var jsonIgnored = Attribute.GetCustomAttribute(item, typeof(JsonIgnoredAttribute)) as JsonIgnoredAttribute;
                if (jsonIgnored != null && (jsonIgnored.JsonIgnoredMethod & JsonMethods.Deserialize) == JsonMethods.Deserialize)
                {
                    continue;
                }
                #region Custom property name handling
                StringView jsonPropertyNameStringView = new StringView(JsonUtility.GetPropertyName(item));
                var jsonPropertyName = new JsonPropertyName(jsonPropertyNameStringView, jsonPropertyNameStringView.GetHashCode());
                #endregion
                
                var getJsonObjItem = Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(item.PropertyType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject), typeof(Dictionary<Type, IJsonCustomConverter>) }),
                        Expression.Call(jsonObjectParameter, typeof(IJsonObject).GetMethod("get_Item", new Type[] { typeof(JsonPropertyName) }), Expression.Constant(jsonPropertyName)), jsonCustomConvertersParameter);
                var propertyValue = Expression.Variable(item.PropertyType, item.Name + "propertyValue");
                parameters.Add(propertyValue);

                expres.Add(Expression.Assign(propertyValue, getJsonObjItem));
                //members.Add()
                members.Add(Expression.Bind(item, propertyValue));
                //TODO: If ternary expression is available, it can be replaced with ternary expression
                //members.Add(Expression.Bind(item, Expression.IfThenElse(Expression.IsTrue(hasChildrenNode), getJsonObjItem, Expression.Default(item.PropertyType))));


            }
            expres.Add(Expression.Assign(result, Expression.MemberInit(Expression.New(curType), members)));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);

            return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(parameters, expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }

        private static Expression<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToNullable()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(IJsonObject), "jsonObjectParameter");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");
            var curType = _type;

            List<Expression> expres = new List<Expression>();

            var result = Expression.Variable(curType, "result");
            var returnLabel = Expression.Label("returnLable");

            //Get the generic type
            var genericType = curType.GetGenericArguments().FirstOrDefault();
            if (genericType == null)
            {
                return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Throw(Expression.New(typeof(JsonDeserializationException).GetConstructor(new Type[] { typeof(IJsonObject),
                    typeof(Type), typeof(string) }), jsonObjectParameter, Expression.Constant(curType), Expression.Constant("反序列化出错获取，获取Nullable泛型出错！"))), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            //Determine if the JSON object is null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "Type"), Expression.Constant(JsonType.Null)),
                Expression.Return(returnLabel)));
            expres.Add(Expression.Assign(result, Expression.New(curType.GetConstructor(new Type[] { genericType }),
                Expression.Call(typeof(JsonDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(IJsonObject), typeof(Dictionary<Type, IJsonCustomConverter>) }), jsonObjectParameter, jsonCustomConvertersParameter))));

            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);

            return Expression.Lambda<Func<IJsonObject, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }
    }
}
