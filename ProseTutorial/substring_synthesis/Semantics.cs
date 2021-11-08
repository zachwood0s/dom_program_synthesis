using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SubstringSynthesis
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

        public static IReadOnlyList<string> JoinList(IReadOnlyList<string> l, char c)
        {
            return new List<string>() { string.Join(c, l) };
        }


        // Not used
        /*public static string[] SubList(string[] l, int start, int end)
        {
            return new List<string>(l).GetRange(start, end - start).ToArray();
        }

        public static string SubstringPP(string s, int start, int end)
        {
            return s.Substring(start, end - start);
        }

        public static string SubstringPL(string s, int pos, int l)
        {
            return s.Substring(pos, l);
        }

        public static int? StrPosLeft(string s, string ss)
        {
            return s.IndexOf(ss);
        }

        public static int? StrPosRight(string s, string ss)
        {
            return s.IndexOf(ss) + ss.Length;
        }

        public static int? AbsPos(string v, int k)
        {
            return k > 0 ? k - 1 : v.Length + k + 1;
        }

        public static int? RelPosLeft(string v, Regex rs)
        {
            MatchCollection matches = rs.Matches(v);

            foreach (Match match in matches)
            {
                return match.Index;
            }

            return null;
        }

        public static int? RelPosRight(string v, Regex re)
        {
            MatchCollection matches = re.Matches(v);

            foreach (Match match in matches)
            {
                return match.Index + match.Length;
            }

            return null;
        }

        public static string Date(string v)
        {
            DateTime date = new DateTime();
            string[] possibleDates = v.Split(" ");

            foreach(string pdate in possibleDates)
            {
                if (DateTime.TryParse(pdate, out date))
                {
                    return pdate;
                }
            }

            return v;
        }*/
    }
}