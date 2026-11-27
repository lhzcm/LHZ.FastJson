using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.Utils;
using LHZ.FastJson.Utils;

namespace LHZ.FastJson.Json
{
    /// <summary>
    /// JSON serialization class
    /// </summary>
    public class JsonSerializer
    {
        private StringBuilder _jsonStrBuilder = new StringBuilder(128);
        private Stack<object> _objStack = new Stack<object>();
        //Property names are baked into the compiled expressions,
        //so the delegates are cached per naming policy: indexed by (int)CaseStyle.
        private static readonly ConcurrentDictionary<Type, Action<JsonSerializer, object>>[] _serializationActions = CreateSerializationActionCaches();

        //The dictionary serializers are looked up once: the generic one is closed over the
        //dictionary's key/value types when the property expression is built.
        public static readonly MethodInfo DictionarySerializer = typeof(JsonSerializer).GetMethod(
            "SerializeDictionary", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(IDictionary) }, null);

        public static readonly MethodInfo GenericDictionarySerializer = typeof(JsonSerializer)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(n => n.Name == "SerializeDictionary" && n.IsGenericMethod);

        private static ConcurrentDictionary<Type, Action<JsonSerializer, object>>[] CreateSerializationActionCaches()
        {
            var caches = new ConcurrentDictionary<Type, Action<JsonSerializer, object>>[JsonConvertOptions.CaseStyleCount];
            for (int i = 0; i < caches.Length; i++)
            {
                caches[i] = new ConcurrentDictionary<Type, Action<JsonSerializer, object>>();
            }
            return caches;
        }
        private void ReferenceStackPush(object obj)
        {
            foreach (var item in _objStack)
            {
                if (obj == item)
                    throw JsonExceptions.CircularReference(obj.GetType());
            }
            _objStack.Push(obj);
        }
        private void ReferenceStackPop()
        {
            _objStack.Pop();
        }
        private object _obj;

        private readonly JsonConvertOptions _options;
        public JsonConvertOptions JsonConvertOptions => _options;

        //Scratch buffer for the allocation-free ISO 8601 DateTime writer (created on first use)
        private char[] _dateTimeBuffer;

        /// <summary>
        /// Initialize the serializer with an object
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        public JsonSerializer(object obj)
            : this(obj, null)
        {
        }
        /// <summary>
        /// Initialize the serializer with an object and options
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="options">Serialization options (null uses the defaults)</param>
        public JsonSerializer(object obj, JsonConvertOptions options)
        {
            this._obj = obj;
            this._options = JsonConvertOptions.Resolve(options);
        }

        /// <summary>
        /// Serialization method
        /// </summary>
        /// <returns>JSON string</returns>
        public string Serialize()
        {
            if (_obj == null)
            {
                return "null";
            }
            var del = GetSerializationAction(_obj.GetType());
            del(this, _obj);
            return _jsonStrBuilder.ToString();
        }

