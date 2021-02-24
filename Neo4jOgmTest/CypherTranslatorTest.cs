using Neo4jOgm.Cypher;
using Neo4jOgm.Domain;
using NUnit.Framework;

namespace Neo4jOgmTest
{
    public class CypherTranslatorTest
    {
        [Test]
        public void ShouldTranslateSingleWhereClause()
        {
            const string target = "WHERE n.name = 'Peter'";
            var c = new Criteria("name", Operator.Equal, "Peter");
            Assert.AreEqual(target, CypherTranslator.ToWhereClause(c, "n"));
        }

        [Test]
        public void ShouldTranslateComplexWhereClause()
        {
            const string target =
                "WHERE n.name = 'Peter' XOR (n.age < 30 AND n.name = 'Timothy') OR NOT (n.name = 'Timothy' OR n.name = 'Peter')";

            var c = new Criteria("name", Operator.Equal, "Peter");

            c.Xor(new Criteria("age", Operator.Lt, 30)
                .And(new Criteria("name", Operator.Equal, "Timothy")));

            c.OrNot(new Criteria("name", Operator.Equal, "Timothy")
                .Or(new Criteria("name", Operator.Equal, "Peter")));

            var whereClause = CypherTranslator.ToWhereClause(c, "n");
            Assert.AreEqual(target, whereClause);
        }

        [Test]
        public void ShouldTranslateWhereClauseWithEmptyRoot()
        {
            const string target = "WHERE (n.name = 'Peter' OR p.release >= 20) XOR ID(a) = 100";

            var c = new Criteria();

            c.Add(new Criteria("n", "name", Operator.Equal, "Peter")
                .Or(new Criteria("p", "release", Operator.Gte, 20)));

            c.Xor(new Criteria("a", Criteria.ID, Operator.Equal, 100));

            var whereClause = CypherTranslator.ToWhereClause(c, "n");
            Assert.AreEqual(target, whereClause);
        }
    }
}