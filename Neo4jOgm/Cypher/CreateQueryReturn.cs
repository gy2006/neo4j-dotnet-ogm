using System.Collections.Generic;
using System.Text;

namespace Neo4jOgm.Cypher
{
    internal class CreateQueryReturn
    {
        public StringBuilder Query { get; } = new();

        public IDictionary<string, object> FlattedEntities { get; } = new Dictionary<string, object>();

        public void Append(CreateQueryReturn r)
        {
            Query.AppendLine(r.Query.ToString());
            foreach (var pair in r.FlattedEntities)
            {
                FlattedEntities.Add(pair);    
            }
        }
    }
}