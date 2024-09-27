using LHZ.FastJson.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LHZ.FastJson.Json.Utils
{
    internal static class JsonSerializerUtility
    {
        public static string GetPropertyName(PropertyInfo propertyInfo)
        {
            string propertyName = propertyInfo.Name;
            var jsonPropertyAttr = Attribute.GetCustomAttribute(propertyInfo, typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
            if (jsonPropertyAttr != null)
            {
                propertyName = jsonPropertyAttr.PropertyName;
            }
            return propertyName;
        }
    }
}
