using System;
using Neo4j.Driver;

namespace Neo4jOgm.Cypher
{
    public class DatetimeConverter : ICypherConverter
    {
        public string ToQuery(object value)
        {
            var dt = (DateTime) value;
            return $"datetime(\"{dt.ToString(Config.DateTimeFormat)}\")";
        }

        public object ToValue(object neoValue, Type target)
        {
            var zonedDateTime = (ZonedDateTime) neoValue;
            return zonedDateTime.ToDateTimeOffset().UtcDateTime;
        }
    }

    /**
     * Support int, float, double and long
     */
    public class NumberConverter : ICypherConverter
    {
        private static readonly Type ConvertType = typeof(Convert);

        public string ToQuery(object value)
        {
            return $"{value}";
        }

        public object ToValue(object neoValue, Type target)
        {
            if (neoValue.GetType() == target)
            {
                return neoValue;
            }

            var methodInfo = ConvertType.GetMethod($"To{target.Name}", new[] {neoValue.GetType()});
            return methodInfo?.Invoke(null, new[] {neoValue});
        }
    }
}