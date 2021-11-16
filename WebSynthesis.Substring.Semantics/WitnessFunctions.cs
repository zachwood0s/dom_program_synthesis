using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;

namespace WebSynthesis.Substring
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public WitnessFunctions(Grammar grammar) : base(grammar)
        {
        }

        private static Regex[] regexes =
        {
            new Regex(@"[a-z]+"),
            new Regex(@"[A-Z]+"),
            new Regex(@"\d+"),
            new Regex(@"\s+"),
            new Regex(@"(\n|\r|\r\n)"), //New Line
            new Regex(@"\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}") // Phone number
        };

        #region Split

        //[WitnessFunction(nameof(Semantics.Split), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessSplitStr(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec charSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[Grammar.InputSymbol] as string;
                var strs = new HashSet<string>();

                char c = (char)charSpec.Examples[inputState];

                foreach (IReadOnlyList<string> output in example.Value)
                {
                    if (output.Count > 1)
                    {
                        string i = string.Join(c, output);
                        if (input.Contains(i))
                            strs.Add(i);
                    }
                }

                if (strs.Count == 0) return null;
                result[inputState] = strs.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.Split), 1)]
        public DisjunctiveExamplesSpec WitnessSplitC(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[Grammar.InputSymbol] as string;
                var chars = new HashSet<char>();

                foreach (IReadOnlyList<string> output in example.Value)
                {
                    List<char> outputC = string.Join("", output).Distinct().ToList();
                    List<char> strC = input.Distinct().ToList();

                    if (outputC.Count() != strC.Count() - 1) continue;

                    List<char> diff = strC.Except(outputC).ToList();

                    if (diff.Count() != 1) continue;

                    chars.Add(diff.First());
                }

                if (chars.Count == 0) return null;
                result[inputState] = chars.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        #region JoinList

        [WitnessFunction(nameof(Semantics.JoinList), 1)]
        public DisjunctiveExamplesSpec WitnessJoinListC(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var chars = new HashSet<char>();

                foreach (string output in example.Value)
                {
                    List<char> newChars = output.Distinct().ToList();
                    chars = chars.Concat(newChars).ToHashSet();
                }

                if (chars.Count == 0) return null;
                result[inputState] = chars.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.JoinList), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessJoinListL(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec charSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var lists = new List<IReadOnlyList<string>>();

                char c = (char) charSpec.Examples[inputState];

                foreach (string output in example.Value)
                {
                    if (output.Contains(c))
                        lists.Add(output.Split(c));
                }

                if (lists.Count == 0) return null;
                result[inputState] = lists.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        #region Concat

        [WitnessFunction(nameof(Semantics.Concat), 0)]
        public DisjunctiveExamplesSpec WitnessConcat1(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var lists = new HashSet<IReadOnlyList<string>>();

                foreach (IReadOnlyList<string> output in example.Value)
                {
                    for (int i = 1; i < output.Count; i++) lists.Add(HelperMethods.SubArray(output.ToList(), 0, i));
                }

                if (lists.Count == 0) return null;
                result[inputState] = lists.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.Concat), 1, DependsOnParameters = new[] { 0 })]
        public DisjunctiveExamplesSpec WitnessConcat2(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec list1Spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var lists = new HashSet<IReadOnlyList<string>>();

                IReadOnlyList<string> l1 = (IReadOnlyList<string>) list1Spec.Examples[inputState];

                foreach (IReadOnlyList<string> output in example.Value)
                {
                    if (output.Count > l1.Count)
                        lists.Add(HelperMethods.SubArray(output.ToList(), l1.Count, output.Count - l1.Count));
                }

                if (lists.Count == 0) return null;
                result[inputState] = lists.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        #region SelectK

        [WitnessFunction(nameof(Semantics.SelectK), 1)]
        public DisjunctiveExamplesSpec WitnessSelectKK(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[Grammar.InputSymbol] as string;
                var k = new HashSet<int>();

                foreach (IReadOnlyList<string> output in example.Value)
                {
                    if (output.Count != 1) continue;

                    char? c = HelperMethods.getSplitChar(input, output);

                    if (c == null) continue;

                    List<string> lists = input.Split((char)c).ToList();

                    k.Add(lists.IndexOf(output.First()));
                }

                if (k.Count == 0) return null;
                result[inputState] = k.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.SelectK), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessSelectKList(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec kSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[Grammar.InputSymbol] as string;
                var lists = new HashSet<IReadOnlyList<string>>();

                int k = (int) kSpec.Examples[inputState];

                foreach (IReadOnlyList<string> output in example.Value)
                {
                    if (output.Count != 1) continue;

                    char? c = HelperMethods.getSplitChar(input, output);

                    if (c == null) continue;

                    IReadOnlyList<string> l = input.Split((char) c);

                    if (k == l.IndexOf(output.First())) lists.Add(l);
                }

                if (lists.Count == 0) return null;
                result[inputState] = lists.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        #region SelectRegex

        [WitnessFunction(nameof(Semantics.SelectRegex), 1)]
        public DisjunctiveExamplesSpec WitnessSelectRegexR(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var regexes = new HashSet<Regex>();

                foreach (IReadOnlyList<string> output in example.Value)
                {
                    regexes = regexes.Concat(HelperMethods.getMatches(output)).ToHashSet();
                }

                if (regexes.Count == 0) return null;
                result[inputState] = regexes.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.SelectRegex), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessSelectRegexList(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec regexSpec)
        {
      
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[Grammar.InputSymbol] as string;
                var lists = new HashSet<IReadOnlyList<string>>();

                Regex r = (Regex)regexSpec.Examples[inputState];

                foreach (IReadOnlyList<string> output in example.Value)
                {
                    if (output.Count < 1) continue;

                    char? c = HelperMethods.getSplitChar(input, output);

                    if (c == null) continue;

                    IReadOnlyList<string> list = input.Split((char)c);
                    IReadOnlyList<string> newList = list.Where(l => r.Match(l).Success).ToList();

                    if (newList.SequenceEqual(output)) lists.Add(list);
                }

                if (lists.Count == 0) return null;
                result[inputState] = lists.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        #region TakeFirst

        [WitnessFunction(nameof(Semantics.TakeFirst), 0)]
        public DisjunctiveExamplesSpec WitnessTakeFirst(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var lists = new List<IReadOnlyList<string>>();

                foreach (string output in example.Value)
                {
                    lists.Add(new List<string>() { output });
                }

                if (lists.Count == 0) return null;
                result[inputState] = lists.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        #region Helper Methods

        public static class HelperMethods
        {
            public static List<string> SubArray(List<string> array, int offset, int length)
            {
                string[] result = new string[length];
                Array.Copy(array.ToArray(), offset, result, 0, length);
                return result.ToList();
            }

            public static List<Regex> getMatches(IReadOnlyList<string> output)
            {
                var result = new List<Regex>();

                foreach (Regex r in regexes)
                {
                    bool isMatch = true;
                    foreach (string o in output)
                    {
                        Match m = r.Match(o);
                        if (!m.Success)
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch) result.Add(r);
                }

                return result;
            }

            public static char? getSplitChar(string input, IReadOnlyList<string> output)
            {
                int longest = 0;
                int index = 0;
                for(int i = 0; i < output.Count(); i++)
                {
                    if (output[i].Length > longest)
                    {
                        longest = output[i].Length;
                        index = i;
                    }
                }

                int start = input.IndexOf(output[index]);
                char? c = null;

                if (start - 1 > 0)
                {
                    c = input[start - 1];
                }
                else if (start + output.First().Count() < input.Length)
                {
                    c = input[start + output.First().Count()];
                }

                if (c == null) return null;
                if (string.Join("", output).Contains((char) c)) return null;
    
                return c;
            }
        }

        /*private static IEnumerable<string> getStrings(string input, int output, bool left = true)
        {
            if (left)
            {
                for (int i = 1; i < input.Length - output; i++)
                {
                    yield return input.Substring(output, i);
                }
            }
            else
            {
                for (int i = output - 1; i >= 0; i--)
                {
                    yield return input.Substring(i, output - i);
                }
            }
        }

        private static IEnumerable<Regex> getMatches(string input, int output, int len = 0)
        {
            List<Regex> chars = getChars();
            foreach(Regex r in regexes.Concat(chars))
            {
                Match m = r.Match(input);
                if (m.Index + m.Length*len == output)
                {
                    yield return r;
                }
            }
        }
        
        private static List<Regex> getChars()
        {
            List<Regex> chars = new List<Regex>();
            for (int i = 0; i < 127; i++)
            {
                char c = Convert.ToChar(i);
                if (!char.IsControl(c))
                {
                    string r = @"[\" + c.ToString() + "]";
                    if (isValidRegex(r))
                        chars.Add(new Regex(r));
                }
            }

            return chars;
        }

        private static bool isValidRegex(string r)
        {
            if (string.IsNullOrWhiteSpace(r)) return false;

            try
            {
                Regex.Match("", r);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        } */

        #endregion
    }
}