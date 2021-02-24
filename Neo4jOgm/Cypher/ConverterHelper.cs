using System;
using System.Collections.Generic;
using Neo4j.Driver;

namespace Neo4jOgm.Cypher
{
    public abstract class ConverterHelper
    {
        private static readonly DatetimeConverter DatetimeConverter = new();
        
        private static readonly NumberConverter NumberConverter = new();

        private static readonly IDictionary<Type, ICypherConverter> QueryConverters =
            new Dictionary<Type, ICypherConverter>
            {
                [typeof(DateTime)] = DatetimeConverter,
                [typeof(int)] = NumberConverter,
                [typeof(long)] = NumberConverter,
                [typeof(double)] = NumberConverter,
                [typeof(float)] = NumberConverter
            };

        private static readonly IDictionary<Type, ICypherConverter> NeoTypeConverters =
            new Dictionary<Type, ICypherConverter>
            {
                [typeof(ZonedDateTime)] = DatetimeConverter,
                [typeof(int)] = NumberConverter,
                [typeof(long)] = NumberConverter,
                [typeof(double)] = NumberConverter,
                [typeof(float)] = NumberConverter
            };

        public static string GetQueryString(object val)
        {
            if (val == null)
            {
                return string.Empty;
            }

            QueryConverters.TryGetValue(val.GetType(), out var converter);
            return converter == null ? $"'{val}'" : converter.ToQuery(val);
        }

        public static object ToObjectValue(object neoVal, Type targetType)
        {
            if (neoVal == null)
            {
                return null;
            }

            NeoTypeConverters.TryGetValue(neoVal.GetType(), out var converter);
            return converter == null ? neoVal : converter.ToValue(neoVal, targetType);
        }
    }
}