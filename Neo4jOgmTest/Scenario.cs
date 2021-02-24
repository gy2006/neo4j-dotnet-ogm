using System;
using System.Collections.Generic;
using Neo4jOgm.Attribute;

namespace Neo4jOgmTest
{
    [NeoNodeEntity("person", "p")]
    public class PersonForTest
    {
        [NeoNodeId]
        public long? Id { get; set; }
        
        public string Name { get; set; }
        
        [NeoRelationship("HAS_ADDRESS")]
        public List<AddressForTest> Addresses { get; set; }
        
        [NeoRelationship("TEACH_FOR")]
        public StudentForTest Student { get; set; }
        
        [NeoCreatedAt]
        public DateTime? CreatedAt { get; set; }
        
        [NeoUpdatedAt]
        public DateTime? UpdatedAt { get; set; }
        
        [NeoIgnored]
        public string Extra { get; set; }
    }

    [NeoNodeEntity("address", "a")]
    public class AddressForTest
    {
        [NeoNodeId]
        public long? Id { get; set; }
        
        public string City { get; set; }
        
        public int Postcode { get; set; }
        
        [NeoCreatedAt]
        public DateTime? CreatedAt { get; set; }
        
        [NeoUpdatedAt]
        public DateTime? UpdatedAt { get; set; }
    }

    [NeoNodeEntity("student", "s")]
    public class StudentForTest
    {
        [NeoNodeId]
        public long? Id { get; set; }
        
        public string Name { get; set; }
    }
}