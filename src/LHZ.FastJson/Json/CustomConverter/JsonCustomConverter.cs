using LHZ.FastJson.Enum.CustomConverter;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LHZ.FastJson.Json.CustomConverter
{
    /// <summary>
    /// 自定义转换类型
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public sealed class JsonCustomConvert<T> : IJsonCustomConverter
    {
        private Func<T, string> _serializeFunc = (T dist) => (new JsonSerializer(dist)).Serialize();
        private Func<IJsonObject, T> _deserializeFunc = (IJsonObject jsonObject) => JsonDeserialzerExpression<T>.Deserialzer(jsonObject);
        private readonly Type _type = typeof(T);
        private readonly JsonCustomConvertItem _item = JsonCustomConvertItem.None;
        private readonly bool _serializeValidate = false;
        public JsonCustomConvert()
        {
        }

        public JsonCustomConvert(Func<T, string> customSerializeMethod, bool serializeValidate = false)
        {
            if (customSerializeMethod == null)
            {
                throw new ArgumentNullException(nameof(customSerializeMethod));
            }
            _item = JsonCustomConvertItem.CustomSerialize;
            _serializeFunc = customSerializeMethod;
            _serializeValidate = serializeValidate;
        }

        public JsonCustomConvert(Func<IJsonObject, T> customDeserializeMethod)
        {
            if (customDeserializeMethod == null)
            {
                throw new ArgumentNullException(nameof(customDeserializeMethod));
            }
            _item = JsonCustomConvertItem.CustomDeSerialize;
            _deserializeFunc = customDeserializeMethod;
        }

        public JsonCustomConvert(Func<T, string> customSerializeMethod, Func<IJsonObject, T> customDeserializeMethod)
        {
            _item = JsonCustomConvertItem.All;
            if (customDeserializeMethod == null)
            {
                throw new ArgumentNullException(nameof(customDeserializeMethod));
            }
            _serializeFunc = customSerializeMethod;
            if (customSerializeMethod == null)
            {
                throw new ArgumentNullException(nameof(customSerializeMethod));
            }
            _deserializeFunc = customDeserializeMethod;
        }

        /// <summary>
        /// 自定义转换的类型
        /// </summary>
        public Type ConvertType => _type;

        Type IJsonCustomConverter.ConvertType => _type;

        /// <summary>
        /// 自定义转换项
        /// </summary>
        public JsonCustomConvertItem CustomItem => _item;
        JsonCustomConvertItem IJsonCustomConverter.CustomItem => _item;

        /// <summary>
        /// 自定义序列化方法
        /// </summary>
        /// <param name="dist">序列化对象</param>
        /// <returns>序列化字符串</returns>
        public string Serialize(T dist)
        {
            var jsonStr = _serializeFunc(dist);
            if (_serializeValidate&& !JsonReader.IsJsonString(jsonStr, out Exception readEx))
            {
                throw new JsonCustomConverterException(this, "该自定义序列化方法序列化出来的Json字符串是无效的Json字符串", readEx);
            }
            return _serializeFunc(dist);
        }

        /// <summary>
        /// 自定义反序列方法
        /// </summary>
        /// <param name="jsonObject">json对象</param>
        /// <returns>反序列化对象</returns>
        public T Deserialize(IJsonObject jsonObject)
        {
            return _deserializeFunc(jsonObject);
        }

        string IJsonCustomConverter.Serialize(object dist)
        {
            return Serialize((T)dist);
        }

        object IJsonCustomConverter.Deserialize(IJsonObject jsonObject)
        {
            return Deserialize(jsonObject);
        }
    }
}
