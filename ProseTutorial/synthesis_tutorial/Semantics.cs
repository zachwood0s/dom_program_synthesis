using System;
using System.Text.RegularExpressions;

namespace WebSynthesis
{
    public static class Semantics
    {

        public static string Cat(string a, string b)
        {
            return a + b;
        }

        public static string Substring(string v, int start, int end)
        {
            return v.Substring(start, end - start);
        }

        public static int? AbsPos(string v, int k)
        {
            return k > 0 ? k - 1 : v.Length + k + 1;
        }

    }
}