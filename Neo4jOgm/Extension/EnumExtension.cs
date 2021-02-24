using System.ComponentModel;
using Neo4jOgm.Domain;

namespace Neo4jOgm.Extension
{
    public static class EnumExtension
    {
        public static string GetSymbol(this RelationshipDirection val)
        {
            return GetEnumDesc(val);
        }
        
        public static string GetSymbol(this Operator val)
        {
            return GetEnumDesc(val);
        }

        private static string GetEnumDesc<T>(this T val)
        {
            var attributes = (DescriptionAttribute[]) val
                .GetType()
                .GetField(val.ToString())
                ?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes != null && attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}