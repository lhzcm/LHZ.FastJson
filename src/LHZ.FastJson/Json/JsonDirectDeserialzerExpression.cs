using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LHZ.FastJson;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Interface;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.Utils;
using LHZ.FastJson.JsonClass;
using LHZ.FastJson.Utils;

namespace LHZ.FastJson.Json
{
    internal class JsonDirectDeserialzerExpression<T>
    {
        private static readonly Dictionary<Type, ObjectType> _objectTypes = JsonObjectType.GetObjectTypes();

        private static readonly Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T> _funcDeserialize = null;
        private static readonly Type _type = typeof(T);

        static JsonDirectDeserialzerExpression()
        {
            var funcDeserializeExpression = CreateExpression();
            _funcDeserialize = funcDeserializeExpression.Compile();
        }

        public static T Deserialzer(JsonDirectReader jsonObject)
        {
            // if(jsonObject.IsReadEnd)
            //     return default(T);
            return _funcDeserialize(jsonObject, null);
        }
        public static T Deserialzer(JsonDirectReader jsonObject, Dictionary<Type, IJsonCustomConverter> jsonCustomConverters)
        {
            // if(jsonObject.IsReadEnd)
            //     return default(T);
            if (jsonCustomConverters != null && jsonCustomConverters.TryGetValue(_type, out IJsonCustomConverter customConverter))
            {
                return (T)customConverter.Deserialize(jsonObject.ReadAsJsonObject());
            }
            return _funcDeserialize(jsonObject, jsonCustomConverters);
        }

