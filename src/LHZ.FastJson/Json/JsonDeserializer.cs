using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace LHZ.FastJson.Json
{ 
    /// <summary>
    /// 反序列化类
    /// </summary>
    /// <typeparam name="T">需要进行反序列化的类</typeparam>
    public class JsonDeserializer<T>
    {
        private static readonly Dictionary<Type, List<PropertyInfo>> _proPertyInfos = new Dictionary<Type, List<PropertyInfo>>(4096);

        private JsonObject _obj;
        public JsonDeserializer(JsonObject obj)
        {
            this._obj = obj;
        }
        public JsonDeserializer(JsonReader reader)
        {
            this._obj = reader.JsonRead();
        }
        public JsonDeserializer(string jsonString)
        {
            JsonReader reader = new JsonReader(jsonString);
            this._obj = reader.JsonRead();
        }

        /// <summary>
        /// 进行反序列化
        /// </summary>
        /// <returns>返回反序列化类型对象</returns>
        public T Deserialize()
        {
            return (T)SwitchDeserializationMethod(typeof(T), _obj);
        }

        /// <summary>
        /// 对反序列化类型进行对应的反序列化操作
        /// </summary>
        /// <param name="type">反序列化对象类型</param>
        /// <param name="jsonObject">Json对象类型</param>
        /// <param name="propertyInfos">反序列属性列表</param>
        /// <returns></returns>
        private object SwitchDeserializationMethod(Type type, JsonObject jsonObject, List<PropertyInfo> propertyInfos = null)
        {
            if (jsonObject.Type == JsonType.Null)
            {
                if (type.IsValueType && !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    throw new JsonDeserializationException(jsonObject, type, "null类型不能解析成非空的值类型");
                }
                return null;
            }
            ObjectType objectType = GetObjectType(type);
            switch (objectType)
            {
                case ObjectType.Boolean: return ConvertToBoolean(jsonObject);
                case ObjectType.Int: return ConvertToInt(jsonObject);
                case ObjectType.Long: return ConvertToLong(jsonObject);
                case ObjectType.Float: return ConvertToFloat(jsonObject);
                case ObjectType.Double: return ConvertToDouble(jsonObject);
                case ObjectType.Decimal: return ConvertToDecimal(jsonObject);
                case ObjectType.DateTime: return ConvertToDateTime(jsonObject);
                case ObjectType.Enum: return ConvertToEnum(type, jsonObject);
                case ObjectType.String: return ConvertToString(jsonObject);
                case ObjectType.Dictionary: return ConvertToDictionary(type, jsonObject);
                case ObjectType.Array: return ConvertToArray(type, jsonObject);
                case ObjectType.List: return ConvertToList(type, jsonObject);
                case ObjectType.Enumerable: throw new JsonDeserializationException(jsonObject, type, "当前Enumerable类型只能解析IList和数组类型");
                case ObjectType.Object: return ConvertToObject(type, propertyInfos??GetPropertyInfos(type), jsonObject);
                default: throw new Exception("未知转换类型");
            }
        }

        /// <summary>
        /// 获取对象的类型
        /// </summary>
        /// <param name="type">反序列化对象类型</param>
        /// <returns></returns>
        private ObjectType GetObjectType(Type type)
        {
            if (type == typeof(bool) || type == typeof(bool?))
                return ObjectType.Boolean;
            if (type == typeof(int) || type == typeof(int?))
                return ObjectType.Int;
            else if (type == typeof(long) || type == typeof(long?))
                return ObjectType.Long;
            else if (type == typeof(float) || type == typeof(float?))
                return ObjectType.Float;
            else if (type == typeof(double) || type == typeof(double?))
                return ObjectType.Double;
            else if (type == typeof(decimal) || type == typeof(decimal?))
                return ObjectType.Decimal;
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
                return ObjectType.DateTime;
            else if (type.IsEnum)
                return ObjectType.Enum;
            else if (type == typeof(string))
                return ObjectType.String;
            else if (typeof(IDictionary).IsAssignableFrom(type))
                return ObjectType.Dictionary;
            else if (type.IsArray)
                return ObjectType.Array;
            else if (typeof(IList).IsAssignableFrom(type))
                return ObjectType.List;
            else if (typeof(IEnumerable).IsAssignableFrom(type))
                return ObjectType.Enumerable;
            else
                return ObjectType.Object;

        }

        /// <summary>
        /// 解析成bool类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <returns></returns>
        private object ConvertToBoolean(JsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Boolean)
            {
                throw new JsonDeserializationException(jsonObject, typeof(bool), "Json对象不为Boolean类型不能解析成Boolean类型");
            }
            return Boolean.Parse((string)jsonObject.Value);
        }
        /// <summary>
        /// 解析成int类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <returns></returns>
        private object ConvertToInt(JsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(int), "Json对象不为Number类型不能解析成Int类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(int), "Json对象Number类型不为Long不能解析成Int类型");
            }
            return int.Parse((string)jsonObject.Value);
        }
        /// <summary>
        /// 解析成long类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <returns></returns>
        private long ConvertToLong(JsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Number)
            {
                throw new JsonDeserializationException(jsonObject, typeof(long), "Json对象不为Number类型不能解析成Long类型");
            }
            if (((JsonNumber)jsonObject).NumberType != NumberType.Long)
            {
                throw new JsonDeserializationException(jsonObject, typeof(long), "Json对象Number类型不为Long不能解析成Long类型");
            }
            return long.Parse((string)jsonObject.Value);
        }
        /// <summary>
        /// 解析成单精度浮点类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <returns></returns>
        private object ConvertToFloat(JsonObject jsonObject)
        {
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
        /// <returns></returns>
        private object ConvertToDouble(JsonObject jsonObject)
        {
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
        /// <returns></returns>
        private object ConvertToDecimal(JsonObject jsonObject)
        {
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
        /// <returns></returns>
        private object ConvertToDateTime(JsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.String)
            {
                throw new JsonDeserializationException(jsonObject, typeof(DateTime), "Json对象不为String类型不能解析成DateTime类型");
            }
            return DateTime.Parse((string)jsonObject.Value);
        }

        /// <summary>
        /// 解析成枚举
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsonObject">Json对象</param>
        /// <returns></returns>
        private object ConvertToEnum(Type type, JsonObject jsonObject)
        {
            if (jsonObject.Type == JsonType.String)
            {
                EnumConverter converter = new EnumConverter(type);
                return converter.ConvertFromString((string)jsonObject.Value);
            }
            else if (jsonObject.Type == JsonType.Number && ((JsonNumber)jsonObject).NumberType == NumberType.Long)
            {
                int value = int.Parse((string)jsonObject.Value);
                if(!System.Enum.IsDefined(type, value))
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
        /// 解析成字符类型
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <returns></returns>
        private object ConvertToString(JsonObject jsonObject)
        {
            if (jsonObject.Type == JsonType.String)
            {
                return (string)jsonObject.Value;
            }
            throw new JsonDeserializationException(jsonObject, typeof(string), "Json对象不为String类型不能解析成String类型");
        }

        /// <summary>
        /// 解析成字典类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="jsonObject">Json对象</param>
        /// <returns></returns>
        private object ConvertToDictionary(Type type, JsonObject jsonObject)
        {
            JsonContent jsonContent = jsonObject as JsonContent;
            if (jsonContent == null)
            {
                throw new JsonDeserializationException(jsonObject, type, "当前字典类型不能够被转换");
            }
            Type[] genericTypes;
#if NET35 || NET40

            genericTypes = type.GetGenericArguments();
#else
            genericTypes = type.GenericTypeArguments;
#endif
            if (genericTypes.Length != 2 && typeof(string).IsAssignableFrom(genericTypes[0]))
            {
                throw new JsonDeserializationException(jsonObject, type, "当前字典类型不能够被转换");
            }
            IDictionary obj = Activator.CreateInstance(type) as IDictionary;
            if (obj is null)
            {
                throw new JsonDeserializationException(jsonObject, type, "当前字典类型默认实例化出错");
            }
            
            foreach (var item in jsonContent)
            {
                obj.Add(item.Key, SwitchDeserializationMethod(genericTypes[1], item.Value));
            }
            return obj;
        }

        /// <summary>
        /// 解析成数组
        /// </summary>
        /// <param name="type">数组对象类型</param>
        /// <param name="jsonObject">Json类型</param>
        /// <returns></returns>
        private object ConvertToArray(Type type, JsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Array)
            {
                throw new JsonDeserializationException(jsonObject, type, "Json对象不为Array类型不能解析成Array类型");
            }
            Type elementType = type.GetElementType();
            type.MakeArrayType();
            Array obj = Activator.CreateInstance(type, ((List<JsonObject>)jsonObject.Value).Count) as Array;
            if (obj == null)
            {
                throw new JsonDeserializationException(jsonObject, type, "初始化数组失败");
            }

            List<PropertyInfo> propertyInfos = GetPropertyInfos(elementType);

            List<JsonObject> list = (List<JsonObject>)jsonObject.Value;
            for (int i = 0; i < list.Count; i++)
            {
                obj.SetValue(SwitchDeserializationMethod(elementType, list[i], propertyInfos), i);
            }
            return obj;
        }

        /// <summary>
        /// 解析成列表集合
        /// </summary>
        /// <param name="type">列表集合对象类型</param>
        /// <param name="jsonObject">Json类型</param>
        /// <returns></returns>
        private object ConvertToList(Type type, JsonObject jsonObject)
        {
            JsonArray jsonArray = jsonObject as JsonArray;
            if (jsonArray == null)
            {
                throw new JsonDeserializationException(jsonObject, type, "Json对象不为Array类型不能解析成List类型");
            }
            Type genericType;
#if NET35 || NET40

            genericType = type.GetGenericArguments()[0];
#else
            genericType = type.GenericTypeArguments[0];
#endif
            IList obj = Activator.CreateInstance(type) as IList;
            if (obj == null)
            {
                throw new JsonDeserializationException(jsonObject, type, "List默认实例初始化出错");
            }

            List<PropertyInfo> propertyInfos = GetPropertyInfos(genericType);

            for (int i = 0; i < ((List<JsonObject>)jsonObject.Value).Count; i++)
            {
                obj.Add(SwitchDeserializationMethod(genericType, ((List<JsonObject>)jsonObject.Value)[i], propertyInfos));
            }
            return obj;
        }

        /// <summary>
        /// 解析成对象
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="properties">对象属性列表</param>
        /// <param name="jsonObject">Json对象</param>
        /// <returns></returns>
        public object ConvertToObject(Type type, List<PropertyInfo> properties, JsonObject jsonObject)
        {
            if (jsonObject.Type != JsonType.Content)
            {
                if (type.IsAssignableFrom(jsonObject.Value.GetType()))
                {
                    return jsonObject.Value;
                }
                throw new JsonDeserializationException(jsonObject, type, "Json对象不能把" + jsonObject.Type.ToString() + "类型不能解析成object类型");
            }
            if (type.IsAssignableFrom(jsonObject.GetType()))
            {
                return jsonObject;
            }
            object obj = System.Activator.CreateInstance(type);
            Dictionary<string, JsonObject> dictionary = (Dictionary<string, JsonObject>)jsonObject.Value;
            foreach (var item in properties)
            {
                JsonObject jsonitem = null;
                dictionary.TryGetValue(item.Name, out jsonitem);
                if (jsonitem == null)
                {
                    continue;
                }
                object objItem = SwitchDeserializationMethod(item.PropertyType, jsonitem);
#if NET35 || NET40
                item.SetValue(obj, objItem, null);
#else
                item.SetValue(obj, objItem);
#endif
            }
            return obj;
        }

        /// <summary>
        /// 从缓存中读取类型的可写属性列表
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>可写属性列表</returns>
        private List<PropertyInfo> GetPropertyInfos(Type type)
        {
            List<PropertyInfo> propertyInfos = null;
            _proPertyInfos.TryGetValue(type, out propertyInfos);
            if (propertyInfos == null)
            {
                propertyInfos = type.GetProperties().Where(n => n.CanWrite).ToList();
                _proPertyInfos.Add(type, propertyInfos);
            }
            return propertyInfos;
        }
    }
}
