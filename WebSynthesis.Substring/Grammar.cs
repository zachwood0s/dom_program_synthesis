using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace WebSynthesis.Substring
{
    public static class GrammarText
    {
        public static string Get()
        {
            var assembly = typeof(GrammarText).GetTypeInfo().Assembly;            
            using (var stream = assembly.GetManifestResourceStream("WebSynthesis.TestGrammar.WebSynthesis.TestGrammar.grammar"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
