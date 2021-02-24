using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Neo4j.Driver;
using Neo4jOgm;
using Neo4jOgm.Domain;
using Neo4jOgm.Repository;
using NUnit.Framework;

namespace Neo4jOgmTest
{
    public class RepositoryTest
    {
        private NeoRepository _repository;

        [OneTimeSetUp]
        public void CreateRepo()
        {
            var context = new NeoContext(Assembly.GetExecutingAssembly());
            var authToken = AuthTokens.Basic("neo4j", "12345");
            var driver = GraphDatabase.Driver("bolt://localhost:7687", authToken);

            _repository = new NeoRepository(driver, "neo4j", context);
        }

        [TearDown]
        public void CleanUp()
        {
            _repository.DeleteAll<Person>();
            _repository.DeleteAll<Address>();
            _repository.DeleteAll<Student>();
        }

        [Test]
        public void ShouldCreateUpdateAndDeleteNode()
        {
            var p = new Person
            {
                Name = "my name",
                Extra = "extra info"
            };

            // Create 
            var created = _repository.Create(p);
            Assert.NotNull(created);
            Assert.AreSame(p, created);
            Assert.NotNull(created.Id);
            Assert.NotNull(created.CreatedAt);
            Assert.NotNull(created.UpdatedAt);

            var loaded = _repository.FindById<Person>(p.Id.Value);
            Assert.NotNull(loaded);
            Assert.AreEqual(p.Id, loaded.Id);
            Assert.AreEqual(p.Name, loaded.Name);
            Assert.NotNull(loaded.CreatedAt);
            Assert.NotNull(loaded.UpdatedAt);

            // Update 
            p.Name = "System";
            _repository.Update(p);

            loaded = _repository.FindById<Person>(p.Id.Value);
            Assert.AreEqual("System", loaded.Name);

            // Delete
            _repository.DeleteById<Person>(p.Id.Value, true);
            Assert.Null(_repository.FindById<Person>(p.Id.Value));
        }

        [Test]
        public void ShouldReturnNullIfIdNotExist()
        {
            Assert.Null(_repository.FindById<Person>(10000));
        }

        [Test]
        public void ShouldListEntityWithPageAndFindByCriteria()
        {
            for (var i = 0; i < 5; i++)
            {
                _repository.Create(new Person
                {
                    Name = "my name " + i
                });

                Thread.Sleep(1000);
            }

            var page = _repository.FindAll<Person>(new PageRequest(1, 5));
            Assert.NotNull(page.Items);
            Assert.AreEqual(5, page.Items.Count);
            Assert.AreEqual(5, page.TotalItems);
            Assert.AreEqual(1, page.CurrentPage);
            Assert.AreEqual(1, page.TotalPages);

            page = _repository.FindAll<Person>(new PageRequest(1, 2));
            Assert.NotNull(page.Items);
            Assert.AreEqual(2, page.Items.Count);
            Assert.AreEqual(5, page.TotalItems);
            Assert.AreEqual(1, page.CurrentPage);
            Assert.AreEqual(3, page.TotalPages);

            page = _repository.FindAll<Person>(new PageRequest(2, 2));
            Assert.NotNull(page.Items);
            Assert.AreEqual(2, page.Items.Count);
            Assert.AreEqual(5, page.TotalItems);
            Assert.AreEqual(2, page.CurrentPage);
            Assert.AreEqual(3, page.TotalPages);


            var c = new Criteria("Name", Operator.Equal, "my name 0")
                .Or(new Criteria("Name", Operator.Equal, "my name 1"));
            page = _repository.FindAll<Person>(new PageRequest(1, 10), c);
            Assert.NotNull(page);
            Assert.AreEqual(2, page.Items.Count);
            Assert.AreEqual(2, page.TotalItems);
            Assert.AreEqual(1, page.TotalPages);
            Assert.AreEqual(1, page.CurrentPage);

            c = new Criteria("Name", Operator.Equal, "not exit 1")
                .Or(new Criteria("Name", Operator.Equal, "not exit 2"));
            page = _repository.FindAll<Person>(new PageRequest(1, 10), c);
            Assert.NotNull(page);
            Assert.AreEqual(0, page.Items.Count);
            Assert.AreEqual(0, page.TotalItems);
            Assert.AreEqual(1, page.TotalPages);
            Assert.AreEqual(1, page.CurrentPage);
        }

        [Test]
        public void ShouldCreateEntityWithRelationship()
        {
            var p = new Person
            {
                Name = "my name",
                Addresses = new List<Address>
                {
                    new()
                    {
                        City = "Beijing",
                        Postcode = 10010
                    },
                    new()
                    {
                        City = "Shanghai",
                        Postcode = 20011
                    }
                },
                Student = new Student
                {
                    Name = "Hello World"
                },
                Extra = "extra info"
            };

            var verify = new Action<Person>(obj =>
            {
                Assert.NotNull(obj);
                Assert.NotNull(obj.Id);
                Assert.NotNull(obj.UpdatedAt);
                Assert.NotNull(obj.CreatedAt);

                Assert.NotNull(obj.Addresses);
                Assert.AreEqual(2, obj.Addresses.Count);

                Assert.NotNull(obj.Addresses[0].Id);
                Assert.NotNull(obj.Addresses[0].CreatedAt);
                Assert.NotNull(obj.Addresses[0].UpdatedAt);

                Assert.NotNull(obj.Addresses[1].Id);
                Assert.NotNull(obj.Addresses[1].CreatedAt);
                Assert.NotNull(obj.Addresses[1].UpdatedAt);

                Assert.NotNull(obj.Student);
                Assert.NotNull(obj.Student.Id);
            });

            // verify created entity and relationships
            var created = _repository.Create(p);
            verify(created);

            // verify loaded entity and relationships
            var loaded = _repository.FindById<Person>(created.Id.Value, new RelationshipOption {Load = true});
            verify(loaded);

            // verify load entity and relationship with paging
            var all = _repository.FindAll<Person>(new PageRequest(1, 100), null, new RelationshipOption {Load = true});
            Assert.NotNull(all);
            Assert.AreEqual(1, all.TotalItems);
            verify(all.Items[0]);
        }

        [Test]
        public void ShouldCreateAndLoadEntityWithCycleRelationship()
        {
            var a = new Person {Name = "A"};
            var b = new Person {Name = "B"};
            var c = new Person {Name = "C"};
            var d = new Person {Name = "D"};

            a.Friends = new List<Person> {b};
            b.Friends = new List<Person> {c, d};
            c.Friends = new List<Person> {a};
            d.Friends = new List<Person> {a};

            var created = _repository.Create(a);
            Assert.NotNull(created);

            var A = _repository.FindById<Person>(created.Id.Value, new RelationshipOption(){Load = true, Depth = 5});
            Assert.NotNull(A);
            Assert.NotNull(A.Id);
            Assert.AreEqual("A", A.Name);
            Assert.AreEqual(1, A.Friends.Count);

            var B = A.Friends[0];
            Assert.NotNull(B.Id);
            Assert.AreEqual("B", B.Name);
            Assert.AreEqual(2, B.Friends.Count);

            var C = B.Friends.Find(f => f.Name == "C");
            Assert.NotNull(C);
            Assert.NotNull(C.Id);
            Assert.AreEqual(1, C.Friends.Count);
            Assert.AreSame(A, C.Friends[0]);
            
            var D = B.Friends.Find(f => f.Name == "D");
            Assert.NotNull(D);
            Assert.NotNull(D.Id);
            Assert.AreEqual(1, D.Friends.Count);
            Assert.AreSame(A, D.Friends[0]);
        }
    }
}