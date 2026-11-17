using LHZ.FastJson.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LHZ.FastJson.Json.Utils
{
    internal static class JsonUtility
    {
        /// <summary>
        /// Determine if properties with the same name exist
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="SameName">The duplicate name</param>
        /// <returns></returns>
        public static bool HasSameNameProperty(IEnumerable<PropertyInfo> properties, CaseStyle caseStyle, out string SameName)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (PropertyInfo property in properties)
            {
                string propertyName = GetPropertyName(property, caseStyle);
                if (names.Contains(propertyName))
                {
                    SameName = propertyName;
                    return true;
                }
                names.Add(propertyName);
            }
            SameName = null;
            return false;
        }
        /// <summary>
        /// Get the JSON property name corresponding to the type property
        /// </summary>
        /// <param name="propertyInfo">The property</param>
        /// <param name="caseStyle">The naming style (ignored when JsonProperty is set)</param>
        /// <returns>JSON property name</returns>
        public static string GetPropertyName(PropertyInfo propertyInfo, CaseStyle caseStyle)
        {
            string propertyName = propertyInfo.Name;
            var jsonPropertyAttr = Attribute.GetCustomAttribute(propertyInfo, typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
            if (jsonPropertyAttr != null)
            {
                return jsonPropertyAttr.PropertyName;
            }
            return JsonConvertOptions.ConvertName(propertyName, caseStyle);
        }
    }
}
