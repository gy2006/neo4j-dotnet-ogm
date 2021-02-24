using Neo4jOgm.Cypher;
using NUnit.Framework;

namespace Neo4jOgmTest
{
    public class ConverterTest
    {
        [Test]
        public void ShouldConvertLongToInt()
        {
            var c = new NumberConverter();
            Assert.AreEqual(typeof(int),c.ToValue(123L, typeof(int)).GetType());
        }
    }
}