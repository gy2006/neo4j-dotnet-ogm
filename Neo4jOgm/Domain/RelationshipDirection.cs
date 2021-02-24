using System.ComponentModel;

namespace Neo4jOgm.Domain
{
    public enum RelationshipDirection
    {
        [Description("->")] Out,

        [Description("<-")] In,
    }
}