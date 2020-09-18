using LHZ.FastJson.Enum;
using LHZ.FastJson.Json.Format;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace LHZ.FastJson.Json
{
    /// <summary>
    /// Json序列化类
    /// </summary>
    public class JsonSerializer
    {
        private StringBuilder _jsonStrBuilder = new StringBuilder(128);
        private Stack<object> _objStack = new Stack<object>();
        private JsonFormatter _formater = null;

        private static readonly Type[] _objectType;
        private static readonly int _objectCount;

        private object _obj;

        static JsonSerializer()
        {
            _objectCount = System.Enum.GetValues(typeof(Enum.ObjectType)).Length;
            _objectType = new Type[_objectCount];

            _objectType[(int)ObjectType.Boolean] = typeof(Boolean);
            _objectType[(int)ObjectType.Byte] = typeof(Byte);
            _objectType[(int)ObjectType.Char] = typeof(Char);
            _objectType[(int)ObjectType.Int16] = typeof(Int16);
            _objectType[(int)ObjectType.UInt16] = typeof(UInt16);
            _objectType[(int)ObjectType.Int32] = typeof(Int32);
            _objectType[(int)ObjectType.UInt32] = typeof(UInt32);
            _objectType[(int)ObjectType.Int64] = typeof(Int64);
            _objectType[(int)ObjectType.UInt64] = typeof(UInt64);
            _objectType[(int)ObjectType.Float] = typeof(Single);
            _objectType[(int)ObjectType.Double] = typeof(Double);
            _objectType[(int)ObjectType.Decimal] = typeof(Decimal);
            _objectType[(int)ObjectType.DateTime] = typeof(DateTime);
            _objectType[(int)ObjectType.String] = typeof(String);
            _objectType[(int)ObjectType.Enum] = typeof(System.Enum);
            _objectType[(int)ObjectType.Dictionary] = typeof(IDictionary);
            _objectType[(int)ObjectType.List] = typeof(IList);
            _objectType[(int)ObjectType.Enumerable] = typeof(IEnumerable);
            _objectType[(int)ObjectType.Object] = typeof(Object);
            _objectType[(int)ObjectType.Array] = typeof(Array);

        }
        public JsonSerializer(object obj)
        {
            this._obj = obj;
            this._formater = new JsonFormatter();
        }
        public JsonSerializer(object obj, IJsonFormat[] formats)
        {
            this._obj = obj;
            this._formater = new JsonFormatter(formats);
        }

        /// <summary>
        /// 序列化方法
        /// </summary>
        /// <returns>Json字符串</returns>
        public string Serialize()
        {
            SwitchSerializationMethod(_obj);
            return _jsonStrBuilder.ToString();
        }

        /// <summary>
        /// 对序列化类型进行对应的序列化操作
        /// </summary>
        /// <param name="type">序列化对象类型</param>
        private void SwitchSerializationMethod(object obj)
        {
            if (obj == null)
            {
                _jsonStrBuilder.Append("null");
                return;
            }
            Type type = obj.GetType();

            //针对jsonobject类型的序列化
            IJsonObject jsonObject = obj as IJsonObject;
            if (jsonObject != null)
            {
                _jsonStrBuilder.Append(jsonObject.ToJsonString());
                return;
            }

            bool isValueType = type.IsValueType;
            if (!isValueType)
            {
                if (_objStack.Contains(obj))
                {
                    throw new Exception("循环引用");
                }
                _objStack.Push(obj);
            }
            ObjectType objectType = GetObjectType(type);
            switch (objectType)
            {
                case ObjectType.Boolean:  SerializeBoolean(obj); break ;
                case ObjectType.Int32: SerializeInt32(obj); break;
                case ObjectType.Int64: SerializeInt64( obj); break;
                case ObjectType.UInt32: SerializeUInt32(obj); break;
                case ObjectType.UInt64: SerializeUInt64(obj); break;
                case ObjectType.Int16: SerializeInt16(obj); break;
                case ObjectType.UInt16: SerializeUInt16(obj); break;
                case ObjectType.Byte: SerializeByte(obj); break;
                case ObjectType.Char: SerializeChar(obj); break;
                case ObjectType.Float: SerializeFloat( obj); break;
                case ObjectType.Double: SerializeDouble( obj); break;
                case ObjectType.Decimal: SerializeDecimal(obj); break;
                case ObjectType.DateTime: SerializeDateTime(obj); break;
                case ObjectType.Enum: SerializeEnum(obj); break;
                case ObjectType.String: SerializeString(obj); break;
                case ObjectType.Dictionary: SerializeDictionary(obj); break;
                case ObjectType.List: SerializeEnumerable(obj); break;
                case ObjectType.Enumerable: SerializeEnumerable(obj); break;
                case ObjectType.Object: SerializeObject(obj); break;
                case ObjectType.Array: SerializeEnumerable(obj); break;
                default: throw new Exception("未知转换类型");
            }
            if (!isValueType)
            {
                _objStack.Pop();
            }
            return;
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="objectType"></param>
        private void Serialize(ObjectType objectType, object obj)
        {
            
        }

        /// <summary>
        /// 获取对象的类型
        /// </summary>
        /// <param name="type">序列化对象类型</param>
        /// <returns>对象类型</returns>
        private ObjectType GetObjectType(Type type)
        {
            for (int i = 0; i < _objectCount; i++)
            {
                if (type == _objectType[i])
                    return (ObjectType)i;
            }

            if (type.IsEnum)
                return ObjectType.Enum;
            else if (_objectType[(int)ObjectType.Dictionary].IsAssignableFrom(type))
                return ObjectType.Dictionary;
            else if (_objectType[(int)ObjectType.Enumerable].IsAssignableFrom(type))
                return ObjectType.Enumerable;
            else
                return ObjectType.Object;
        }

        /// <summary>
        /// bool类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeBoolean(object obj)
        {
            if ((bool)obj)
                _jsonStrBuilder.Append("true");
            else
                _jsonStrBuilder.Append("false");
        }

        /// <summary>
        /// byte类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeByte(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// char类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeChar(object obj)
        {
            _jsonStrBuilder.Append("\"" + obj.ToString() + "\"");
        }

        /// <summary>
        /// int16类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeInt16(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// uint16类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeUInt16(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }


        /// <summary>
        /// int32类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeInt32(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// uint32类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeUInt32(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// int64类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeInt64(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// uint64类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeUInt64(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// float类型序列化
        /// </summary>
        /// <param name="obj"></param>
        private void SerializeFloat(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// double类型序列
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeDouble(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }
        private void SerializeDecimal(object obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }
        /// <summary>
        /// DateTime类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeDateTime(object obj)
        {
            _jsonStrBuilder.Append("\"" + _formater.DateTimeFormat((DateTime)obj) + "\"");
        }
        /// <summary>
        /// Enum类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeEnum(object obj)
        {
            _jsonStrBuilder.Append("\"" + obj.ToString() + "\"");
        }
        /// <summary>
        /// String类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeString(object obj)
        {
            string str = obj as string;
            if (str == null)
            {
                throw new Exception("当前对象不能转化成string类型");
            }
            _jsonStrBuilder.Append('"');
            foreach (char item in str)
            {
                if (item > '"')
                    _jsonStrBuilder.Append(item);
                else
                    _jsonStrBuilder.Append(CharParaphrase(item));
            }
            _jsonStrBuilder.Append('"');
        }
        /// <summary>
        /// Dictionary类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeDictionary(object obj)
        {
            IDictionary dictionary = obj as IDictionary;
            _jsonStrBuilder.Append("{");
            int i = 0;
            foreach (DictionaryEntry item in dictionary)
            {
                if (i == 0)
                    _jsonStrBuilder.Append("\"" + item.Key.ToString() + "\":");
                else
                    _jsonStrBuilder.Append(",\"" + item.Key.ToString() + "\":");
                SwitchSerializationMethod(item.Value);
                i++;
            }
            _jsonStrBuilder.Append("}");
        }
        /// <summary>
        /// Enumerable类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeEnumerable(object obj)
        {
            IEnumerable enumerable = obj as IEnumerable;
            _jsonStrBuilder.Append("[");
            int i = 0;
            foreach (var item in enumerable)
            {
                if (i != 0)
                    _jsonStrBuilder.Append(",");
                SwitchSerializationMethod(item);
                i++;
            }
            _jsonStrBuilder.Append("]");
        }
        /// <summary>
        /// object类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeObject(object obj)
        {
            _jsonStrBuilder.Append("{");
            List<PropertyInfo> propertyInfos = obj.GetType().GetProperties().Where(n => n.CanRead).ToList();

            for (int i = 0; i < propertyInfos.Count; i++)
            {
                if (i == 0)
                    _jsonStrBuilder.Append("\"" + propertyInfos[i].Name + "\":");
                else
                    _jsonStrBuilder.Append(",\"" + propertyInfos[i].Name + "\":");
#if NET35 || NET40
                SwitchSerializationMethod(propertyInfos[i].GetValue(obj, null));
#else
                 SwitchSerializationMethod(propertyInfos[i].GetValue(obj));
#endif
            }
            _jsonStrBuilder.Append("}");
        }

        /// <summary>
        /// 字符转义
        /// </summary>
        /// <param name="paraphrase">需要转义的字符</param>
        /// <returns>转义好的字符串</returns>
        private string CharParaphrase(char paraphrase)
        {
            if (paraphrase == '"')
                return "\\\"";
            else if (paraphrase == '\\')
                return "\\\\";
            else if (paraphrase == '\n')
                return "\\n";
            else if (paraphrase == '\t')
                return "\\t";
            else if (paraphrase == '\a')
                return "\\a";
            else if (paraphrase == '\b')
                return "\\b";
            else if (paraphrase == '\f')
                return "\\f";
            else if (paraphrase == '\r')
                return "\\r";
            else if (paraphrase == '\v')
                return "\\v";
            return paraphrase.ToString();
        }
    }
}
