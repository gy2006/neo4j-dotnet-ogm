# neo4j-dotnet-ogm

Neo4j Object Graph Mapping Library for .NET

## Installation

## Quick start

### Declare domain entities

```c#
[NeoNodeEntity("person", "p")]
public class Person
{
    [NeoNodeId]
    public long? Id { get; set; }
    
    public string Name { get; set; }
    
    [NeoRelationship("ARE_FRIENDS")]
    public List<Person> Friends { get; set; }
    
    [NeoCreatedAt]
    public DateTime? CreatedAt { get; set; }
    
    [NeoUpdatedAt]
    public DateTime? UpdatedAt { get; set; }
    
    [NeoIgnored]
    public string Extra { get; set; }
}
```

### Setup

```c#

// make neo4j connection
var authToken = AuthTokens.Basic("neo4j", "12345");
var driver = GraphDatabase.Driver("bolt://localhost:7687", authToken);

// create context by Assembly and load entities
var context = new NeoContext(Assembly.GetExecutingAssembly());

// new repoistory
var repository = new NeoRepository(driver, "neo4j", context);


// Create an entity with relationship
var a = new Person {Name = "A"};
var b = new Person {Name = "B"};
a.Friends = new List<Person> {b};
var created = repository.Create(a);

// Update entity
created.Name = "New name"
repository.Update(created);

// Load entity and relationship by id
var loaded = repository.FindById<Person>(created.Id.Value, new RelationshipOption(){Load = true, Depth = 5});

// Find all entities and relationship with criteria
var crteria = new Criteria("Name", Operator.Equal, "A")
        .Or(new Criteria("Name", Operator.Equal, "B"));
        
var page = repository.FindAll<Person>(new PageRequest(1, 5), crteria, new RelationshipOption {Load = true});

// Delete entity and relationship by id
repository.DeleteById<Person>(created.Id.Value, true);

// Delete all
repository.DeleteAll<Person>();

```


