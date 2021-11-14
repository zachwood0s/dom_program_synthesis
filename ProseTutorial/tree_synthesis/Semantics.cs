using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis.Wrangling.Tree;
using HtmlAgilityPack;

namespace TreeManipulation
{
    public static class Semantics
    {

        public static IReadOnlyList<ProseHtmlNode> Concat(IReadOnlyList<ProseHtmlNode> a, IReadOnlyList<ProseHtmlNode> b)
            => a.Concat(b).ToList();
        public static IReadOnlyList<ProseHtmlNode> Children(ProseHtmlNode node)
        {
            return node.ChildNodes.ToList();
        }

        public static IReadOnlyList<ProseHtmlNode> Descendants(ProseHtmlNode node)
        {
            return node.Descendants.ToList();
        }

        public static IReadOnlyList<ProseHtmlNode> Single(ProseHtmlNode node)
        {
            return new List<ProseHtmlNode>{node};
        }

        public static ProseHtmlNode KthDescendantWithTag(ProseHtmlNode node, string tag, int k)
        {
            var filtered = Descendants(node).Where(x => x.Name == tag);
            if(k < 0)
            {
                return filtered.ElementAt(filtered.Count() + k);
            }

            return filtered.ElementAt(k);
        }

        public static bool MatchTag(ProseHtmlNode n, string tag)
        {
            return n.Name.Equals(tag);
        }

        public static bool MatchAttribute(ProseHtmlNode n, string attr)
        {
            return n[attr] != null;
        }

        public static bool MatchAttributeValue(ProseHtmlNode n, string attr, string value)
        {
            var attrVal = n[attr]?.Value;
            return attrVal != null ? attrVal.Equals(value) : false;
        }

        public static bool True() => true;

        public static bool NodeEquivalent(HtmlNode a, HtmlNode b)
        {
            if (a.Name != b.Name) return false;

            foreach(var attr in a.Attributes)
            {
                if (attr.Value != b.Attributes[attr.Name]?.Value)
                    return false;
            }

            if (a.ChildNodes.Count != b.ChildNodes.Count)
                return false;

            foreach (var (childA, childB) in a.ChildNodes.Zip(b.ChildNodes, Tuple.Create))
            {
                if(!NodeEquivalent(childA, childB))
                {
                    return false;
                }
            }
            return true;
        }

    }
}