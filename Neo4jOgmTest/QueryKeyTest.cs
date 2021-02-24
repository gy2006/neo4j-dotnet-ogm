using Neo4jOgm.Cypher;
using NUnit.Framework;

namespace Neo4jOgmTest
{
    public class QueryKeyTest
    {
        [Test]
        public void ShouldCreateKeyForNode()
        {
            Assert.AreEqual("p_0", NodeKey.Create("p"));
            Assert.AreEqual("p_1", NodeKey.Create("p", 1));
            Assert.AreEqual("p_0_a_1", NodeKey.Create("a", 1, "p_0"));
        }

        [Test]
        public void ShouldParseNodeKey()
        {
            var current = NodeKey.Parse("p_2_a_1_b_3");
            Assert.AreEqual("b", current.Key);
            Assert.AreEqual(3, current.Index);
            
            Assert.AreEqual("a", current.Parent.Key);
            Assert.AreEqual(1, current.Parent.Index);
            
            Assert.AreEqual("p", current.Parent.Parent.Key);
            Assert.AreEqual(2, current.Parent.Parent.Index);
        }
    }
}