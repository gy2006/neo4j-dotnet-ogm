using System;
using System.Collections.Generic;
using Neo4jOgm.Attribute;

namespace Neo4jOgmTest
{
    [NeoNodeEntity("person", "p")]
    public class Person
    {
        [NeoNodeId]
        public long? Id { get; set; }
        
        public string Name { get; set; }
        
        [NeoRelationship("HAS_ADDRESS")]
        public List<Address> Addresses { get; set; }
        
        [NeoRelationship("TEACH_FOR")]
        public Student Student { get; set; }
        
        [NeoRelationship("ARE_FRIENDS")]
        public List<Person> Friends { get; set; }
        
        [NeoCreatedAt]
        public DateTime? CreatedAt { get; set; }
        
        [NeoUpdatedAt]
        public DateTime? UpdatedAt { get; set; }
        
        [NeoIgnored]
        public string Extra { get; set; }
    }

    [NeoNodeEntity("address", "a")]
    public class Address
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
    public class Student
    {
        [NeoNodeId]
        public long? Id { get; set; }
        
        public string Name { get; set; }
    }
}