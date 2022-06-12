using LHZ.FastJson.Enum;
using LHZ.FastJson.Json.Format;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LHZ.FastJson.Json
{
    public class JsonSerializer
    {
        private StringBuilder _jsonStrBuilder = new StringBuilder(128);
        private Stack<object> _objStack = new Stack<object>();
        private JsonFormatter _formater;

        private static readonly Dictionary<Type, ObjectType> _objectTypes = JsonObjectType.GetObjectTypes();
        private static readonly Dictionary<Type, Action<JsonSerializer, object>> _serializationActions = new Dictionary<Type, Action<JsonSerializer, object>>(4049);

        private object _obj;

        public JsonSerializer(object obj)
        {
            this._obj = obj;
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
            if (_obj == null)
            {
                return "null";
            }
            var del = GetSerializationAction(_obj.GetType());
            del(this, _obj);
            return _jsonStrBuilder.ToString();
        }

        /// <summary>
        /// 通过对象类型获取对象序列化的方法
        /// </summary>
        /// <param name="objType">对象类型</param>
        /// <returns>对象序列化委托方法</returns>
        private Action<JsonSerializer, object> GetSerializationAction(Type objType)
        {
            Action<JsonSerializer, object> act = null;
            if (_serializationActions.TryGetValue(objType, out act))
            {
                return act;
            }
            Expression<Action<JsonSerializer, object>> exp = CreateSerializationExpression(objType);
            act = exp.Compile();
            _serializationActions[objType] = act;
            return act;
        }

        /// <summary>
        /// 通过对象类型创建对象序列化的表达式
        /// </summary>
        /// <param name="objType">对象类型</param>
        /// <returns>对象序列化表达式</returns>
        private Expression<Action<JsonSerializer, object>> CreateObjectSerializationExpression(Type objType)
        {
            //当前对象
            var thisObjParameter = Expression.Parameter(typeof(JsonSerializer), "thisObjParameter");
            //序列化对象参数
            var objParameter = Expression.Parameter(typeof(object), "objParameter");
            //jsonStrBuilder对象参数
            var jsonStrBuilder = Expression.Parameter(typeof(StringBuilder), "strBuilder");
            //循环引用栈对象参数
            var stack = Expression.Parameter(typeof(Stack<object>), "stack");
            //对对象进行转换
            var obj = Expression.Parameter(objType, "obj");
            //表达式列表
            List<Expression> expList = new List<Expression>();
            

            //是否可能循环引用
            bool maybeCircularReference = false;
            //获取属性
            var properties = objType.GetProperties().Where(n => n.CanRead).ToList();

            //变量赋值
            expList.Add(Expression.Assign(obj, Expression.Convert(objParameter, objType)));
            expList.Add(Expression.Assign(jsonStrBuilder, Expression.Call(thisObjParameter, ((Func<StringBuilder>) GetStringBuilder).Method)));
            
            

            expList.Add(Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant("{")));

            foreach (var item in properties)
            {
                ObjectType objectType = GetObjectType(item.PropertyType);
                Expression exp = null;
                Func<Type, Action<JsonSerializer, object>> sact = GetSerializationAction;

                expList.Add(Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant( "\"" + item.Name + "\":")));
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
                    case ObjectType.Enum: exp = Expression.Call(thisObjParameter, ((Action<object>)SerializeEnum).Method, Expression.Property(obj, item.Name)); break;
                    case ObjectType.String: exp = Expression.Call(thisObjParameter, ((Action<string>)SerializeString).Method, Expression.Property(obj, item.Name)); break;
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
                //判断引用类型是否为空
                if (objectType == ObjectType.String || objectType == ObjectType.Dictionary || objectType == ObjectType.Enumerable || objectType == ObjectType.Object)
                {
                    var propertyNull = Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant("null"));
                    exp = Expression.IfThenElse(Expression.Equal(Expression.Property(obj, item.Name), Expression.Constant(null)), propertyNull, exp);
                }
                expList.Add(exp);
                //判断是否是最后一个字段，如果不是则需要加上“,”
                if (properties.IndexOf(item) < properties.Count - 1)
                {
                    expList.Add(Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant(",")));
                }
            }
            //循环引用判断
            if (maybeCircularReference && !objType.IsValueType)
            {
                
                var isCircularReference = Expression.Call(thisObjParameter, ((Func<object, bool>)IsCircularReference).Method, objParameter);
                var judgeCircularReference = Expression.IfThen(Expression.IsTrue(isCircularReference), Expression.Throw(Expression.Constant(new Exception("循环引用"))));

                expList.Insert(0, Expression.Assign(stack, Expression.Call(thisObjParameter, ((Func<Stack<object>>)GetStack).Method)));
                expList.Insert(1, judgeCircularReference);
                expList.Insert(2, Expression.Call(stack, typeof(Stack<object>).GetMethod("Push", new Type[]{ typeof(object) }), objParameter));
                expList.Add(Expression.Call(stack, typeof(Stack<object>).GetMethod("Pop")));
            }

            expList.Add(Expression.Call(jsonStrBuilder, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }), Expression.Constant("}")));
            return  Expression.Lambda<Action<JsonSerializer, object>>(Expression.Block(new ParameterExpression[] { obj, jsonStrBuilder, stack}, expList), thisObjParameter, objParameter);
        }
       
        /// <summary>
        /// 创建序列化的表达式
        /// </summary>
        /// <param name="objType">对象类型</param>
        /// <returns>序列化表达式</returns>
        private Expression<Action<JsonSerializer, object>> CreateSerializationExpression(Type objType)
        {
            //当前对象
            var thisObjParameter = Expression.Parameter(typeof(JsonSerializer), "thisObjParameter");
            //序列化对象参数
            var objParameter = Expression.Parameter(typeof(object));
            //对象进行转换
            var obj = Expression.Convert(objParameter, objType);
           
            //表达式
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
                case ObjectType.Enum: exp = Expression.Call(thisObjParameter, ((Action<object>)SerializeEnum).Method, obj); break;
                case ObjectType.String: exp = Expression.Call(thisObjParameter, ((Action<string>)SerializeString).Method, obj); break;
                case ObjectType.Dictionary: exp = Expression.Call(thisObjParameter, ((Action<IDictionary>)SerializeDictionary).Method, obj); break;
                case ObjectType.Enumerable: exp = Expression.Call(thisObjParameter, ((Action<IEnumerable>)SerializeEnumerable).Method, obj); break;
                default: exp = Expression.Invoke(CreateObjectSerializationExpression(objType), thisObjParameter, objParameter); break;
            }
 
            return Expression.Lambda<Action<JsonSerializer, object>>(exp, thisObjParameter, objParameter);
        }

        /// <summary>
        /// 获取对象的类型
        /// </summary>
        /// <param name="type">序列化对象类型</param>
        /// <returns>对象类型</returns>
        private ObjectType GetObjectType(Type type)
        {
            ObjectType objType;
            if (_objectTypes.TryGetValue(type, out objType))
            {
                return objType;
            }

            if (type.IsEnum)
                return ObjectType.Enum;
            else if (typeof(IDictionary).IsAssignableFrom(type))
                return ObjectType.Dictionary;
            else if (typeof(IEnumerable).IsAssignableFrom(type))
                return ObjectType.Enumerable;
            else
                return ObjectType.Object;
        }

        /// <summary>
        /// bool类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeBoolean(bool obj)
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
        private void SerializeByte(byte obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// char类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeChar(char obj)
        {
            _jsonStrBuilder.Append("\"" + obj.ToString() + "\"");
        }

        /// <summary>
        /// int16类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeInt16(short obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// uint16类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeUInt16(ushort obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }


        /// <summary>
        /// int32类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeInt32(int obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// uint32类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeUInt32(uint obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// int64类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeInt64(long obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// uint64类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeUInt64(ulong obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// float类型序列化
        /// </summary>
        /// <param name="obj"></param>
        private void SerializeFloat(float obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }

        /// <summary>
        /// double类型序列
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeDouble(double obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }
        private void SerializeDecimal(decimal obj)
        {
            _jsonStrBuilder.Append(obj.ToString());
        }
        /// <summary>
        /// DateTime类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeDateTime(DateTime obj)
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
        private void SerializeString(string obj)
        {
            if (obj == null)
            {
                throw new Exception("当前对象不能转化成string类型");
            }
            _jsonStrBuilder.Append('"');
            unsafe
            {
                fixed (char* test = obj)
                {
                    char* start = test;
                    char* cur = test;

                    while (*cur != '\0')
                    {
                        if (*cur < 0x20 || *cur == '"' || *cur == '\\')
                        {
                            if (cur > start)
                            {
#if NET40 || NET45
                                _jsonStrBuilder.Append(obj, (int)(start - test), (int)(cur - start));
#else
                                _jsonStrBuilder.Append(start, (int)(cur - start));
#endif
                            }
                            _jsonStrBuilder.Append(CharParaphrase(*cur));
                            start = cur + 1;
                        }
                        cur++;
                    }
                    if (cur > start)
                    {
#if NET40 || NET45
                        _jsonStrBuilder.Append(obj, (int)(start - test), (int)(cur - start));
#else
                        _jsonStrBuilder.Append(start, (int)(cur - start));
#endif
                    }

                }
            }
            _jsonStrBuilder.Append('"');

            //_jsonStrBuilder.Append('"');

            //foreach (char item in obj)
            //{
            //    if (item < 0x20 || item == '"' || item == '\\')
            //        _jsonStrBuilder.Append(CharParaphrase(item));
            //    else
            //        _jsonStrBuilder.Append(item);
            //}
            //_jsonStrBuilder.Append('"');
        }
        /// <summary>
        /// Dictionary类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeDictionary(IDictionary obj)
        {
            _jsonStrBuilder.Append("{");
            int i = 0;
            foreach (DictionaryEntry item in obj)
            {
                if (i == 0)
                    _jsonStrBuilder.Append("\"" + item.Key.ToString() + "\":");
                else
                    _jsonStrBuilder.Append(",\"" + item.Key.ToString() + "\":");
                var del = GetSerializationAction(item.Value.GetType());
                del(this, item.Value);
                i++;
            }
            _jsonStrBuilder.Append("}");
        }
        /// <summary>
        /// Enumerable类型序列化
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        private void SerializeEnumerable(IEnumerable obj)
        {
            _jsonStrBuilder.Append("[");
            int i = 0;
            foreach (var item in obj)
            {
                if (i != 0)
                    _jsonStrBuilder.Append(",");
                var del = GetSerializationAction(item.GetType());
                del(this, item);
                i++;
            }
            _jsonStrBuilder.Append("]");
        }
        
        /// <summary>
        /// 是否循环引用判断
        /// </summary>
        /// <param name="obj">判断对象</param>
        /// <returns>是否循环引用</returns>
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

        /// <summary>
        /// 字符转义
        /// </summary>
        /// <param name="paraphrase">需要转义的字符</param>
        /// <returns>转义好的字符串</returns>
        private string CharParaphrase(char paraphrase)
        {
            switch (paraphrase)
            {
                case '"': return "\\\"";
                case '\\': return "\\\\";
                case '\n': return "\\n";
                case '\t': return "\\t";
                case '\a': return "\\a";
                case '\b': return "\\b";
                case '\f': return "\\f";
                case '\r': return "\\r";
                case '\v': return "\\v";
                default : return paraphrase.ToString();
            }
        }
    }
}
