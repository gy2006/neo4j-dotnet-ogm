namespace Neo4jOgm.Domain
{
    public class RelationshipOption
    {
        public bool Load { get; set; }

        public int MinHops { get; set; }
        
        public int MaxHops { get; set; }
    }
}