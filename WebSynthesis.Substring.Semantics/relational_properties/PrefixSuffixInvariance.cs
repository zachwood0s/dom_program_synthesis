using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSynthesis.RelationalProperties;
using WebSynthesis.TreeManipulation;

namespace WebSynthesis.Substring.RelationalProperties
{
    [RelationalProperty("Substring Prefix Invariant", typeof(ProseHtmlNode))]
    public class PrefixInvariance : IRelationalProperty
    {
        public string Name => "Substring Prefix Invariant";
        public Type Type => typeof(ProseHtmlNode);
        private const int MaxReorderCount = 10;
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            var outstrs = output as List<string>;
            for (var i = 0; i < MaxReorderCount; i++)
            {
                var newTree = node.DeepCopy();
                foreach (string o in outstrs)
                    newTree.Traverse(x => PermutePrefix(x, o));
                if (!newTree.Equals(node))
                    yield return Tuple.Create<object, object>(newTree, output);
            }
        }

        private void PermutePrefix(ProseHtmlNode input, string output)
        {
            if (input.Text == null || !input.Text.Contains(output) || input.Text == output) return;

            int index = input.Text.IndexOf(output);
            string subStr = input.Text.Substring(0, index);

            input.Text = PermuteString.Random(subStr) + input.Text.Substring(index, input.Text.Length - index);
        }
    }

    [RelationalProperty("Substring Suffix Invariant", typeof(ProseHtmlNode))]
    public class SuffixInvariance : IRelationalProperty
    {
        public string Name => "Substring Suffix Invariant";
        public Type Type => typeof(ProseHtmlNode);
        private const int MaxReorderCount = 10;
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            var outstrs = output as List<string>;
            for (var i = 0; i < MaxReorderCount; i++)
            {
                var newTree = node.DeepCopy();
                foreach (string o in outstrs)
                    newTree.Traverse(x => PermuteSuffix(x, o));
                if (!newTree.Equals(node))
                    yield return Tuple.Create<object, object>(newTree, output);
            }
        }

        private void PermuteSuffix(ProseHtmlNode input, string output)
        {
            if (input.Text == null || !input.Text.Contains(output) || input.Text == output) return;

            int index = input.Text.IndexOf(output) + output.Length - 1;
            string subStr = input.Text.Substring(index, input.Text.Length - index);

            input.Text = input.Text.Substring(0, index) + PermuteString.Random(subStr);
        }
    }

    public static class PermuteString
    {
        private static Random random = new Random();
        public static string Random(string str)
        {
            string ret = "";
            for (int i = 0; i < str.Length; i++)
                ret += (char) random.Next(1, 127);
            return ret;
        }
    }
}