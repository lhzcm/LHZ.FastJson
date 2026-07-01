using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Schema;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Enum.CustomConverter;
using LHZ.FastJson.Interface;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.Format;
using LHZ.FastJson.Json.Utils;

namespace LHZ.FastJson.Json
{
    /// <summary>
    /// JSON serialization class
    /// </summary>
    public class JsonSerializer
    {
        private StringBuilder _jsonStrBuilder = new StringBuilder(128);
        private Stack<object> _objStack = new Stack<object>();
        [Obsolete]
        private JsonFormatter _formater;

        private static readonly Dictionary<Type, ObjectType> _objectTypes = JsonObjectType.GetObjectTypes();
        private static readonly ConcurrentDictionary<Type, Action<JsonSerializer, object>> _serializationActions = new ConcurrentDictionary<Type, Action<JsonSerializer, object>>();

        private object _obj;

        private Dictionary<Type, IJsonCustomConverter> _customConverters;

        /// <summary>
        /// Initialize the serializer with an object
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        public JsonSerializer(object obj)
        {
            this._obj = obj;
        }
        /// <summary>
        /// Initialize with formatters (deprecated)
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="formats">Formatters</param>
        [Obsolete]
        public JsonSerializer(object obj, IJsonFormat[] formats)
        {
            this._obj = obj;
            this._formater = new JsonFormatter(formats);
        }

