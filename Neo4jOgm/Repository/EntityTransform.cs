using System;
using System.Collections;
using System.Collections.Generic;
using Neo4j.Driver;
using Neo4jOgm.Cypher;
using Neo4jOgm.Domain;

namespace Neo4jOgm.Repository
{
    public abstract class EntityTransform
    {
        public static object NewInstance(IRecord record, Meta meta, int index = 0)
        {
            var iNode = record[index].As<INode>();
            return NewInstance(iNode, meta);
        }

        public static object NewInstance(INode iNode, Meta meta)
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

        public static IList NewInstanceList(
            IReadOnlyCollection<IRecord> records, Meta meta, Func<IRecord, object> onCreateEntity = null)
        {
            var listType = typeof(List<>);
            var list = Activator.CreateInstance(listType.MakeGenericType(meta.RawType)).As<IList>();

            foreach (var iRecord in records)
            {
                if (iRecord == null)
                {
                    continue;
                }

                var entity = onCreateEntity == null ? NewInstance(iRecord, meta) : onCreateEntity.Invoke(iRecord);
                list.Add(entity);
            }

            return list;
        }
    }
}