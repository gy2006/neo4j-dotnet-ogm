namespace Neo4jOgm.Domain
{
    public class RelationshipOption
    {
        public bool Load { get; set; }

        public int Depth { get; set; } = 1;
    }
}