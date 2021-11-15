using System.IO;
using System.Reflection;

namespace WebSynthesis.Substring
{
    public static class GrammarText
    {
        public static string Get()
        {
            var assembly = typeof(GrammarText).GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream("WebSynthesis.Substring.WebSynthesis.Substring.grammar"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
