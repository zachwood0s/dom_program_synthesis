using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSynthesis.RelationalProperties;
using WebSynthesis.TreeManipulation;

namespace WebSynthesis.Substring.RelationalProperties
{
    [RelationalProperty("Prefix Substring Invariant", typeof(ProseHtmlNode))]
    public class PrefixInvariance : IRelationalProperty
    {
        public string Name => "Prefix Substring Invariant";
        public Type Type => typeof(ProseHtmlNode);
        private const int MaxReorderCount = 10;
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            var outStr = output as string;
            for (var i = 0; i < MaxReorderCount; i++)
            {
                var newTree = node.DeepCopy();
                newTree.Traverse(x => x.Text = PermutePrefix(x.Text, outStr));
                yield return Tuple.Create<object, object>(newTree, output);
            }
        }

        private string PermutePrefix(string input, string output)
        {
            if (!input.Contains(output)) return input;

            int index = input.IndexOf(output);
            string subStr = input.Substring(0, index);

            return PermuteString.Random(subStr) + input.Substring(index, input.Length - index);
        }
    }

    [RelationalProperty("Suffix Substring Invariant", typeof(ProseHtmlNode))]
    public class SuffixInvariance : IRelationalProperty
    {
        public string Name => "Suffix Substring Invariant";
        public Type Type => typeof(ProseHtmlNode);
        private const int MaxReorderCount = 10;
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            var outStr = output as string;
            for (var i = 0; i < MaxReorderCount; i++)
            {
                var newTree = node.DeepCopy();
                newTree.Traverse(x => x.Text = PermuteSuffix(x.Text, outStr));
                yield return Tuple.Create<object, object>(newTree, output);
            }
        }

        private string PermuteSuffix(string input, string output)
        {
            if (!input.Contains(output)) return input;

            int index = input.IndexOf(output) + output.Length - 1;
            string subStr = input.Substring(index, input.Length - index);

            return input.Substring(0, index) + PermuteString.Random(subStr);
        }
    }

    public static class PermuteString
    {
        private static Random random = new Random();
        public static string Random(string str)
        {
            string ret = "";
            for (int i = 0; i < str.Length; i++)
                ret += (char) random.Next(0, 127);
            return ret;
        }
    }
}