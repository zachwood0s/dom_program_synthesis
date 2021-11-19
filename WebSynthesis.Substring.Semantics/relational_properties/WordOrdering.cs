using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WebSynthesis.RelationalProperties;
using WebSynthesis.TreeManipulation;

namespace WebSynthesis.Substring.RelationalProperties
{
    [RelationalProperty("Word Ordering", typeof(ProseHtmlNode))]
    public class WordOrdering : IRelationalProperty
    {
        public string Name => "Word Ordering";
        public Type Type => typeof(ProseHtmlNode);
        private const int MaxReorderCount = 10;
        private static Random random = new Random();
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            var outstrs = output as List<string>;
            for (var i = 0; i < MaxReorderCount; i++)
            {
                var newTree = node.DeepCopy();
                foreach (string o in outstrs)
                    newTree.Traverse(x => PermuteWords(x, o));
                if (!newTree.Equals(node))
                    yield return Tuple.Create<object, object>(newTree, output);
            }
        }

        private void PermuteWords(ProseHtmlNode input, string output)
        {
            if (input.Text == null || !input.Text.Contains(output) || input.Text == output || !input.Text.Contains(" ")) return;

            List<string> words = input.Text.Split(" ").ToList();
            int l = words.Count;
            string ret = "";

            for(int i = 0; i < l; i++)
            {
                int j = random.Next(0, words.Count - 1);
                ret += words[j] + " ";
                words.Remove(words[j]);
            }

            input.Text = ret.Substring(0, ret.Length - 1); ;
        }
    }
}
