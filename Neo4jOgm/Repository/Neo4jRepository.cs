using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver;
using Neo4jOgm.Cypher;
using Neo4jOgm.Domain;

namespace Neo4jOgm.Repository
{
    public class NeoRepository
    {
        private readonly IDriver _driver;

        private readonly Action<SessionConfigBuilder> _sessionBuilder;

        private readonly NeoContext _context;

        public NeoRepository(IDriver driver, string database, NeoContext context)
        {
            _driver = driver;
            _context = context;
            _sessionBuilder = o => o.WithDatabase(database);
        }

        public T Create<T>(T entity)
        {
            var create = CypherTranslator.ToCreate(_context, entity);

            using var session = NewSession();
            return session.WriteTransaction(tx =>
            {
                var result = tx.Run(create.Query.ToString()).Single();
                foreach (var (key, value) in result.Values)
                {
                    var iNode = value.As<INode>();
                    var e = create.FlattedEntities[key];
                    var eMeta = _context.GetMetaData(e.GetType());
                    eMeta.SetId(e, iNode.Id);
                }

                return entity;
            });
        }

        public T Update<T>(T entity)
        {
            var meta = _context.GetMetaData(typeof(T));

            if (meta.GetId(entity) == null)
            {
                throw new Neo4JException("Id is required on update");
            }

            meta.GetUpdatedAtProperty()?.SetValue(entity, DateTime.UtcNow);

            var cypher = CypherTranslator.ToUpdate(meta, entity);
            using var session = NewSession();
            session.WriteTransaction(tx => tx.Run(cypher));
            return entity;
        }

        public void DeleteById<T>(long id, bool isDetach = false)
        {
            var meta = _context.GetMetaData(typeof(T));
            var cypher = CypherTranslator.ToDelete(meta, id, isDetach);
            using var session = NewSession();
            session.WriteTransaction(tx => tx.Run(cypher));
        }

        public T FindById<T>(long id, RelationshipOption option = null)
        {
            var meta = _context.GetMetaData(typeof(T));
            using var session = NewSession();
            return session.ReadTransaction(tx =>
            {
                var nodeRecord = tx.Run(CypherTranslator.FindNodeById(meta, id)).SingleOrDefault();
                if (nodeRecord == null)
                {
                    return default;
                }

                var entity = ToEntity(nodeRecord, meta);
                if (ShouldLoadRelationship(option))
                {
                    FetchRelationshipByNodeId(meta, entity, tx);
                }

                return entity.As<T>();
            });
        }

        public Page<T> FindAll<T>(PageRequest r, Criteria criteria = null, RelationshipOption rOption = null)
        {
            var meta = _context.GetMetaData(typeof(T));
            SetDefaultSortIfNotExist(r, meta);

            var query = CypherTranslator.FindAll(meta, r, criteria);
            using var session = NewSession();
            return session.ReadTransaction(tx =>
            {
                var records = tx.Run(query.Items).ToList();
                var total = tx.Run(query.Count).Single()[0].As<long>();
                var loadRelationship = ShouldLoadRelationship(rOption);

                var items = ToEntityList(records, meta, (iNode, entity) =>
                {
                    if (!loadRelationship) return;
                    FetchRelationshipByNodeId(meta, entity, tx);
                }).As<IList<T>>();

                return new Page<T>(r)
                {
                    Items = items,
                    TotalItems = total
                };
            });
        }

        public void DeleteAll<T>()
        {
            var meta = _context.GetMetaData(typeof(T));
            using var session = NewSession();
            session.WriteTransaction(tx => tx.Run(CypherTranslator.ToDeleteAll(meta)));
        }

        private ISession NewSession()
        {
            return _driver.Session(_sessionBuilder);
        }

        private void FetchRelationshipByNodeId(Meta meta, object entity, ITransaction tx)
        {
            var id = meta.GetId(entity).As<long>();

            foreach (var rp in meta.RelationshipProperties)
            {
                var rMeta = _context.GetMetaData(rp.EntityType);
                var iRecords = tx.Run(CypherTranslator.FindRelationshipNodesById(meta, id, rp)).ToList();
                if (iRecords.Count == 0) continue;
                
                if (rp.IsCollection)
                {
                    var listVal = ToEntityList(iRecords, rMeta);
                    rp.SetValue(entity, listVal);
                    continue;
                }
                
                rp.SetValue(entity, ToEntity(iRecords[0], rMeta));
            }
        }

        private static bool ShouldLoadRelationship(RelationshipOption o)
        {
            return o != null && o.Load;
        }

        private static IList ToEntityList(IReadOnlyCollection<IRecord> records, Meta meta,
            Action<INode, object> onItem = null)
        {
            var listType = typeof(List<>);
            var list = Activator.CreateInstance(listType.MakeGenericType(meta.RawType)).As<IList>();

            foreach (var iRecord in records)
            {
                if (iRecord == null)
                {
                    continue;
                }

                var iNode = iRecord[0].As<INode>();
                var entity = CreateInstance(iNode, meta);
                onItem?.Invoke(iNode, entity);
                list.Add(entity);
            }

            return list;
        }

        private static object ToEntity(IRecord record, Meta meta)
        {
            var iNode = record[0].As<INode>();
            return CreateInstance(iNode, meta);
        }

        private static object CreateInstance(INode iNode, Meta meta)
        {
            var instance = Activator.CreateInstance(meta.RawType);
            meta.SetId(instance, iNode.Id);

            foreach (var prop in meta.RegularProperties)
            {
                if (iNode.Properties.TryGetValue(prop.GetName(), out var neoVal))
                {
                    var objectValue = ConverterHelper.ToObjectValue(neoVal, prop.Info.PropertyType);
                    prop.SetValue(instance, objectValue);
                }
            }

            return instance;
        }

        private static void SetDefaultSortIfNotExist(PageRequest r, Meta meta)
        {
            if (r.Sort.HasOrders())
            {
                return;
            }

            var createdAtProperty = meta.GetCreatedAtProperty();
            if (createdAtProperty == null)
            {
                return;
            }

            r.Sort.Orders.Add(new Order
            {
                Property = createdAtProperty.GetName()
            });
        }
    }
}