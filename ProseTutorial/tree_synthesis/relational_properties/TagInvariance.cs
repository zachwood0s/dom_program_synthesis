using RelationalProperties;
using System;
using System.Collections.Generic;
using System.Text;

namespace TreeManipulation.RelationalProperties
{

    [RelationalProperty("Tag Invariant", typeof(ProseHtmlNode))]
    public class TagInvariance : IRelationalProperty
    {
        public string Name => "Tag Invariant";
        private const int MaxReorderCount = 10;
        public IEnumerable<Tuple<object, object>> ApplyProperty(object input, object output)
        {
            var node = input as ProseHtmlNode;
            //Not the best way to achieve this but it should work
            for(var i = 0; i < MaxReorderCount; i++)
            {
                var newTree = node.DeepCopy();
                newTree.Traverse(x => x.Name = PermuteTag(x.Name));
                yield return Tuple.Create<object, object>(newTree, output);
            }
        }
        
        private string PermuteTag(string tag)
        {
            switch (tag)
            {
                case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                    return new[] { "h1", "h2", "h3", "h4", "h5", "h6" }.RandomElement();

                case "div": case "span": case "p": 
                    return new[] { "div", "span", "p", "strong", "li" }.RandomElement();

                default:
                    return tag;
            }
        }
    }

    public static class CollectionExtension
    {
        private static Random rng = new Random();

        public static T RandomElement<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        }

        public static T RandomElement<T>(this T[] array)
        {
            return array[rng.Next(array.Length)];
        }
    }
}
