using System.IO;
using System.Reflection;

namespace WebSynthesis.Substring
{
    public static class GrammarText
    {
        public static string Get()
        {
            var assembly = typeof(GrammarText).GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream("SubstringSynthesis.substring.grammar"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
