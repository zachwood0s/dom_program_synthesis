using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebSynthesis.Substring
{
    public static class Semantics
    {
        public static IReadOnlyList<string> Split(string s, char c)
        {
            return s.Split(c);
        }
        public static IReadOnlyList<string> Concat(IReadOnlyList<string> l1, IReadOnlyList<string> l2)
        {
            return l1.Concat(l2).ToList();
        }

        public static IReadOnlyList<string> SelectK(IReadOnlyList<string> l, int k)
        {
            return new List<string>() { l[k] };
        }

        public static IReadOnlyList<string> SelectRegex(IReadOnlyList<string> l, Regex r)
        {
            return l.Where(s => r.Match(s).Success).ToList();
        }

        public static string TakeFirst(IReadOnlyList<string> l)
        {
            return l.First();
        }

        public static string JoinList(IReadOnlyList<string> l, char c)
        {
            return string.Join(c, l);
        }
    }
}