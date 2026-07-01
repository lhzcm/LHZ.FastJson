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
    /// Custom conversion type
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public sealed class JsonCustomConvert<T> : IJsonCustomConverter
    {
        private Func<T, string> _serializeFunc = (T dist) => (new JsonSerializer(dist)).Serialize();
        private Func<IJsonObject, T> _deserializeFunc = (IJsonObject jsonObject) => JsonDeserialzerExpression<T>.Deserialzer(jsonObject);
        private readonly Type _type = typeof(T);
        private readonly JsonCustomConvertItem _item = JsonCustomConvertItem.None;
        private readonly bool _serializeValidate = false;
        /// <summary>
        /// Default constructor, using built-in serialization/deserialization
        /// </summary>
        public JsonCustomConvert()
        {
        }

        /// <summary>
        /// Custom serialization constructor
        /// </summary>
        /// <param name="customSerializeMethod">Custom serialization method</param>
        /// <param name="serializeValidate">Whether to validate serialization result</param>
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

        /// <summary>
        /// Custom deserialization constructor
        /// </summary>
        /// <param name="customDeserializeMethod">Custom deserialization method</param>
        public JsonCustomConvert(Func<IJsonObject, T> customDeserializeMethod)
        {
            if (customDeserializeMethod == null)
            {
                throw new ArgumentNullException(nameof(customDeserializeMethod));
            }
            _item = JsonCustomConvertItem.CustomDeSerialize;
            _deserializeFunc = customDeserializeMethod;
        }
        /// <summary>
        /// Constructor for custom serialization and deserialization
        /// </summary>
        /// <param name="customSerializeMethod">Custom serialization method</param>
        /// <param name="customDeserializeMethod">Custom deserialization method</param>
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
        /// The type for custom conversion
        /// </summary>
        public Type ConvertType => _type;

        Type IJsonCustomConverter.ConvertType => _type;

        /// <summary>
        /// Custom conversion item
        /// </summary>
        public JsonCustomConvertItem CustomItem => _item;
        JsonCustomConvertItem IJsonCustomConverter.CustomItem => _item;

        /// <summary>
        /// Custom serialization method
        /// </summary>
        /// <param name="dist">Object to serialize</param>
        /// <returns>Serialized string</returns>
        public string Serialize(T dist)
        {
            var jsonStr = _serializeFunc(dist);
            if (_serializeValidate&& !JsonReader.IsJsonString(jsonStr, out Exception readEx))
            {
                throw new JsonCustomConverterException(this, "该自定义序列化方法序列化出来的Json字符串是无效的Json字符串", readEx);
            }
            return jsonStr;
        }

        /// <summary>
        /// Custom deserialization method
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns>Deserialized object</returns>
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
