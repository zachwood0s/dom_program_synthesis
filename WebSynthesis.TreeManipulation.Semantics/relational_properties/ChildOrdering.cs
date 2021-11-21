using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSynthesis.RelationalProperties;

namespace WebSynthesis.TreeManipulation.RelationalProperties
{
    [RelationalProperty("Child Order Invariant", typeof(ProseHtmlNode))]
    public class ChildOrderingInvariance : IRelationalProperty
    {
        public string Name => "Child Order Invariant";
        public Type Type => typeof(ProseHtmlNode);
        private const int MaxReorderCount = 2;
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            //Not the best way to achieve this but it should work
            for(var i = 0; i < MaxReorderCount; i++)
            {
                var newTree = node.DeepCopy();
                newTree.Traverse(x => x.RandomlyOrderChildren());
                yield return Tuple.Create<object, object>(newTree, output);
            }
        }
    }
}
