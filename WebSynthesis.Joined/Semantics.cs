using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WebSynthesis.TreeManipulation;

namespace WebSynthesis.Joined
{
    public static class Semantics
    {
        public static IReadOnlyList<string> NodesToStrs(IReadOnlyList<ProseHtmlNode> nodes)
        {
            List<string> strs = new List<string>();
            foreach(ProseHtmlNode node in nodes)
            {
                strs.Add(node.Text);
            }

            return strs;
        }

        public static IReadOnlyList<string> Concat(IReadOnlyList<string> a, IReadOnlyList<string> b)
            => a.Concat(b).ToList();

        public static ProseHtmlNode StrToTree(string url)
        {
            return new ProseHtmlNode(url);
        }
    }
}