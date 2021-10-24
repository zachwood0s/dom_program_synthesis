﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis.Wrangling.Tree;

namespace TreeManipulation
{
    public static class Semantics
    {
        public static IReadOnlyList<Node> Children(Node node)
        {
            return node.Children;
        }

        public static IReadOnlyList<Node> Descendants(Node node)
        {
            return (from c in node.Children
                   from d in new[] { c }.RecursiveSelect(x => x.Children)
                   select d).ToList();
        }

        public static IReadOnlyList<Node> Single(Node node)
        {
            return new List<Node>{node};
        }

        public static bool MatchTag(Node n, string tag)
        {
            return n.Label.Equals(tag);
        }

        public static bool True() => true;

        public static bool Match(string n, string r)
        {
            return r.Equals(n);
        }
    }
}