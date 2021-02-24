using System;
using System.Reflection;
using Neo4jOgm.Attribute;
using Neo4jOgm.Extension;

namespace Neo4jOgm.Domain
{
    public abstract class MetaProperty
    {
        public PropertyInfo Info { get; }

        protected MetaProperty(PropertyInfo info)
        {
            Info = info;
        }

        public string GetName()
        {
            return Info.Name;
        }

        public object GetValue(object instance)
        {
            return Info.GetValue(instance);
        }

        public void SetValue(object instance, object value)
        {
            Info.SetValue(instance, value);
        }
    }

    public class RegularProperty : MetaProperty
    {
        public bool IsCreatedAt { get; init; }

        public bool IsUpdatedAt { get; init; }

        public RegularProperty(PropertyInfo info) : base(info)
        {
        }
    }

    public class RelationshipProperty : MetaProperty
    {
        public bool IsCollection { get; }

        public Type EntityType { get; }

        public RelationshipDirection Direction { get; }

        public string RelationshipType { get; }

        public RelationshipProperty(PropertyInfo info, NeoRelationship attribute) : base(info)
        {
            var t = Info.PropertyType;
            IsCollection = Info.IsCollection();
            EntityType = IsCollection ? t.GetTypeInfo().GenericTypeArguments[0] : t;
            Direction = attribute.Direction;
            RelationshipType = attribute.Type;
        }
    }
}