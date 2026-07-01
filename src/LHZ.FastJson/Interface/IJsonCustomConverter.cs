using LHZ.FastJson.Enum.CustomConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Interface
{
    // /// <summary>
    // /// Custom conversion type
    // /// </summary>
    // /// <typeparam name="T">The type requiring custom conversion</typeparam>
    // public interface IJsonCustomConverter<T> : IJsonCustomConverter
    // {
    //     /// <summary>
    //     /// Custom serialization method
    //     /// </summary>
    //     /// <param name="dist">Object to serialize</param>
    //     /// <returns>Serialized string</returns>
    //     public string Serialize(T dist);

    //     /// <summary>
    //     /// Custom deserialization method
    //     /// </summary>
    //     /// <param name="jsonObject">JSON object</param>
    //     /// <returns>Deserialized object</returns>
    //     public T Deserialize(IJsonObject jsonObject);
    // }


    /// <summary>
    /// Custom conversion type
    /// </summary>
    public interface IJsonCustomConverter
    {
        /// <summary>
        /// Custom serialization method
        /// </summary>
        /// <param name="dist">Object to serialize</param>
        /// <returns>Serialized string</returns>
        string Serialize(object dist);

        /// <summary>
        /// Custom deserialization method
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <returns>Deserialized object</returns>
        object Deserialize(IJsonObject jsonObject);

        /// <summary>
        /// The target type for custom conversion
        /// </summary>
        Type ConvertType { get; }
        /// <summary>
        /// Included custom items (serialization | deserialization)
        /// </summary>
        JsonCustomConvertItem CustomItem { get; }
    }
}
