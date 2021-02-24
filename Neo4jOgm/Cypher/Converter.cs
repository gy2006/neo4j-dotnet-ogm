using System;

namespace Neo4jOgm.Cypher
{
    public interface ICypherConverter
    {
        public string ToQuery(object value);
        
        public object ToValue(object neoValue, Type target);
    }
}