        /// <summary>
        /// Get the serialization method for the given object type
        /// </summary>
        /// <param name="objType">Object type</param>
        /// <returns>Serialization delegate method</returns>
        private Action<JsonSerializer, object> GetSerializationAction(Type objType)
        {
            var cache = _serializationActions[(int)_options.CaseStyle];
            Action<JsonSerializer, object> action;
            if (cache.TryGetValue(objType, out action))
            {
                return action;
            }
            var exp = CreateSerializationExpression(objType);
            action = CreateSerializationExpression(objType).Compile();
            cache[objType] = action;
            return action;
        }
        /// <summary>
        /// Create a serialization expression
        /// </summary>
        /// <param name="objType">Object type</param>
        /// <returns>Serialization expression</returns>
        private Expression<Action<JsonSerializer, object>> CreateSerializationExpression(Type objType)
        {
            //Current object
            var serializerObjParameter = Expression.Parameter(typeof(JsonSerializer), "serializerObjParameter");
            //Serialization object parameter
            var objParameter = Expression.Parameter(typeof(object));
            //Convert to target object
            var obj = Expression.Convert(objParameter, objType);
            //Expression
            Expression exp = Expression.Call(typeof(JsonSerializerExpression<>).MakeGenericType(objType).GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static), serializerObjParameter, obj);
            return Expression.Lambda<Action<JsonSerializer, object>>(exp, serializerObjParameter, objParameter);
        }
        /// <summary>
        /// bool type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeBoolean(bool obj)
        {
            if ((bool)obj)
                _jsonStrBuilder.Append("true");
            else
                _jsonStrBuilder.Append("false");
        }

        /// <summary>
        /// byte type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeByte(byte obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// char type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeChar(char obj)
        {
            SerializeString(obj.ToString());
        }

        /// <summary>
        /// int16 type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeInt16(short obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// uint16 type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeUInt16(ushort obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// int32 type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeInt32(int obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// uint32 type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeUInt32(uint obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// int64 type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeInt64(long obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// uint64 type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeUInt64(ulong obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// float type serialization
        /// </summary>
        /// <param name="obj"></param>
        private void SerializeFloat(float obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// double type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeDouble(double obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }
        private void SerializeDecimal(decimal obj)
        {
            _jsonStrBuilder.Append(obj.ToString(CultureInfo.InvariantCulture));
        }
        /// <summary>
        /// DateTime type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeDateTime(DateTime obj)
        {
            string format = _options.DateTimeFormat;
            if (format != null)
            {
                SerializeString(obj.ToString(format, CultureInfo.InvariantCulture));
                return;
            }
            if (_dateTimeBuffer == null)
            {
                _dateTimeBuffer = new char[DateTimeFormatter.MaxIsoLength + 2];
            }
            DateTimeFormatter.WriteIsoQuoted(_jsonStrBuilder, obj, _dateTimeBuffer);
        }
        /// <summary>
        /// JSON node serialization: the node already holds its JSON text, so it is appended as-is
        /// </summary>
        /// <param name="obj">JSON node to serialize</param>
        private void SerializeJsonObject(IJsonObject obj)
        {
            if (obj == null)
            {
                _jsonStrBuilder.Append("null");
                return;
            }
            obj.ToStringBuilder(_jsonStrBuilder);
        }
        /// <summary>
        /// Enum type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeEnum(System.Enum obj)
        {
            _jsonStrBuilder.Append(obj.ToString("D"));
        }
        /// <summary>
        /// Guid type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeGuid(Guid obj)
        {
            SerializeString(obj.ToString());
        }
        /// <summary>
        /// String type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeString(string obj)
        {
            if (obj == null)
            {
                _jsonStrBuilder.Append("null");
                return;
            }
            _jsonStrBuilder.Append('"');
            for (int i = 0; i < obj.Length; i++)
            {
                char current = obj[i];
                if (current < 0x20 || current == '"' || current == '\\')
                {
                    _jsonStrBuilder.Append(CharParaphrase(current));
                }
                else
                {
                    _jsonStrBuilder.Append(current);
                }
            }
            _jsonStrBuilder.Append('"');
        }

        private void SerializePropertyName(string name)
        {
            SerializeString(name);
            _jsonStrBuilder.Append(':');
        }

        private void SerializeAny(object obj)
        {
            if (obj == null)
            {
                _jsonStrBuilder.Append("null");
                return;
            }
            var type = obj.GetType();
            var action = GetSerializationAction(obj.GetType());
            action(this, obj);
        }

        /// <summary>
        /// Dictionary type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeDictionary(IDictionary obj)
        {
            if (obj == null)
            {
                _jsonStrBuilder.Append("null");
                return;
            }
            ReferenceStackPush(obj);
            _jsonStrBuilder.Append('{');
            int i = 0;
            foreach (DictionaryEntry item in obj)
            {
                if (i == 0)
                    SerializePropertyName(item.Key.ToString());
                else
                {
                    _jsonStrBuilder.Append(',');
                    SerializePropertyName(item.Key.ToString());
                }
                SerializeAny(item.Value);
                i++;
            }
            _jsonStrBuilder.Append('}');
            ReferenceStackPop();
        }
        /// <summary>
        /// Dictionary type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> obj)
        {
            if (obj == null)
            {
                _jsonStrBuilder.Append("null");
                return;
            }
            ReferenceStackPush(obj);
            _jsonStrBuilder.Append('{');
            int i = 0;
            foreach (var item in obj)
            {
                if (i == 0)
                    SerializePropertyName(item.Key.ToString());
                else
                {
                    _jsonStrBuilder.Append(',');
                    SerializePropertyName(item.Key.ToString());
                }
                SerializeAny(item.Value);
                i++;
            }
            _jsonStrBuilder.Append('}');
            ReferenceStackPop();
        }
        /// <summary>
        /// Enumerable type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeEnumerable(IEnumerable obj)
        {
            if (obj == null)
            {
                _jsonStrBuilder.Append("null");
                return;
            }
            ReferenceStackPush(obj);
            _jsonStrBuilder.Append('[');
            int i = 0;
            foreach (var item in obj)
            {
                if (i != 0)
                    _jsonStrBuilder.Append(',');
                SerializeAny(item);
                i++;
            }
            _jsonStrBuilder.Append(']');
            ReferenceStackPop();
        }
        
        private void AppendCharToStringBuilder(char cvalue)
        {
            _jsonStrBuilder.Append(cvalue);
        }
        private void AppendStringToStringBuilder(string str)
        {
            _jsonStrBuilder.Append(str);
        }

        /// <summary>
        /// Character escaping
        /// </summary>
        /// <param name="paraphrase">Character to escape</param>
        /// <returns>Escaped string</returns>
        private static string CharParaphrase(char paraphrase)
        {
            switch (paraphrase)
            {
                case '"': return "\\\"";
                case '\\': return "\\\\";
                case '\n': return "\\n";
                case '\t': return "\\t";
                case '\b': return "\\b";
                case '\f': return "\\f";
                case '\r': return "\\r";
                default : return "\\u" + ((int)paraphrase).ToString("x4");
            }
        }
    }
}
