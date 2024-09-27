﻿using LHZ.FastJson.Json.Attributes;
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
        /// 判断是否存在相同名字的属性
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="SameName">相同的名字</param>
        /// <returns></returns>
        public static bool HasSameNameProperty(IEnumerable<PropertyInfo> properties, out string SameName)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                var jsonPropertyAttr = Attribute.GetCustomAttribute(property, typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
                if (jsonPropertyAttr != null)
                {
                    propertyName = jsonPropertyAttr.PropertyName;
                }
                if (names.Contains(propertyName))
                {
                    SameName = propertyName;
                    return true;
                }
            }
            SameName = null;
            return false;
        }
    }
}
