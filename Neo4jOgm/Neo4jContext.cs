using System;
using System.Collections.Generic;
using System.Reflection;
using Neo4jOgm.Attribute;
using Neo4jOgm.Domain;
using Neo4jOgm.Extension;

namespace Neo4jOgm
{
    public class NeoContext
    {
        private static readonly Type NodeEntityType = typeof(NeoNodeEntity);

        private static readonly Type NodeIdType = typeof(NeoNodeId);

        private static readonly Type CreatedAtType = typeof(NeoCreatedAt);

        private static readonly Type UpdatedAtType = typeof(NeoUpdatedAt);

        private static readonly Type IgnoredType = typeof(NeoIgnored);

        private static readonly Type RelationshipEntityType = typeof(NeoRelationshipEntity);

        private static readonly Type RelationshipType = typeof(NeoRelationship);

        private static readonly Neo4JException DuplicateNodeEntityKey =
            new Neo4JException("Duplicate node entity key");
        
        private static readonly Neo4JException UnsupportedNodeEntityType =
            new Neo4JException("Unsupported entity type");

        private static readonly Neo4JException UnsupportedRelationshipType =
            new Neo4JException("Unsupported relationship type");

        private static readonly Neo4JException IllegalNodeEntityException =
            new Neo4JException("Invalid neo4j node entity");

        private static readonly Neo4JException DuplicateIdException =
            new Neo4JException("Duplicate id property");

        private static readonly Neo4JException IdIsMissingException =
            new Neo4JException("Id property is missing");

        private static readonly Neo4JException IllegalIdFormatException =
            new Neo4JException("Id property is not nullable long");

        private static readonly Neo4JException IllegalCreatedAtOrUpdatedAtFormatException =
            new Neo4JException("CreatedAt or UpdatedAt property is not nullable DateTime");

        private readonly IDictionary<Type, Meta> _allTypes = new Dictionary<Type, Meta>();
        
        private readonly IDictionary<string, Meta> _allKeys = new Dictionary<string, Meta>();
        
        private Assembly Assembly { get; }

        public NeoContext(Assembly assembly)
        {
            Assembly = assembly;
            Init();
        }

        private void Init()
        {
            foreach (var t in Assembly.GetTypes())
            {
                if (!IsSupportedEntityType(t)) continue;
                
                var meta = CreateMeta(t);
                if (_allKeys.ContainsKey(meta.Key))
                {
                    throw DuplicateNodeEntityKey;
                }

                _allKeys.Add(meta.Key, meta);
                _allTypes.Add(t, meta);
            }

            foreach (var (t, m) in _allTypes)
            {
                if (!m.HasRelationshipProperty()) continue;
                ValidateRelationship(m);
            }
        }

        public Meta GetMetaData(Type t)
        {
            if (_allTypes.TryGetValue(t, out var val))
            {
                return val;
            }

            throw UnsupportedNodeEntityType;
        }

        public Meta GetMetaData(string key)
        {
            if (_allKeys.TryGetValue(key, out var val))
            {
                return val;
            }

            throw UnsupportedNodeEntityType;
        }

        private void ValidateRelationship(Meta meta)
        {
            foreach (var metaProperty in meta.RelationshipProperties)
            {
                var t = metaProperty.Info.PropertyType;
                if (metaProperty.Info.IsCollection())
                {
                    t = t.GetTypeInfo().GenericTypeArguments[0];
                }

                if (!_allTypes.ContainsKey(t))
                {
                    throw UnsupportedRelationshipType;
                }   
            }
        }

        #region Static Methods
        
        private static Meta CreateMeta(Type t)
        {
            var props = t.GetProperties();
            var entityAttribute = GetEntityAttribute(t);
            var meta = new Meta
            {
                RawType = t,
                Label = entityAttribute.Label,
                Key = entityAttribute.Key,
            };

            var hasId = false;
            foreach (var prop in props)
            {
                if (prop.HasAttribute(IgnoredType))
                {
                    continue;
                }

                if (prop.HasAttribute(NodeIdType))
                {
                    if (hasId)
                    {
                        throw DuplicateIdException;
                    }

                    if (!prop.IsNullableLong())
                    {
                        throw IllegalIdFormatException;
                    }

                    hasId = true;
                    meta.IdField = prop;
                    continue;
                }

                // relationship property
                var relationshipAtt = (NeoRelationship) prop.GetCustomAttribute(RelationshipType);
                if (relationshipAtt != null)
                {
                    var rp = new RelationshipProperty(prop, relationshipAtt);
                    meta.RelationshipProperties.Add(rp);
                    continue;
                }

                // regular property
                var property = new RegularProperty(prop)
                {
                    IsCreatedAt = prop.HasAttribute(CreatedAtType),
                    IsUpdatedAt = prop.HasAttribute(UpdatedAtType),
                };

                ValidateRegularProperty(property);
                meta.RegularProperties.Add(property);
            }

            if (!hasId)
            {
                throw IdIsMissingException;
            }

            return meta;
        }

        private static void ValidateRegularProperty(RegularProperty metaProperty)
        {
            if (metaProperty.IsCreatedAt || metaProperty.IsUpdatedAt)
            {
                if (!metaProperty.Info.IsDateTime())
                {
                    throw IllegalCreatedAtOrUpdatedAtFormatException;
                }
            }
        }
        
        private static NeoNodeEntity GetEntityAttribute(MemberInfo t)
        {
            var att = t.GetCustomAttribute(NodeEntityType);
            if (att == null)
            {
                throw IllegalNodeEntityException;
            }

            return (NeoNodeEntity) att;
        }

        private static bool IsSupportedEntityType(MemberInfo t)
        {
            return t.GetCustomAttribute(NodeEntityType) != null
                   || t.GetCustomAttribute(RelationshipEntityType) != null;
        }
        
        #endregion
    }
}