using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Enum.CustomConverter;
using LHZ.FastJson.Interface;
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
        private static readonly Dictionary<Type, ObjectType> _objectTypes = JsonObjectType.GetObjectTypes();
        //Property names are baked into the compiled expressions,
        //so the delegates are cached per naming policy: indexed by (int)CaseStyle.
        private static readonly ConcurrentDictionary<Type, Action<JsonSerializer, object>>[] _serializationActions = CreateSerializationActionCaches();

        //The dictionary serializers are looked up once: the generic one is closed over the
        //dictionary's key/value types when the property expression is built.
        private static readonly MethodInfo _dictionarySerializer = typeof(JsonSerializer).GetMethod(
            "SerializeDictionary", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(IDictionary) }, null);

        private static readonly MethodInfo _genericDictionarySerializer = typeof(JsonSerializer)
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

        private object _obj;

        private readonly JsonConvertOptions _options;

        //Scratch buffer for the allocation-free ISO 8601 DateTime writer (created on first use)
        private char[] _dateTimeBuffer;

        private Dictionary<Type, IJsonCustomConverter> _customConverters;
        private static readonly FieldInfo _customConvertersField = typeof(JsonSerializer).GetField(nameof(_customConverters), BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Initialize the serializer with an object
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        public JsonSerializer(object obj)
            : this(obj, (JsonConvertOptions)null, null)
        {
        }
        /// <summary>
        /// Initialize with custom converters
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="jsonCustomConverters">Custom converters</param>
        public JsonSerializer(object obj, params IJsonCustomConverter[] jsonCustomConverters)
            : this(obj, (JsonConvertOptions)null, jsonCustomConverters)
        {
        }
        /// <summary>
        /// Initialize with options and custom converters
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="options">Serialization options (null uses the defaults)</param>
        /// <param name="jsonCustomConverters">Custom converters</param>
        public JsonSerializer(object obj, JsonConvertOptions options, params IJsonCustomConverter[] jsonCustomConverters)
        {
            this._obj = obj;
            this._options = JsonConvertOptions.Resolve(options);
            if(jsonCustomConverters != null && jsonCustomConverters.Length > 0)
            {
                _customConverters = new Dictionary<Type, IJsonCustomConverter>(jsonCustomConverters.Length + 4);
                foreach(var item in jsonCustomConverters)
                {
                    if ((item.CustomItem & Enum.CustomConverter.JsonCustomConvertItem.CustomSerialize) != Enum.CustomConverter.JsonCustomConvertItem.CustomSerialize)
                    {
                        continue;
                    }
                    if(_customConverters.ContainsKey(item.ConvertType))
                    {
                        _customConverters[item.ConvertType] = item;
                    }
                    else
                    {
                        _customConverters.Add(item.ConvertType, item);
                    }
                }
                //Keep the fast built-in path when no converter applies to serialization
                if (_customConverters.Count == 0)
                {
                    _customConverters = null;
                }
            }
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
            action = CreateSerializationExpression(objType).Compile();
            cache[objType] = action;
            return action;
        }

        /// <summary>
        /// Create a serialization expression for the given object type
        /// </summary>
        /// <param name="objType">Object type</param>
        /// <returns>Serialization expression</returns>
        private Expression<Action<JsonSerializer, object>> CreateObjectSerializationExpression(Type objType)
        {
            //Current object
            var thisObjParameter = Expression.Parameter(typeof(JsonSerializer), "thisObjParameter");
            //Serialization object parameter
            var objParameter = Expression.Parameter(typeof(object), "objParameter");
            //jsonStrBuilder object parameter
            var jsonStrBuilder = Expression.Parameter(typeof(StringBuilder), "strBuilder");
            //Circular reference stack object parameter
            var stack = Expression.Parameter(typeof(Stack<object>), "stack");
            //Convert to target object
            var obj = Expression.Parameter(objType, "obj");
            //Expression list
            List<Expression> expList = new List<Expression>();
            //End label target
            LabelTarget endLabelTarget = Expression.Label("endLabelTarget");
            //Get properties
            var properties = objType.GetProperties().Where(n => {
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

            //Variable assignment
            expList.Add(Expression.Assign(obj, Expression.Convert(objParameter, objType)));
            expList.Add(Expression.Assign(jsonStrBuilder, Expression.Call(thisObjParameter, ((Func<StringBuilder>) GetStringBuilder).Method)));

            ////Custom JSON format (add to the parent level implementation)
            //List<Expression> customConvertersBody = new List<Expression>();
            //var customConvertersField = typeof(JsonSerializer).GetField("_customConverters", BindingFlags.NonPublic | BindingFlags.Instance);
            //var customConvertersFieldExp = Expression.Field(thisObjParameter, customConvertersField);

            //expList.Add(Expression.IfThen(Expression.NotEqual(customConvertersFieldExp, Expression.Constant(null)), Expression.Block(customConvertersBody)));

            //Check for duplicate property names
            if (JsonUtility.HasSameNameProperty(properties, _options.CaseStyle, out string samePropertyName))
            {
                throw new Exception($"序列化失败，类型{objType.FullName}不同的属性存在相同的属性名\"{samePropertyName}\"");
            }

            //Add left brace for JSON object type
            expList.Add(Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(char) }), Expression.Constant('{')));

            foreach (var item in properties)
            {
                var propertyType = item.PropertyType;
                ObjectType objectType = GetObjectType(propertyType);
                Expression exp = null;
                //Whether the property is serialized by a built-in method directly (such calls skip custom converters)
                bool builtInCall = true;
                Func<Type, Action<JsonSerializer, object>> sact = GetSerializationAction;

                #region Custom property name handling
                string jsonPropertyName = JsonUtility.GetPropertyName(item, _options.CaseStyle);
                #endregion
                expList.Add(Expression.Call(thisObjParameter, ((Action<string>)SerializePropertyName).Method, Expression.Constant(jsonPropertyName)));
                switch (objectType)
                {
                    case ObjectType.Boolean: exp = Expression.Call(thisObjParameter, ((Action<bool>)SerializeBoolean).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Int32: exp = Expression.Call(thisObjParameter, ((Action<int>)SerializeInt32).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Int64: exp = Expression.Call(thisObjParameter, ((Action<long>)SerializeInt64).Method, Expression.Property(obj, item)); break;
                    case ObjectType.UInt32: exp = Expression.Call(thisObjParameter, ((Action<uint>)SerializeUInt32).Method, Expression.Property(obj, item)); break;
                    case ObjectType.UInt64: exp = Expression.Call(thisObjParameter, ((Action<ulong>)SerializeUInt64).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Int16: exp = Expression.Call(thisObjParameter, ((Action<short>)SerializeInt16).Method, Expression.Property(obj, item)); break;
                    case ObjectType.UInt16: exp = Expression.Call(thisObjParameter, ((Action<ushort>)SerializeUInt16).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Byte: exp = Expression.Call(thisObjParameter, ((Action<byte>)SerializeByte).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Char: exp = Expression.Call(thisObjParameter, ((Action<char>)SerializeChar).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Float: exp = Expression.Call(thisObjParameter, ((Action<float>)SerializeFloat).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Double: exp = Expression.Call(thisObjParameter, ((Action<double>)SerializeDouble).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Decimal: exp = Expression.Call(thisObjParameter, ((Action<decimal>)SerializeDecimal).Method, Expression.Property(obj, item)); break;
                    case ObjectType.DateTime: exp = Expression.Call(thisObjParameter, ((Action<DateTime>)SerializeDateTime).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Enum: exp = Expression.Call(thisObjParameter, ((Action<System.Enum>)SerializeEnum).Method, Expression.Convert(Expression.Property(obj, item), typeof(System.Enum))); break;
                    case ObjectType.Guid: exp = Expression.Call(thisObjParameter, ((Action<Guid>)SerializeGuid).Method, Expression.Property(obj, item)); break;
                    case ObjectType.String: exp = Expression.Call(thisObjParameter, ((Action<string>)SerializeString).Method, Expression.Property(obj, item)); break;
                    case ObjectType.Nullable: builtInCall = false; exp = Expression.Call(thisObjParameter, ((Action<object>)SerializeAny).Method, Expression.Convert(Expression.Property(obj, item), typeof(object))); break;
                    case ObjectType.Dictionary:
                        {
                            exp = CreateDictionarySerializationExpression(thisObjParameter, propertyType, Expression.Property(obj, item));
                            break;
                        }
                    case ObjectType.Enumerable:
                        {
                            exp = Expression.Call(thisObjParameter, ((Action<IEnumerable>)SerializeEnumerable).Method, Expression.Property(obj, item));
                            break;
                        }
                    default:
                        {
                            builtInCall = false;
                            //Box value types before passing them to the object-typed serializer delegate.
                            var boxedProperty = Expression.Convert(Expression.Property(obj, item), typeof(object));
                            exp = Expression.Invoke(
                                Expression.Call(thisObjParameter, sact.Method, Expression.Call(boxedProperty, typeof(object).GetMethod("GetType"))),
                                thisObjParameter,
                                boxedProperty);
                            break;
                        }
                }
                //Built-in calls skip custom converters, so when custom converters are supplied
                //(checked at run time: the compiled delegate is shared by all serializer instances)
                //route the value through SerializeAny, which looks the converter up by type.
                if (builtInCall)
                {
                    exp = Expression.IfThenElse(
                        Expression.Equal(Expression.Field(thisObjParameter, _customConvertersField), Expression.Constant(null, typeof(Dictionary<Type, IJsonCustomConverter>))),
                        exp,
                        Expression.Call(thisObjParameter, ((Action<object>)SerializeAny).Method, Expression.Convert(Expression.Property(obj, item), typeof(object))));
                }
                ////Get property type serialization (this method does not determine the actual type of each object)
                // var expressionMethods = this.GetType().GetMethods((BindingFlags) 0x7FFFFFFF).First(n => n.Name == "CreateSerializationExpression" && n.IsGenericMethod).MakeGenericMethod(item.PropertyType);
                // var serializationExpression = expressionMethods.Invoke(this, null) as Expression;
                // exp = Expression.Invoke(serializationExpression, new Expression[]{thisObjParameter, Expression.Property(obj, item)});

                //Check if reference type is null
                if (!propertyType.IsValueType && (objectType == ObjectType.String || objectType == ObjectType.Dictionary || objectType == ObjectType.Enumerable || objectType == ObjectType.Object))
                {
                    //Perform null check
                    var propertyValue = Expression.Property(obj, item);
                    var propertyNull = Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant("null"));
                    exp = Expression.IfThenElse(Expression.Equal(propertyValue, Expression.Constant(null, propertyType)), propertyNull, exp);
                }
                expList.Add(exp);
                //Check if it is the last field; if not, add ","
                if (properties.IndexOf(item) < properties.Count - 1)
                {
                    expList.Add(Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant(",")));
                }
            }
            //Circular reference check
            if (!objType.IsValueType)
            {
                
                var isCircularReference = Expression.Call(thisObjParameter, ((Func<object, bool>)IsCircularReference).Method, objParameter);
                var judgeCircularReference = Expression.IfThen(Expression.IsTrue(isCircularReference), Expression.Throw(Expression.Constant(new Exception("循环引用"))));

                expList.Insert(0, Expression.Assign(stack, Expression.Call(thisObjParameter, ((Func<Stack<object>>)GetStack).Method)));
                expList.Insert(1, judgeCircularReference);
                expList.Insert(2, Expression.Call(stack, typeof(Stack<object>).GetMethod("Push", new Type[]{ typeof(object) }), objParameter));
                expList.Add(Expression.Call(stack, typeof(Stack<object>).GetMethod("Pop")));
            }
            //Add closing label at the end
            expList.Add(Expression.Label(endLabelTarget));

            expList.Add(Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant("}")));
            return  Expression.Lambda<Action<JsonSerializer, object>>(Expression.Block(new ParameterExpression[] { obj, jsonStrBuilder, stack}, expList), thisObjParameter, objParameter);
        }
        /// <summary>
        /// Create a serialization expression
        /// </summary>
        /// <param name="objType">Object type</param>
        /// <returns>Serialization expression</returns>
        private Expression<Action<JsonSerializer, object>> CreateSerializationExpression(Type objType)
        {
            //Current object
            var thisObjParameter = Expression.Parameter(typeof(JsonSerializer), "thisObjParameter");
            //Serialization object parameter
            var objParameter = Expression.Parameter(typeof(object));
            //Convert to target object
            var obj = Expression.Convert(objParameter, objType);
            //Expression
            Expression exp = null;

            //JSON node types are JSON already: write their text instead of reflecting over their members
            if (typeof(IJsonObject).IsAssignableFrom(objType))
            {
                return Expression.Lambda<Action<JsonSerializer, object>>(
                    Expression.Call(thisObjParameter, ((Action<IJsonObject>)SerializeJsonObject).Method, obj),
                    thisObjParameter, objParameter);
            }

            ObjectType objectType = GetObjectType(objType);
            switch (objectType)
            {
                case ObjectType.Boolean: exp = Expression.Call(thisObjParameter, ((Action<bool>)SerializeBoolean).Method, obj); break;
                case ObjectType.Int32: exp = Expression.Call(thisObjParameter, ((Action<int>)SerializeInt32).Method, obj); break;
                case ObjectType.Int64: exp = Expression.Call(thisObjParameter, ((Action<long>)SerializeInt64).Method, obj); break;
                case ObjectType.UInt32: exp = Expression.Call(thisObjParameter, ((Action<uint>)SerializeUInt32).Method, obj); break;
                case ObjectType.UInt64: exp = Expression.Call(thisObjParameter, ((Action<ulong>)SerializeUInt64).Method, obj); break;
                case ObjectType.Int16: exp = Expression.Call(thisObjParameter, ((Action<short>)SerializeInt16).Method, obj); break;
                case ObjectType.UInt16: exp = Expression.Call(thisObjParameter, ((Action<ushort>)SerializeUInt16).Method, obj); break;
                case ObjectType.Byte: exp = Expression.Call(thisObjParameter, ((Action<byte>)SerializeByte).Method, obj); break;
                case ObjectType.Char: exp = Expression.Call(thisObjParameter, ((Action<char>)SerializeChar).Method, obj); break;
                case ObjectType.Float: exp = Expression.Call(thisObjParameter, ((Action<float>)SerializeFloat).Method, obj); break;
                case ObjectType.Double: exp = Expression.Call(thisObjParameter, ((Action<double>)SerializeDouble).Method, obj); break;
                case ObjectType.Decimal: exp = Expression.Call(thisObjParameter, ((Action<decimal>)SerializeDecimal).Method, obj); break;
                case ObjectType.DateTime: exp = Expression.Call(thisObjParameter, ((Action<DateTime>)SerializeDateTime).Method, obj); break;
                case ObjectType.Enum: exp = Expression.Call(thisObjParameter, ((Action<System.Enum>)SerializeEnum).Method, Expression.Convert(obj, typeof(System.Enum))); break;
                case ObjectType.Guid: exp = Expression.Call(thisObjParameter, ((Action<Guid>)SerializeGuid).Method, obj); break;
                case ObjectType.String: exp = Expression.Call(thisObjParameter, ((Action<string>)SerializeString).Method, obj); break;
                case ObjectType.Nullable: exp = Expression.Call(thisObjParameter, ((Action<object>)SerializeAny).Method, objParameter); break;
                case ObjectType.Dictionary: exp = CreateDictionarySerializationExpression(thisObjParameter, objType, obj); break;
                case ObjectType.Enumerable: exp = Expression.Call(thisObjParameter, ((Action<IEnumerable>)SerializeEnumerable).Method, obj); break;
                default: exp = Expression.Invoke(CreateObjectSerializationExpression(objType), thisObjParameter, objParameter); break;
            }

            #region Custom serialization
            //Custom serialization object
            var method = typeof(IJsonCustomConverter).GetMethod("Serialize");

            List<Expression> customeConverterExpList = new List<Expression>();
            //var customConvertersField = typeof(JsonSerializer).GetField("_customConverters", BindingFlags.NonPublic | BindingFlags.Instance);
            //var customConvertersFieldExp = Expression.Field(thisObjParameter, customConvertersField);

            var customConverter = Expression.Variable(typeof(IJsonCustomConverter), "customConverter");
            var jsonStrBuilder = Expression.Variable(typeof(StringBuilder), "customConverter");

            customeConverterExpList.Add(Expression.Assign(customConverter, Expression.Call(thisObjParameter, ((Func<Type, IJsonCustomConverter>)GetCustomConverter).Method, Expression.Constant(objType))));
            customeConverterExpList.Add(Expression.Assign(jsonStrBuilder, Expression.Call(thisObjParameter, ((Func<StringBuilder>)GetStringBuilder).Method)));
            var callCoustomeConverterMethod = Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Call(customConverter, method, objParameter));
            customeConverterExpList.Add(Expression.IfThenElse(Expression.NotEqual(customConverter, Expression.Constant(null)), callCoustomeConverterMethod, exp));
            exp = Expression.Block(new ParameterExpression[] { customConverter, jsonStrBuilder }, customeConverterExpList);
            #endregion 

            var exp2 = Expression.Lambda<Action<JsonSerializer, object>>(exp, thisObjParameter, objParameter);


            return Expression.Lambda<Action<JsonSerializer, object>>(exp, thisObjParameter, objParameter);
        }
        /// <summary>
        /// Get the type of the object
        /// </summary>
        /// <param name="type">Serialization object type</param>
        /// <returns>Object type</returns>
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
            else if (type == typeof(Guid))
                return ObjectType.Guid;
            else if (typeof(IDictionary).IsAssignableFrom(type) || IsGenericDictionary(type))
                return ObjectType.Dictionary;
            else if (typeof(IEnumerable).IsAssignableFrom(type))
                return ObjectType.Enumerable;
            else
                return ObjectType.Object;
        }

        /// <summary>
        /// Whether the type is, or implements, a generic dictionary interface
        /// (IDictionary&lt;TKey,TValue&gt; or, when the target framework provides it,
        /// IReadOnlyDictionary&lt;TKey,TValue&gt;). These types are not assignable to the
        /// non-generic IDictionary, so they need the generic serializer.
        /// </summary>
        private static bool IsGenericDictionary(Type type)
        {
            Type keyType, valueType;
            return TryGetDictionaryTypes(type, out keyType, out valueType);
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
        private static Expression CreateDictionarySerializationExpression(ParameterExpression thisObjParameter, Type dictionaryType, Expression value)
        {
            Type keyType, valueType;
            if (TryGetDictionaryTypes(dictionaryType, out keyType, out valueType))
            {
                var pairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(pairType);
                return Expression.Call(thisObjParameter, _genericDictionarySerializer.MakeGenericMethod(keyType, valueType), Expression.Convert(value, enumerableType));
            }
            return Expression.Call(thisObjParameter, _dictionarySerializer, value);
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
                throw new Exception("当前对象不能转化成string类型");
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
            var del = GetSerializationAction(obj.GetType());
            del(this, obj);
        }

        /// <summary>
        /// Dictionary type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeDictionary(IDictionary obj)
        {
            if (IsCircularReference(obj))
            {
                throw new Exception("循环引用");
            }
            this._objStack.Push(obj);
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
            this._objStack.Pop();
        }
        /// <summary>
        /// Dictionary type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> obj)
        {
            if (IsCircularReference(obj))
            {
                throw new Exception("循环引用");
            }
            this._objStack.Push(obj);
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
            this._objStack.Pop();
        }
        /// <summary>
        /// Enumerable type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeEnumerable(IEnumerable obj)
        {
            if (IsCircularReference(obj))
            {
                throw new InvalidOperationException("循环引用");
            }
            this._objStack.Push(obj);
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
            this._objStack.Pop();
        }
        
        /// <summary>
        /// Check if circular reference exists
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>Whether circular reference exists</returns>
        private bool IsCircularReference(object obj)
        {
            foreach (var item in _objStack)
            {
                if (obj == item)
                    return true;
            }
            return false;
        }

        private StringBuilder GetStringBuilder()
        {
            return _jsonStrBuilder;
        }
        private Stack<object> GetStack()
        {
            return _objStack;
        }
        private IJsonCustomConverter GetCustomConverter(Type type)
        {
            if (_customConverters == null)
            {
                return null;
            }
            IJsonCustomConverter converter = null;
            if (_customConverters.TryGetValue(type, out converter) && (converter.CustomItem & JsonCustomConvertItem.CustomSerialize) == JsonCustomConvertItem.CustomSerialize)
            {
                return converter;
            }
            return null;
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
