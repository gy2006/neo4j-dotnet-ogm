using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Neo4jOgm.Domain;
using Neo4jOgm.Extension;

[assembly: InternalsVisibleTo("Neo4jOgmTest")]

namespace Neo4jOgm.Cypher
{
    public abstract class CypherTranslator
    {
        #region Create

        internal static CreateQueryReturn ToCreate(NeoContext context, object entity)
        {
            var meta = context.GetMetaData(entity.GetType());

            var createQuery = ToCreateNode(context, entity, NodeKey.Create(meta.Key), new Dictionary<object, string>());
            createQuery.Query.Append("RETURN ");
            foreach (var key in createQuery.FlattedEntities.Keys)
            {
                createQuery.Query.Append($"{key},");
            }

            createQuery.Query.TryRemoveLastCharacter(',');
            return createQuery;
        }

        private static CreateQueryReturn ToCreateNode(NeoContext context, object entity, string queryKey, IDictionary<object, string> entities)
        {
            entities.Add(entity, queryKey);
            var meta = context.GetMetaData(entity.GetType());

            meta.GetCreatedAtProperty()?.SetValue(entity, DateTime.UtcNow);
            meta.GetUpdatedAtProperty()?.SetValue(entity, DateTime.UtcNow);

            var properties = BuildPropertyClause(meta, entity);

            var r = new CreateQueryReturn();
            r.FlattedEntities.Add(queryKey, entity);
            r.Query.AppendLine($"CREATE ({queryKey}:{meta.Label} {properties})");

            var rIndex = 0;
            foreach (var p in meta.RelationshipProperties)
            {
                var rMeta = context.GetMetaData(p.EntityType);
                var rEntity = p.GetValue(entity);
                if (rEntity == null) continue;

                var rCollection = p.IsCollection ? (ICollection) rEntity : new[] {rEntity};
                foreach (var item in rCollection)
                {
                    if (entities.TryGetValue(item, out var existKey))
                    {
                        r.Query.AppendLine(ToRelationshipCreate(p, queryKey, existKey));
                        continue;
                    }
                    
                    var rNodeKey = NodeKey.Create(rMeta.Key, rIndex++, queryKey);
                    r.Append(ToCreateNode(context, item, rNodeKey, entities));
                    r.Query.AppendLine(ToRelationshipCreate(p, queryKey, rNodeKey));
                }
            }

            return r;
        }

        #endregion

        #region Update

        internal static string ToUpdate(Meta meta, object entity)
        {
            var properties = BuildPropertyClause(meta, entity);
            var id = meta.GetId(entity);
            var k = meta.Key;
            return $"MATCH ({k}:{meta.Label}) WHERE ID({k}) = {id} SET {k} = {properties} RETURN {k}";
        }

        #endregion

        #region Relationship

        /**
         * MATCH (p:person)-[:HAS_ADDRESS]->(a) WHERE ID(p) = 4 return a
         */
        internal static string FindRelationshipNodesById(Meta meta, long id, RelationshipProperty rp)
        {
            var rel = rp.Direction == RelationshipDirection.Out
                ? $"-[:{rp.RelationshipType}]->"
                : $"<-[:{rp.RelationshipType}]-";
            return $"MATCH ({meta.Key}:{meta.Label}){rel}(list) WHERE ID({meta.Key}) = {id} return list";
        }

        private static string ToRelationshipCreate(RelationshipProperty p, string leftKey, string rightKey)
        {
            return p.Direction == RelationshipDirection.Out
                ? $"CREATE ({leftKey})-[:{p.RelationshipType}]->({rightKey})"
                : $"CREATE ({leftKey})<-[:{p.RelationshipType}]-({rightKey})";
        }

        #endregion

        #region Delete

        internal static string ToDelete(Meta meta, long id, bool isDetach)
        {
            var deleteToken = isDetach ? "DETACH DELETE" : "DELETE";
            var k = meta.Key;
            return $"MATCH ({k}:{meta.Label}) WHERE ID({k}) = {id} {deleteToken} {k}";
        }

