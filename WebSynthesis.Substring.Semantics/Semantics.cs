using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis.DslLibrary;
using Microsoft.ProgramSynthesis.Utils;

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
        public static StringRegion Substr(StringRegion v, Record<uint?, uint?>? posPair)
        {
            uint? start = posPair.Value.Item1;
            uint? end = posPair.Value.Item2;
            if (start == null || end == null || start < v.Start || start > v.End || end < v.Start || end > v.End)
                return null;
            return v.Slice((uint)start, (uint)end);
        }

        public static uint? AbsPos(StringRegion v, int k)
        {
            if (Math.Abs(k) > v.Length + 1) return null;
            return (uint)(k > 0 ? (v.Start + k - 1) : (v.End + k + 1));
        }

        public static uint? RegPos(StringRegion v, Record<RegularExpression, RegularExpression>? rr, int k)
        {
            List<PositionMatch> ms = rr.Value.Item1.Run(v).Where(m => rr.Value.Item2.MatchesAt(v, m.Right)).ToList();
            int index = k > 0 ? (k - 1) : (ms.Count + k);
            return index < 0 || index >= ms.Count ? null : (uint?)ms[index].Right;
        }

        public static string ToString(StringRegion stringRegion)
        {
            return stringRegion.ToString();
        }

        public static StringRegion ToStringRegion(string str)
        {
            return new StringRegion(str, Token.Tokens);
        }
    }
}