using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Neo4jOgm.Extension
{
    public static class PropertyIfExtensions
    {
        public static bool HasAttribute(this PropertyInfo prop, Type t)
        {
            return prop.GetCustomAttribute(t) != null;
        }

        public static bool IsNullableLong(this PropertyInfo prop)
        {
            return prop.PropertyType == typeof(long?);
        }

        public static bool IsDateTime(this PropertyInfo prop)
        {
            return prop.PropertyType == typeof(DateTime?) || prop.PropertyType == typeof(DateTime);
        }

        public static bool IsCollection(this PropertyInfo prop)
        {
            return prop.PropertyType.GetInterface(nameof(ICollection)) != null;
        }
    }
}