        internal static string ToDeleteAll(Meta meta)
        {
            return $"MATCH ({meta.Key}:{meta.Label}) DETACH DELETE {meta.Key}";
        }

        #endregion

        #region Find

        public class PagingQuery
        {
            public string Items { get; set; }

            public string Count { get; set; }
        }

        internal static string FindNodeById(Meta meta, long id)
        {
            return $"MATCH ({meta.Key}:{meta.Label}) WHERE ID({meta.Key}) = {id} RETURN {meta.Key}";
        }

        internal static PagingQuery FindAll(Meta meta, PageRequest pr, Criteria criteria)
        {
            var where = ToWhereClause(criteria, meta.Key);
            var paging = ToPagingClause(pr, meta.Key);

            return new PagingQuery
            {
                Items = $"MATCH ({meta.Key}:{meta.Label}) {where} RETURN {meta.Key} {paging}",
                Count = $"MATCH ({meta.Key}:{meta.Label}) {where} RETURN COUNT(*)"
            };
        }

        private static string ToPagingClause(PageRequest r, string queryKey)
        {
            if (!r.Sort.HasOrders())
            {
                return $"SKIP {r.Offset()} LIMIT {r.Size}";
            }

            var builder = new StringBuilder();
            builder.Append("ORDER BY ");
            foreach (var order in r.Sort.Orders)
            {
                builder.AppendFormat("{0}.{1} {2},", queryKey, order.Property, order.Direction);
            }

            builder.Remove(builder.Length - 1, 1);
            return $"{builder} SKIP {r.Offset()} LIMIT {r.Size}";
        }

        internal static string ToWhereClause(Criteria criteria, string defaultQueryKey)
        {
            if (criteria == null || criteria.Chain.Count == 0)
            {
                return string.Empty;
            }

            var whereContent = GetClauseFromChain(criteria, defaultQueryKey);
            if (whereContent.StartsWith('(') && whereContent.EndsWith(')'))
            {
                return $"WHERE {whereContent.Substring(1, whereContent.Length - 2).Trim()}";
            }

            return $"WHERE {whereContent.Trim()}";
        }

        private static string GetClauseFromChain(Criteria criteria, string defaultQueryKey)
        {
            var builder = new StringBuilder();
            builder.Append(GetSingleClause(criteria, defaultQueryKey));

            foreach (var sub in criteria.Chain)
            {
                if (sub == criteria)
                {
                    continue;
                }

                if (sub is BooleanOperator o)
                {
                    builder.AppendFormat(" {0}", GetSingleClause(o));
                    continue;
                }

                if (sub is Criteria c)
                {
                    builder.AppendFormat(" {0}", GetClauseFromChain(c, defaultQueryKey));
                }
            }

            if (criteria.Chain.Count > 1)
            {
                builder.Insert(0, '(');
                builder.Append(')');
            }

            return builder.ToString();
        }

        private static string GetSingleClause(BooleanOperator o)
        {
            return o.Operator;
        }

        private static string GetSingleClause(Criteria c, string defaultQueryKey)
        {
            if (c.IsEmpty)
            {
                return string.Empty;
            }

            if (c.Value == null)
            {
                throw new Neo4JException("Value of criteria is null");
            }

            c.QueryKey ??= defaultQueryKey;
            var valStr = ConverterHelper.GetQueryString(c.Value);

            if (c.Property.Equals(Criteria.ID))
            {
                return $"{c.Property}({c.QueryKey}) {c.Operator.GetSymbol()} {valStr}";
            }

            return $"{c.QueryKey}.{c.Property} {c.Operator.GetSymbol()} {valStr}";
        }

        #endregion

        #region Properties

        /**
         * Build regular properties clause like:
         * {name:'name', release:20}
         */
        private static string BuildPropertyClause<T>(Meta meta, T entity)
        {
            var builder = new StringBuilder();
            builder.Append('{');

            foreach (var prop in meta.RegularProperties)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                {
                    builder.AppendFormat("{0}:{1},", prop.GetName(), ConverterHelper.GetQueryString(value));
                }
            }

            builder.Remove(builder.Length - 1, 1);
            builder.Append('}');
            return builder.ToString();
        }

        #endregion
    }
}