        /// <summary>
        /// Initialize with custom converters
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="jsonCustomConverters">Custom converters</param>
        public JsonSerializer(object obj, params IJsonCustomConverter[] jsonCustomConverters)
        {
            this._obj = obj;
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
            return _serializationActions.GetOrAdd(objType, type => CreateSerializationExpression(type).Compile());
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

            //Whether circular reference is possible
            bool maybeCircularReference = false;
            //Get properties
            var properties = objType.GetProperties().Where(n => {
                if (!n.CanRead)
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
            if (JsonUtility.HasSameNameProperty(properties, out string samePropertyName))
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
                Func<Type, Action<JsonSerializer, object>> sact = GetSerializationAction;

                #region Custom property name handling
                string jsonPropertyName = JsonUtility.GetPropertyName(item);
                #endregion
                expList.Add(Expression.Call(thisObjParameter, ((Action<string>)SerializePropertyName).Method, Expression.Constant(jsonPropertyName)));
                switch (objectType)
                {
                    case ObjectType.Boolean: exp = Expression.Call(thisObjParameter, ((Action<bool>)SerializeBoolean).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Int32: exp = Expression.Call(thisObjParameter, ((Action<int>)SerializeInt32).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Int64: exp = Expression.Call(thisObjParameter, ((Action<long>)SerializeInt64).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.UInt32: exp = Expression.Call(thisObjParameter, ((Action<uint>)SerializeUInt32).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.UInt64: exp = Expression.Call(thisObjParameter, ((Action<ulong>)SerializeUInt64).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Int16: exp = Expression.Call(thisObjParameter, ((Action<short>)SerializeInt16).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.UInt16: exp = Expression.Call(thisObjParameter, ((Action<ushort>)SerializeUInt16).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Byte: exp = Expression.Call(thisObjParameter, ((Action<byte>)SerializeByte).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Char: exp = Expression.Call(thisObjParameter, ((Action<char>)SerializeChar).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Float: exp = Expression.Call(thisObjParameter, ((Action<float>)SerializeFloat).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Double: exp = Expression.Call(thisObjParameter, ((Action<double>)SerializeDouble).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Decimal: exp = Expression.Call(thisObjParameter, ((Action<decimal>)SerializeDecimal).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.DateTime: exp = Expression.Call(thisObjParameter, ((Action<DateTime>)SerializeDateTime).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Enum: exp = Expression.Call(thisObjParameter, ((Action<System.Enum>)SerializeEnum).Method, Expression.Convert(Expression.Property(obj, item.Name), typeof(System.Enum))); break;
                    case ObjectType.Guid: exp = Expression.Call(thisObjParameter, ((Action<Guid>)SerializeGuid).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.String: exp = Expression.Call(thisObjParameter, ((Action<string>)SerializeString).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.Nullable: exp = Expression.Call(thisObjParameter, ((Action<object>)SerializeAny).Method, Expression.Convert(Expression.Property(obj, item.Name), typeof(object))); break;
                    case ObjectType.Dictionary:
                        {
                            exp = Expression.Call(thisObjParameter, ((Action<IDictionary>)SerializeDictionary).Method, Expression.Property(obj, item.Name));
                            maybeCircularReference = true; 
                            break;
                        }
                    case ObjectType.Enumerable:
                        {
                            exp = Expression.Call(thisObjParameter, ((Action<IEnumerable>)SerializeEnumerable).Method, Expression.Property(obj, item.Name));
                            maybeCircularReference = true;
                            break;
                        }
                    default:
                        {
                            exp = Expression.Invoke(Expression.Call(thisObjParameter, sact.Method, Expression.Call(Expression.Property(obj, item.Name), typeof(object).GetMethod("GetType"))), thisObjParameter, Expression.Property(obj, item.Name));
                            maybeCircularReference = true;
                            break;
                        }
                }
                ////Get property type serialization (this method does not determine the actual type of each object)
                // var expressionMethods = this.GetType().GetMethods((BindingFlags) 0x7FFFFFFF).First(n => n.Name == "CreateSerializationExpression" && n.IsGenericMethod).MakeGenericMethod(item.PropertyType);
                // var serializationExpression = expressionMethods.Invoke(this, null) as Expression;
                // exp = Expression.Invoke(serializationExpression, new Expression[]{thisObjParameter, Expression.Property(obj, item.Name)});

                if(objectType == ObjectType.Dictionary || objectType == ObjectType.Enumerable || objectType == ObjectType.Object)
                {
                    maybeCircularReference = true;
                }

                //Check if reference type is null
                if (objectType == ObjectType.String || objectType == ObjectType.Dictionary || objectType == ObjectType.Enumerable || objectType == ObjectType.Object)
                {
                    //Need to check for circular reference
                    if(objectType != ObjectType.String)
                    {
                        maybeCircularReference = true;
                    }
                    //Perform null check
                    var propertyNull = Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant("null"));
                    exp = Expression.IfThenElse(Expression.Equal(Expression.Property(obj, item.Name), Expression.Constant(null)), propertyNull, exp);
                }
                expList.Add(exp);
                //Check if it is the last field; if not, add ","
                if (properties.IndexOf(item) < properties.Count - 1)
                {
                    expList.Add(Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant(",")));
                }
            }
            //Circular reference check
            if (maybeCircularReference && !objType.IsValueType)
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
                case ObjectType.Enum: exp = Expression.Call(thisObjParameter, ((Action<System.Enum>)SerializeEnum).Method, obj); break;
                case ObjectType.Guid: exp = Expression.Call(thisObjParameter, ((Action<Guid>)SerializeGuid).Method, obj); break;
                case ObjectType.String: exp = Expression.Call(thisObjParameter, ((Action<string>)SerializeString).Method, obj); break;
                case ObjectType.Nullable: exp = Expression.Call(thisObjParameter, ((Action<object>)SerializeAny).Method, objParameter); break;
                case ObjectType.Dictionary: exp = Expression.Call(thisObjParameter, ((Action<IDictionary>)SerializeDictionary).Method, obj); break;
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
        /// Create a serialization expression (generic)
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <returns>Expression</returns>
        private Expression<Action<JsonSerializer, T>> CreateSerializationExpression<T>()
        {
            //Object type
            var objType = typeof(T);
            //Current object
            var thisObjParameter = Expression.Parameter(typeof(JsonSerializer), "thisObjParameter");
            //Serialization object parameter
            var objParameter = Expression.Parameter(objType);

            //Expression
            Expression exp = null;
        

            ObjectType objectType = GetObjectType(objType);
            switch (objectType)
            {
                case ObjectType.Boolean: exp = Expression.Call(thisObjParameter, ((Action<bool>)SerializeBoolean).Method, objParameter); break;
                case ObjectType.Int32: exp = Expression.Call(thisObjParameter, ((Action<int>)SerializeInt32).Method, objParameter); break;
                case ObjectType.Int64: exp = Expression.Call(thisObjParameter, ((Action<long>)SerializeInt64).Method, objParameter); break;
                case ObjectType.UInt32: exp = Expression.Call(thisObjParameter, ((Action<uint>)SerializeUInt32).Method, objParameter); break;
                case ObjectType.UInt64: exp = Expression.Call(thisObjParameter, ((Action<ulong>)SerializeUInt64).Method, objParameter); break;
                case ObjectType.Int16: exp = Expression.Call(thisObjParameter, ((Action<short>)SerializeInt16).Method, objParameter); break;
                case ObjectType.UInt16: exp = Expression.Call(thisObjParameter, ((Action<ushort>)SerializeUInt16).Method, objParameter); break;
                case ObjectType.Byte: exp = Expression.Call(thisObjParameter, ((Action<byte>)SerializeByte).Method, objParameter); break;
                case ObjectType.Char: exp = Expression.Call(thisObjParameter, ((Action<char>)SerializeChar).Method, objParameter); break;
                case ObjectType.Float: exp = Expression.Call(thisObjParameter, ((Action<float>)SerializeFloat).Method, objParameter); break;
                case ObjectType.Double: exp = Expression.Call(thisObjParameter, ((Action<double>)SerializeDouble).Method, objParameter); break;
                case ObjectType.Decimal: exp = Expression.Call(thisObjParameter, ((Action<decimal>)SerializeDecimal).Method, objParameter); break;
                case ObjectType.DateTime: exp = Expression.Call(thisObjParameter, ((Action<DateTime>)SerializeDateTime).Method, objParameter); break;
                case ObjectType.Enum: exp = Expression.Call(thisObjParameter, ((Action<System.Enum>)SerializeEnum).Method, objParameter); break;
                case ObjectType.String: exp = Expression.Call(thisObjParameter, ((Action<string>)SerializeString).Method, objParameter); break;
                case ObjectType.Guid: exp = Expression.Call(thisObjParameter, ((Action<Guid>)SerializeGuid).Method, objParameter); break;
                case ObjectType.Nullable: exp = Expression.Call(thisObjParameter, ((Action<object>)SerializeAny).Method, Expression.Convert(objParameter, typeof(object))); break;
                case ObjectType.Dictionary: exp = Expression.Call(thisObjParameter, ((Action<IDictionary>)SerializeDictionary).Method, objParameter); break;
                case ObjectType.Enumerable: exp = Expression.Call(thisObjParameter, ((Action<IEnumerable>)SerializeEnumerable).Method, objParameter); break;
                default: exp = Expression.Invoke(CreateObjectSerializationExpression(objType), thisObjParameter, objParameter); break;
            }
            return Expression.Lambda<Action<JsonSerializer, T>>(exp, thisObjParameter, objParameter);
        }

        /// <summary>
        /// Get the type of the object
        /// </summary>
        /// <param name="type">Serialization object type</param>
        /// <returns>Object type</returns>
        private ObjectType GetObjectType(Type type)
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
            else if (typeof(IDictionary).IsAssignableFrom(type))
                return ObjectType.Dictionary;
            else if (typeof(IEnumerable).IsAssignableFrom(type))
                return ObjectType.Enumerable;
            else
                return ObjectType.Object;
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
            if (_formater == null)
            {
                SerializeString(obj.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                return;
            }
            var dateStr = _formater.DateTimeFormat(obj, out bool execCharParaphrase);
            if (execCharParaphrase)
            {
                SerializeString(dateStr);
            }
            else
            {
                _jsonStrBuilder.Append("\"" + dateStr + "\"");
            }
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
        }
        /// <summary>
        /// Enumerable type serialization
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        private void SerializeEnumerable(IEnumerable obj)
        {
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
        private string CharParaphrase(char paraphrase)
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
