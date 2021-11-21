using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis.Wrangling.Tree;
using HtmlAgilityPack;

namespace WebSynthesis.TreeManipulation
{
    public static class Semantics
    {

        public static IReadOnlyList<ProseHtmlNode> Concat(IReadOnlyList<ProseHtmlNode> a, IReadOnlyList<ProseHtmlNode> b)
            => a.Concat(b).ToList();
        public static IReadOnlyList<ProseHtmlNode> Children(ProseHtmlNode node)
        {
            return node.ChildNodes.ToList();
        }

        private static Dictionary<ProseHtmlNode, IReadOnlyList<ProseHtmlNode>> _cachedDescendants = new Dictionary<ProseHtmlNode, IReadOnlyList<ProseHtmlNode>>();
        public static IReadOnlyList<ProseHtmlNode> Descendants(ProseHtmlNode node)
        {
            if (_cachedDescendants.TryGetValue(node, out var res))
                return res;
            var newList = node.Descendants.ToList();
            _cachedDescendants[node] = newList;
            return newList;
        }

        public static IReadOnlyList<ProseHtmlNode> Single(ProseHtmlNode node)
        {
            return new List<ProseHtmlNode>{node};
        }

        private static Dictionary<Tuple<string, ProseHtmlNode>, IReadOnlyList<ProseHtmlNode>> _cached = new Dictionary<Tuple<string, ProseHtmlNode>, IReadOnlyList<ProseHtmlNode>>();
        public static IReadOnlyList<ProseHtmlNode> DescendantsWithTag(ProseHtmlNode node, string tag)
        {
            var key = Tuple.Create(tag, node);
            if (_cached.TryGetValue(key, out var res))
                return res;
            var newList = Descendants(node).Where(x => x.Name == tag).ToList();
            _cached[key] = newList;
            return newList;
        }

        private static Dictionary<Tuple<string, ProseHtmlNode>, IReadOnlyList<ProseHtmlNode>> _cachedAttrs = new Dictionary<Tuple<string, ProseHtmlNode>, IReadOnlyList<ProseHtmlNode>>();
        public static IReadOnlyList<ProseHtmlNode> DescendantsWithAttr(ProseHtmlNode node, string attr)
        {
            var key = Tuple.Create(attr, node);
            if (_cachedAttrs.TryGetValue(key, out var res))
                return res;
            var newList = Descendants(node).Where(x => x[attr] != null).ToList();
            _cachedAttrs[key] = newList;
            return newList;
        }

        private static Dictionary<Tuple<string, string, ProseHtmlNode>, IReadOnlyList<ProseHtmlNode>> _cachedAttrValues = new Dictionary<Tuple<string, string, ProseHtmlNode>, IReadOnlyList<ProseHtmlNode>>();
        public static IReadOnlyList<ProseHtmlNode> DescendantsWithAttrValue(ProseHtmlNode node, string attr, string value)
        {
            var key = Tuple.Create(attr, value, node);
            if (_cachedAttrValues.TryGetValue(key, out var res))
                return res;
            var newList = Descendants(node).Where(x => x[attr]?.Value == value).ToList();
            _cachedAttrValues[key] = newList;
            return newList;
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