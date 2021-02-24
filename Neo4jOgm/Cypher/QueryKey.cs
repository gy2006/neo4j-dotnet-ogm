namespace Neo4jOgm.Cypher
{
    public class NodeKey
    {
        public NodeKey Parent { get; private init; }
        
        public string Key { get; }

        public int Index { get; }

        private NodeKey(string key, int index)
        {
            Key = key;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Key}_{Index}";
        }

        public static string Create(string key, int index = 0, string related = null)
        {
            var current = new NodeKey(key, index).ToString();
            return related == null ? current : $"{related}_{current}";
        }

        /**
         * Parse query key to NodeKey instance
         * ex: p_0_a_0, p_0 is parent NodeKey, a_0 is current 
         */
        public static NodeKey Parse(string str)
        {
            var strings = str.Split('_');

            NodeKey parent = null;
            NodeKey current = null;
            for (var i = 0; i < strings.Length; i += 2)
            {
                current = new NodeKey(strings[i], int.Parse(strings[i + 1])) {Parent = parent};
                parent = current;
            }

            return current;
        }
    }
}