        /// <summary>
        /// Get deserialization expression tree
        /// </summary>
        /// <returns></returns>
        private static Expression<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>> CreateExpression()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(JsonDirectReader), "jsonDirectReader");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");
            var curType = _type;
            ObjectType objectType = GetObjectType(curType);
            switch (objectType)
            {
                case ObjectType.Boolean:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, bool>)ConvertToBoolean).Method, jsonObjectParameter), jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Byte:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, byte>)ConvertToByte).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Char:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, char>)ConvertToChar).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Int16:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, short>)ConvertToInt16).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.UInt16:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, ushort>)ConvertToUInt16).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Int32:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, int>)ConvertToInt32).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.UInt32:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, uint>)ConvertToUInt32).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Int64:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, long>)ConvertToInt64).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.UInt64:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, ulong>)ConvertToUInt64).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Float:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, float>)ConvertToFloat).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Double:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, double>)ConvertToDouble).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Decimal:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, decimal>)ConvertToDecimal).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.DateTime:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, DateTime>)ConvertToDateTime).Method, jsonObjectParameter), jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.Guid:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call(
                        ((Func<JsonDirectReader, Guid>)ConvertToGuid).Method, jsonObjectParameter), jsonObjectParameter, jsonCustomConvertersParameter);
                case ObjectType.String:
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Call( 
                        ((Func<JsonDirectReader, string>)ConvertToString).Method, jsonObjectParameter),  jsonObjectParameter, jsonCustomConvertersParameter);
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
            if (JsonObjectType.ObjectTypes.TryGetValue(type, out objType))
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
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static bool ConvertToBoolean(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Boolean)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(bool), "Json对象不为Boolean类型不能解析成Boolean类型");
            }
            return jsonDirectReader.ReadBoolean();
        }

        /// <summary>
        /// Parse to byte type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static byte ConvertToByte(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(Byte), "Json对象不为Number类型不能解析成Byte类型");
            }
            return jsonDirectReader.ReadNumber().ToByte(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to char type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static char ConvertToChar(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.String)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(Char), "Json对象不为String类型不能解析成Char类型");
            }
            var str = jsonDirectReader.ReadString();
            if (str.Length != 1)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(Char), "Json对象字符串长度不等于1，不能解析成Char类型");
            }
            return str[0];
        }

        /// <summary>
        /// Parse to int16 type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static short ConvertToInt16(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(Int16), "Json对象不为Number类型不能解析成Int16类型");
            }
            return jsonDirectReader.ReadNumber().ToInt16(CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Parse to uint16 type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static ushort ConvertToUInt16(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(UInt16), "Json对象不为Number类型不能解析成UInt16类型");
            }
            return jsonDirectReader.ReadNumber().ToUInt16(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to int32 type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static int ConvertToInt32(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(int), "Json对象不为Number类型不能解析成Int32类型");
            }
            return jsonDirectReader.ReadNumber().ToInt32(CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Parse to uint32 type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static uint ConvertToUInt32(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(uint), "Json对象不为Number类型不能解析成UInt32类型");
            }
            return jsonDirectReader.ReadNumber().ToUInt32(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to int64 type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static long ConvertToInt64(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(long), "Json对象不为Number类型不能解析成Int64类型");
            }
            return jsonDirectReader.ReadNumber().ToInt64(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to uint64 type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static ulong ConvertToUInt64(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(ulong), "Json对象不为Number类型不能解析成UInt64类型");
            }
            return jsonDirectReader.ReadNumber().ToUInt64(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to single-precision floating-point type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static float ConvertToFloat(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(float), "Json对象不为Number类型不能解析成Float类型");
            }
            return jsonDirectReader.ReadNumber().ToSingle(CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Parse to double-precision floating-point type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static double ConvertToDouble(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(double), "Json对象不为Number类型不能解析成Double类型");
            }
            return jsonDirectReader.ReadNumber().ToDouble(CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Parse to decimal type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static decimal ConvertToDecimal(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.Number)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(decimal), "Json对象不为Number类型不能解析成Decimal类型");
            }
            return jsonDirectReader.ReadNumber().ToDecimal(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse to DateTime type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static DateTime ConvertToDateTime(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.String)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(DateTime), "Json对象不为String类型不能解析成DateTime类型");
            }
            return DateTime.Parse(jsonDirectReader.ReadString(), CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Parse to enum
        /// </summary>
        /// <returns></returns>
        private static object ConvertToEnum(JsonDirectReader jsonDirectReader)
        {
            var type = _type;
            if (jsonDirectReader.JsonType == JsonType.String)
            {
                EnumConverter converter = new EnumConverter(type);
                return converter.ConvertFromString(jsonDirectReader.ReadString());
            }
            else if (jsonDirectReader.JsonType == JsonType.Number)
            {
                int value = jsonDirectReader.ReadNumber().ToInt32(CultureInfo.InvariantCulture);
                if (!System.Enum.IsDefined(type, value))
                {
                    throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, type, "当前Number数字，在枚举中未定义");
                }
                return System.Enum.ToObject(type, value);
            }
            else
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, type, "Json对象的" + jsonDirectReader.JsonType.ToString() + "类型不能解析成Enum类型");
            }
        }
        /// <summary>
        /// Parse to GUID type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static Guid ConvertToGuid(JsonDirectReader jsonDirectReader)
        {
            if (jsonDirectReader.JsonType != JsonType.String)
            {
                throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(Guid), "Json对象不为String类型不能解析成Guid类型");
            }
            return Guid.Parse(jsonDirectReader.ReadString());
        }
        /// <summary>
        /// Parse to enum
        /// </summary>
        /// <returns>Enum</returns>
        private static Expression<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToEnum()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(JsonDirectReader), "jsonDirectReader");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");

            return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(
                Expression.Convert(
                    Expression.Call(((Func<JsonDirectReader, object>)ConvertToEnum).Method, jsonObjectParameter),
                    _type),
                jsonObjectParameter, jsonCustomConvertersParameter);
        }




        /// <summary>
        /// Parse to string type
        /// </summary>
        /// <param name="jsonDirectReader">JSON Direct Reader</param>
        /// <returns></returns>
        private static string ConvertToString(JsonDirectReader jsonDirectReader)
        {
            switch(jsonDirectReader.JsonType)
            {
                case JsonType.String : return jsonDirectReader.ReadString();
                case JsonType.Null : return jsonDirectReader.ReadNull<string>();
                default: throw new JsonDirectDeserializationException(jsonDirectReader.Position, jsonDirectReader.JsonType, typeof(string), "Json对象不为String类型不能解析成String类型");
            }
        }

        /// <summary>
        /// Parse to dictionary type
        /// </summary>
        /// <returns>Deserialized dictionary expression</returns>
        private static Expression<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToDictionary()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(JsonDirectReader), "jsonDirectReader");
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

            var result = Expression.Variable(curType, "result");
            var enumerator = Expression.Variable(typeof(IEnumerator<KeyValuePair<JsonPropertyName, JsonDirectReader>>), "enumerator");
            var keyValue = Expression.Variable(typeof(KeyValuePair<JsonPropertyName, JsonDirectReader>), "keyValue");
            var returnLabel = Expression.Label("returnLable");
            var loopLabel = Expression.Label("loopLabel");

            List<Expression> expres = new List<Expression>();
            List<Expression> loopexpres = new List<Expression>();

            //Determine if the JSON object is null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Null)),
                Expression.Block(new Expression[]{Expression.Call(jsonObjectParameter, "SkipCurrentObject", EmptyArray<Type>.Value), Expression.Return(returnLabel)})));

            //Determine if it is JsonContent; if not, throw an exception
            expres.Add(Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Content)),
                Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                Expression.Property(jsonObjectParameter, "Position"), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType), Expression.Constant("Json对象不为Content类型不能解析成Dictionary类型")))));

            //Determine if the object can be instantiated
            if (curType.GetConstructor(EmptyArray<Type>.Value) == null)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                Expression.Constant(0), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType), Expression.Constant("反序列化类型没有默认的构造函数，无法创建该类型对象"))));
                return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            expres.Add(Expression.Assign(result, Expression.New(curType)));
            expres.Add(Expression.Assign(enumerator, Expression.Call(
                Expression.Call(jsonObjectParameter, typeof(JsonDirectReader).GetMethod("ReadContent")),
                typeof(IEnumerable<KeyValuePair<JsonPropertyName, JsonDirectReader>>).GetMethod("GetEnumerator"))));

            loopexpres.Add(Expression.IfThen(Expression.IsFalse(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"))), Expression.Break(loopLabel)));
            loopexpres.Add(Expression.Assign(keyValue, Expression.Property(enumerator, "Current")));

            //Value types require boxing
            if (genericType.IsValueType)
            {
                loopexpres.Add(Expression.Call(result, typeof(IDictionary).GetMethod("Add", new Type[] { typeof(object), typeof(object) }), Expression.Call(Expression.Property(keyValue, "Key"), "ToString", EmptyArray<Type>.Value),
                    Expression.Convert(Expression.Call(typeof(JsonDirectDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(JsonDirectReader), typeof(Dictionary<Type, IJsonCustomConverter>) }), Expression.Property(keyValue, "Value"), jsonCustomConvertersParameter), typeof(object))));
            }
            else
            {
                loopexpres.Add(Expression.Call(result, typeof(IDictionary).GetMethod("Add", new Type[] { typeof(object), typeof(object) }), Expression.Call(Expression.Property(keyValue, "Key"), "ToString", EmptyArray<Type>.Value),
                    Expression.Call(typeof(JsonDirectDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(JsonDirectReader), typeof(Dictionary<Type, IJsonCustomConverter>) }), Expression.Property(keyValue, "Value"), jsonCustomConvertersParameter)));
            }

            expres.Add(Expression.Loop(Expression.Block(loopexpres), loopLabel));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);

            return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result, enumerator, keyValue }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }

        /// <summary>
        /// Parse to array
        /// </summary>
        /// <returns>Deserialized array expression</returns>
        private static Expression<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToArray()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(JsonDirectReader), "jsonDirectReader");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");

            var curType = _type;
            var elementType = curType.GetElementType();

            var result = Expression.Variable(curType, "result");
            var list = Expression.Variable(typeof(List<>).MakeGenericType(elementType), "list");
            var enumerator = Expression.Variable(typeof(IEnumerator<JsonDirectReader>), "enumerator");
            var item = Expression.Variable(typeof(JsonDirectReader), "item");
            var i = Expression.Variable(typeof(int), "i");
            var returnLabel = Expression.Label("returnLable");
            var loopLabel = Expression.Label("loopLabel");

            List<Expression> expres = new List<Expression>();
            List<Expression> loopexpres = new List<Expression>();

            //Determine if the JSON object is null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Null)),
                Expression.Block(new Expression[]{Expression.Call(jsonObjectParameter, "SkipCurrentObject", EmptyArray<Type>.Value), Expression.Return(returnLabel)})));
            //Determine if the JSON object is an array type
            expres.Add(
            Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Array)),
                Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                Expression.Property(jsonObjectParameter, "Position"), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType), Expression.Constant("Json对象不为Array类型不能解析成Array类型")))));

            //Initialize list and enumerator
            expres.Add(Expression.Assign(list, Expression.New(typeof(List<>).MakeGenericType(elementType))));
            expres.Add(Expression.Assign(enumerator, Expression.Call(
                Expression.Call(jsonObjectParameter, typeof(JsonDirectReader).GetMethod("ReadArray")),
                typeof(IEnumerable<JsonDirectReader>).GetMethod("GetEnumerator"))));

            //Loop
            loopexpres.Add(Expression.IfThen(Expression.IsFalse(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"))), Expression.Break(loopLabel)));
            loopexpres.Add(Expression.Assign(item, Expression.Convert(Expression.Property(enumerator, "Current"), typeof(JsonDirectReader))));
            loopexpres.Add(Expression.Call(list, typeof(List<>).MakeGenericType(elementType).GetMethod("Add"),
                Expression.Call(typeof(JsonDirectDeserialzerExpression<>).MakeGenericType(elementType).GetMethod("Deserialzer", new Type[] { typeof(JsonDirectReader), typeof(Dictionary<Type, IJsonCustomConverter>) }), item, jsonCustomConvertersParameter)));

            expres.Add(Expression.Loop(Expression.Block(loopexpres), loopLabel));

            //Convert list to array
            expres.Add(Expression.Assign(result, Expression.Call(list, typeof(List<>).MakeGenericType(elementType).GetMethod("ToArray"))));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);
            return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { list, enumerator, item, result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }
        /// <summary>
        /// Parse to list collection
        /// </summary>
        /// <returns>List collection</returns>
        private static Expression<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToList()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(JsonDirectReader), "jsonDirectReader");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");

            var curType = _type;
            //Get the generic type
            var genericType = curType.GetGenericArguments().FirstOrDefault();

            var result = Expression.Variable(curType, "result");
            var enumerator = Expression.Variable(typeof(IEnumerator<JsonDirectReader>), "enumerator");
            var item = Expression.Variable(typeof(JsonDirectReader), "item");
            var returnLabel = Expression.Label("returnLable");
            var loopLabel = Expression.Label();

            List<Expression> expres = new List<Expression>();
            List<Expression> loopexpres = new List<Expression>();

            if (genericType == null)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType),
                    typeof(Type), typeof(string) }), Expression.Constant(0), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType), Expression.Constant("泛型列化出错获取List泛型出错！"))));
                expres.Add(result);
                return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }

            //Determine if the JSON object is null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Null)),
                Expression.Block(new Expression[]{Expression.Call(jsonObjectParameter, "SkipCurrentObject", EmptyArray<Type>.Value), Expression.Return(returnLabel)})));

            //Determine if the JSON object is an array type
            expres.Add(
            Expression.IfThen(Expression.NotEqual(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Array)),
                Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                Expression.Property(jsonObjectParameter, "Position"), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType), Expression.Constant("Json对象不为Array类型不能解析成List类型")))));
            //Initialize List and enumerator
            expres.Add(Expression.Assign(result, Expression.New(curType)));
            expres.Add(Expression.Assign(enumerator, Expression.Call(
                Expression.Call(jsonObjectParameter, typeof(JsonDirectReader).GetMethod("ReadArray")),
                typeof(IEnumerable<JsonDirectReader>).GetMethod("GetEnumerator"))));
            //Loop to assign List values
            loopexpres.Add(Expression.IfThen(Expression.IsFalse(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"))), Expression.Break(loopLabel)));
            loopexpres.Add(Expression.Assign(item, Expression.Convert(Expression.Property(enumerator, "Current"), typeof(JsonDirectReader))));
            loopexpres.Add(Expression.Call(result, curType.GetMethod("Add", new Type[] { genericType }),
                Expression.Call(typeof(JsonDirectDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(JsonDirectReader), typeof(Dictionary<Type, IJsonCustomConverter>) }), item, jsonCustomConvertersParameter)));
            expres.Add(Expression.Loop(Expression.Block(loopexpres), loopLabel));
            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);
            return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { enumerator, item, result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);

        }

        /// <summary>
        /// Parse to object
        /// </summary>
        /// <returns>Object</returns>
        private static Expression<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToObject()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(JsonDirectReader), "jsonDirectReader");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");

            var curType = _type;

            List<Expression> expres = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();
            List<MemberBinding> bindings = new List<MemberBinding>();
            List<Expression> loopBody = new List<Expression>();

            var result = Expression.Variable(curType, "result");
            var enumerator = Expression.Variable(typeof(IEnumerator<KeyValuePair<JsonPropertyName, JsonDirectReader>>), "enumerator");
            var kvp = Expression.Variable(typeof(KeyValuePair<JsonPropertyName, JsonDirectReader>), "kvp");
            variables.Add(result);
            variables.Add(enumerator);
            variables.Add(kvp);
            

            var returnLabel = Expression.Label(typeof(T), "returnLabel");
            var loopBreak = Expression.Label("loopBreak");

            //Determine if the JSON object is null; value types cannot be null
            if (curType.IsValueType)
            {
               expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Null)),
               Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
               Expression.Property(jsonObjectParameter, "Position"), Expression.Constant(JsonType.Null), Expression.Constant(curType), Expression.Constant("反序列化失败，无法将json的null类型转换成值类型")))));
            }
            else
            {
                expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Null)),
                Expression.Block(new Expression[]{Expression.Call(jsonObjectParameter, "SkipCurrentObject", EmptyArray<Type>.Value), Expression.Return(returnLabel, Expression.Default(curType))})));
            }
            //Check if the type if assignable to JsonObject
            if (curType.IsAssignableFrom(typeof(JsonObject)))
            {
                expres.Add(Expression.Return(returnLabel, Expression.Call(jsonObjectParameter, "ReadAsJsonObject", EmptyArray<Type>.Value)));
                expres.Add(Expression.Label(returnLabel, result));
                return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            else if (typeof(JsonObject).IsAssignableFrom(curType))
            {
                var jsonObj = Expression.Variable(typeof(JsonObject), "jsonObj");
                variables.Add(jsonObj);
                expres.Add(Expression.Assign(jsonObj, Expression.Call(jsonObjectParameter, "ReadAsJsonObject", EmptyArray<Type>.Value)));
                if(typeof(JsonArray).IsAssignableFrom(curType))
                {
                    expres.Add(Expression.IfThenElse(Expression.Equal(Expression.Property(jsonObj, "Type"), Expression.Constant(JsonType.Array)),
                    Expression.Return(returnLabel, Expression.Convert(jsonObj, curType)),
                    Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                        Expression.Property(jsonObj, "Position"), Expression.Property(jsonObj, "Type"), Expression.Constant(curType), Expression.Constant("反序列化失败，无法进行显式的JsonObject类型转换"))
                    )));
                    expres.Add(Expression.Label(returnLabel, result));
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
                }
                else if(typeof(JsonBoolean).IsAssignableFrom(curType))
                {
                    expres.Add(Expression.IfThenElse(Expression.Equal(Expression.Property(jsonObj, "Type"), Expression.Constant(JsonType.Boolean)),
                    Expression.Return(returnLabel, Expression.Convert(jsonObj, curType)),
                    Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                        Expression.Property(jsonObj, "Position"), Expression.Property(jsonObj, "Type"), Expression.Constant(curType), Expression.Constant("反序列化失败，无法进行显式的JsonObject类型转换"))
                    )));
                    expres.Add(Expression.Label(returnLabel, result));
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
                }
                else if(typeof(JsonContent).IsAssignableFrom(curType))
                {
                    expres.Add(Expression.IfThenElse(Expression.Equal(Expression.Property(jsonObj, "Type"), Expression.Constant(JsonType.Content)),
                    Expression.Return(returnLabel, Expression.Convert(jsonObj, curType)),
                    Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                        Expression.Property(jsonObj, "Position"), Expression.Property(jsonObj, "Type"), Expression.Constant(curType), Expression.Constant("反序列化失败，无法进行显式的JsonObject类型转换"))
                    )));
                    expres.Add(Expression.Label(returnLabel, result));
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
                }
                else if(typeof(JsonNull).IsAssignableFrom(curType))
                {
                    expres.Add(Expression.IfThenElse(Expression.Equal(Expression.Property(jsonObj, "Type"), Expression.Constant(JsonType.Null)),
                    Expression.Return(returnLabel, Expression.Convert(jsonObj, curType)),
                    Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                        Expression.Property(jsonObj, "Position"), Expression.Property(jsonObj, "Type"), Expression.Constant(curType), Expression.Constant("反序列化失败，无法进行显式的JsonObject类型转换"))
                    )));
                    expres.Add(Expression.Label(returnLabel, result));
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
                }
                else if(typeof(JsonNumber).IsAssignableFrom(curType))
                {
                    expres.Add(Expression.IfThenElse(Expression.Equal(Expression.Property(jsonObj, "Type"), Expression.Constant(JsonType.Number)),
                    Expression.Return(returnLabel, Expression.Convert(jsonObj, curType)),
                    Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                        Expression.Property(jsonObj, "Position"), Expression.Property(jsonObj, "Type"), Expression.Constant(curType), Expression.Constant("反序列化失败，无法进行显式的JsonObject类型转换"))
                    )));
                    expres.Add(Expression.Label(returnLabel, result));
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
                }
                else if(typeof(JsonString).IsAssignableFrom(curType))
                {
                    expres.Add(Expression.IfThenElse(Expression.Equal(Expression.Property(jsonObj, "Type"), Expression.Constant(JsonType.String)),
                    Expression.Return(returnLabel, Expression.Convert(jsonObj, curType)),
                    Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                        Expression.Property(jsonObj, "Position"), Expression.Property(jsonObj, "Type"), Expression.Constant(curType), Expression.Constant("反序列化失败，无法进行显式的JsonObject类型转换"))
                    )));
                    expres.Add(Expression.Label(returnLabel, result));
                    return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
                }
            }
            //Check if the type is an interface or abstract type
            if (curType.IsInterface || curType.IsAbstract)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                Expression.Constant(0), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType), Expression.Constant("类型为接口或者是抽象接口，无法创建该类型对象"))));
                expres.Add(Expression.Label(returnLabel, Expression.Default(curType)));
                return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            //Check if the type has a default constructor
            if (curType.GetConstructor(EmptyArray<Type>.Value) == null)
            {
                expres.Add(Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                Expression.Constant(0), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType), Expression.Constant("反序列化类型没有默认的构造函数，无法创建该类型对象"))));
                expres.Add(Expression.Label(returnLabel, Expression.Default(curType)));
                return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
            }

            //Type check: must be Content
            expres.Add(Expression.IfThen(
                Expression.NotEqual(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Content)),
                Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType), typeof(Type), typeof(string) }),
                Expression.Property(jsonObjectParameter, "Position"), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType),
                Expression.Constant("Json对象不为Content类型不能解析成Object类型")))));

            //Get writable properties and create variables for each
            var groups = curType.GetProperties()
            .Where(n => n.CanWrite)
            //Ignore Attribute
            .Where(n => !Attribute.GetCustomAttributes(n).Any(x => x is JsonIgnoredAttribute a && (a.JsonIgnoredMethod & JsonMethods.Deserialize) == JsonMethods.Deserialize))
            .Select(n => new KeyValuePair<JsonPropertyName, PropertyInfo>(new JsonPropertyName(n.Name), n))
            .GroupBy(n=>n.Key.HashCode);
            var propVarMap = new Dictionary<System.Reflection.PropertyInfo, ParameterExpression>();
            List<SwitchCase> switchCases = new List<SwitchCase>();
            foreach(var item in groups)
            {
                
                Expression ifExp = null;
                foreach(var propertie in item)
                {
                    var propertyValue = Expression.Variable(propertie.Value.PropertyType, propertie.Value.Name + "propertyValue");
                    variables.Add(propertyValue);
                    propVarMap[propertie.Value] = propertyValue;
                    bindings.Add(Expression.Bind(propertie.Value, propertyValue));
                    var deserializerMethod = typeof(JsonDirectDeserialzerExpression<>).MakeGenericType(propertie.Value.PropertyType)
                    .GetMethod("Deserialzer", new Type[] { typeof(JsonDirectReader), typeof(Dictionary<Type, IJsonCustomConverter>) });
                    if(ifExp == null)
                    {
                        ifExp = Expression.IfThenElse(Expression.Equal(Expression.Constant(propertie.Key), Expression.Property(kvp, "Key")),
                            Expression.Assign(propertyValue, Expression.Call(deserializerMethod, Expression.Property(kvp, "Value"), jsonCustomConvertersParameter)),
                            Expression.Call(jsonObjectParameter, "SkipCurrentObject", EmptyArray<Type>.Value));
                    }
                    else
                    {
                        ifExp = Expression.IfThenElse(Expression.Equal(Expression.Constant(propertie.Key), Expression.Property(kvp, "Key")),
                            Expression.Assign(propertyValue, Expression.Call(deserializerMethod, Expression.Property(kvp, "Value"), jsonCustomConvertersParameter)),
                            ifExp);
                    }
                }
                switchCases.Add(Expression.SwitchCase(ifExp,Expression.Constant(item.Key)));
            }
            loopBody.Add(Expression.Switch(typeof(void), Expression.Call(Expression.Property(kvp, "Key"), "GetHashCode", EmptyArray<Type>.Value), Expression.Call(jsonObjectParameter, "SkipCurrentObject", EmptyArray<Type>.Value), null, switchCases.ToArray()));
            expres.Add(Expression.Assign(enumerator,
                Expression.Call(
                    Expression.Call(jsonObjectParameter, typeof(JsonDirectReader).GetMethod("ReadContent")),
                    typeof(IEnumerable<KeyValuePair<JsonPropertyName, JsonDirectReader>>).GetMethod("GetEnumerator"))));

            // Loop: while (enumerator.MoveNext()) { kvp = enumerator.Current; ... }
            loopBody.Insert(0, Expression.IfThen(
                Expression.IsFalse(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"))),
                Expression.Break(loopBreak)));
            loopBody.Insert(1, Expression.Assign(kvp, Expression.Property(enumerator, "Current")));

            expres.Add(Expression.Loop(Expression.Block(loopBody), loopBreak));

            // result = new T { Prop1 = propVar1, ... }
            expres.Add(Expression.Assign(result, Expression.MemberInit(Expression.New(curType), bindings)));
            expres.Add(Expression.Label(returnLabel, result));

            return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(variables, expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }

        private static Expression<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>> ConvertToNullable()
        {
            ParameterExpression jsonObjectParameter = Expression.Parameter(typeof(JsonDirectReader), "jsonDirectReader");
            ParameterExpression jsonCustomConvertersParameter = Expression.Parameter(typeof(Dictionary<Type, IJsonCustomConverter>), "jsonCustomConverters");
            var curType = _type;

            List<Expression> expres = new List<Expression>();

            var result = Expression.Variable(curType, "result");
            var returnLabel = Expression.Label("returnLable");

            //Get the generic type
            var genericType = curType.GetGenericArguments().FirstOrDefault();
            if (genericType == null)
            {
                return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Throw(Expression.New(typeof(JsonDirectDeserializationException).GetConstructor(new Type[] { typeof(int), typeof(JsonType),
                    typeof(Type), typeof(string) }), Expression.Constant(0), Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(curType), Expression.Constant("反序列化出错获取，获取Nullable泛型出错！"))), jsonObjectParameter, jsonCustomConvertersParameter);
            }
            //Determine if the JSON object is null
            expres.Add(Expression.IfThen(Expression.Equal(Expression.Property(jsonObjectParameter, "JsonType"), Expression.Constant(JsonType.Null)),
                Expression.Block(new Expression[]{Expression.Call(jsonObjectParameter, "SkipCurrentObject", EmptyArray<Type>.Value), Expression.Return(returnLabel)})));
            expres.Add(Expression.Assign(result, Expression.New(curType.GetConstructor(new Type[] { genericType }),
                Expression.Call(typeof(JsonDirectDeserialzerExpression<>).MakeGenericType(genericType).GetMethod("Deserialzer", new Type[] { typeof(JsonDirectReader), typeof(Dictionary<Type, IJsonCustomConverter>) }), jsonObjectParameter, jsonCustomConvertersParameter))));

            expres.Add(Expression.Label(returnLabel));
            expres.Add(result);

            return Expression.Lambda<Func<JsonDirectReader, Dictionary<Type, IJsonCustomConverter>, T>>(Expression.Block(new ParameterExpression[] { result }, expres), jsonObjectParameter, jsonCustomConvertersParameter);
        }
    }
}
