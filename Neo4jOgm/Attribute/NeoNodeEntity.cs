using System;

namespace Neo4jOgm.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NeoNodeEntity : System.Attribute
    {
        public string Label { get; }
        
        public string Key { get; }

        public NeoNodeEntity(string label, string key)
        {
            Label = label;
            Key = key;
        }
    }
}