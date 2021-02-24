using System;
using Neo4jOgm.Domain;

namespace Neo4jOgm.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NeoRelationship : System.Attribute
    {
        public string Type { get; }

        public RelationshipDirection Direction { get; } = RelationshipDirection.Out;

        public NeoRelationship(string type)
        {
            Type = type;
        }
    }
}