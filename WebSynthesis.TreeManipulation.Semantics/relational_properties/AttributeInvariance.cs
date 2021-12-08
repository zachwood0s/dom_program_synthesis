using WebSynthesis.RelationalProperties;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSynthesis.TreeManipulation.RelationalProperties
{

    [RelationalProperty("Attribute Invariant", typeof(ProseHtmlNode))]
    public class AttributeInvariance : IRelationalProperty
    {
        public string Name => "Attribute Invariant";
        public Type Type => typeof(ProseHtmlNode);
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            //Not the best way to achieve this but it should work
            var newTree = node.DeepCopy();
            newTree.Traverse(x => x.RemoveRandomAttribute());
            yield return Tuple.Create<object, object>(newTree, output);
        }
    }
}
