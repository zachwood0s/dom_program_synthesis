using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSynthesis.RelationalProperties;

namespace WebSynthesis.TreeManipulation.RelationalProperties
{
    [RelationalProperty("Exact Match", typeof(ProseHtmlNode))]
    public class ExactMatch : IRelationalProperty
    {
        public string Name => "Exact Match";
        public Type Type => typeof(ProseHtmlNode);
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            if (!(output is List<string> outList))
                yield break;

            var newTree = node.DeepCopy();
            var count = 0;
            newTree.Traverse(x => count += RemoveNonExactChildren(x, outList));
            Console.WriteLine($"Removed {count} non-exact children");
            newTree.RemoveDuplicates();
            newTree.Simplify();
            yield return Tuple.Create<object, object>(newTree, output);
        }

        private int RemoveNonExactChildren(ProseHtmlNode node, List<string> text)
        {
            return node.RemoveAllChildren(c => c.Text != null && c.Text.Length > 0 && !text.Contains(c.Text));
        }
    }
}
