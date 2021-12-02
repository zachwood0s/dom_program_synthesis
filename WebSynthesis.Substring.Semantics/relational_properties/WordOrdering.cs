using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WebSynthesis.RelationalProperties;
using WebSynthesis.TreeManipulation;

namespace WebSynthesis.Substring.RelationalProperties
{
    //[RelationalProperty("Word Ordering", typeof(ProseHtmlNode))]
    public class WordOrdering : IRelationalProperty
    {
        public string Name => "Word Ordering";
        public Type Type => typeof(ProseHtmlNode);
        private const int MaxReorderCount = 2;
        private static Random random = new Random();
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            for (var i = 0; i < MaxReorderCount; i++)
            {
                var newTree = node.DeepCopy();
                newTree.Traverse(x => PermuteWords(x));
                if (!newTree.Equals(node))
                    yield return Tuple.Create<object, object>(newTree, output);
            }
        }

        private void PermuteWords(ProseHtmlNode input)
        {
            if (input.Text == null || !input.Text.Contains(" ")) return;

            List<string> words = input.Text.Split(" ").ToList();
            int l = words.Count;
            string ret = input.Text;

            while (ret == input.Text)
            {
                ret = "";
                List<string> newWords = new List<string>(words);
                for (int i = 0; i < words.Count; i++)
                {
                    int j = random.Next(0, newWords.Count - 1);
                    ret += newWords[j] + " ";
                    newWords.Remove(newWords[j]);
                }
                ret = ret.Substring(0, ret.Length - 1);
            }

            input.Text = ret;
        }
    }
}
