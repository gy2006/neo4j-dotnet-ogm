using System.Text;

namespace Neo4jOgm.Extension
{
    public static class StringBuilderExtension
    {
        public static void TryRemoveLastCharacter(this StringBuilder builder, char lastCharToRemove) 
        {
            if (builder.Length == 0)
            {
                return;
            }

            if (builder[^1] == lastCharToRemove)
            {
                builder.Remove(builder.Length - 1, 1);
            }
        }
    }
}