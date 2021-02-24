using System;

namespace Neo4jOgm
{
    public class Neo4JException : Exception
    {
        public Neo4JException(string message) : base(message)
        {
        }
    }
}