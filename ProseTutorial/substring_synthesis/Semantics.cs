using System;
using System.Text.RegularExpressions;

namespace SubstringSynthesis
{
    public static class Semantics
    {
        public static string Concat(string s1, string s2)
        {
            return s1 + s2;
        }

        public static string SubstringPP(string v, int start, int end)
        {
            return v.Substring(start, end - start);
        }

        public static string SubstringPL(string v, int pos, int l)
        {
            return v.Substring(pos, l);
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
        }
    }
}