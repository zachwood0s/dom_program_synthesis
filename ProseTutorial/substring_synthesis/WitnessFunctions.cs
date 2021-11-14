using System;
using System.Collections.Generic;
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
            new Regex(@"[a-zA-Z]+"),
            new Regex(@"\d+"),
            new Regex(@"\s+"),
            new Regex(@"(\n|\r|\r\n)"),
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
                        string i = string.Join((char)c, output);
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
        public DisjunctiveExamplesSpec WitnessConcat2(GrammarRule rule, DisjunctiveExamplesSpec spec, DisjunctiveExamplesSpec list1Spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var lists = new HashSet<IReadOnlyList<string>>();

                foreach (IReadOnlyList<string> l1 in list1Spec.DisjunctiveExamples[inputState])
                {
                    foreach (IReadOnlyList<string> output in example.Value)
                    {
                        if (output.Count > l1.Count)
                            lists.Add(HelperMethods.SubArray(output.ToList(), l1.Count, output.Count - l1.Count));
                    }
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

        /* #region Concat Witness Functions

        //[WitnessFunction(nameof(Semantics.Concat), 0)]
        public DisjunctiveExamplesSpec WitnessS1(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var strings = new HashSet<string>();

                foreach (string output in example.Value)
                {
                    for (int i = 1; i < output.Length; i++) strings.Add(output.Substring(0, i));
                }

                if (strings.Count == 0) return null;
                result[inputState] = strings;
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        //[WitnessFunction(nameof(Semantics.Concat), 1, DependsOnParameters = new[] { 0 })]
        public DisjunctiveExamplesSpec WitnessS2(GrammarRule rule, DisjunctiveExamplesSpec spec, DisjunctiveExamplesSpec s1Spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var outputs = new HashSet<string>();

                foreach (string s1 in s1Spec.DisjunctiveExamples[inputState])
                {
                    foreach (string output in example.Value)
                        if (output.Length > s1.Length && output.IndexOf(s1) == 0)
                            outputs.Add(output.Substring(s1.Length, output.Length - s1.Length));
                }

                if (outputs.Count == 0) return null;
                result[inputState] = outputs;
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        #region Substring Witness Fuctions

        [WitnessFunction(nameof(Semantics.SubstringPP), 1)]
        [WitnessFunction(nameof(Semantics.SubstringPL), 1)]
        public DisjunctiveExamplesSpec WitnessStartPosition(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[Grammar.InputSymbol] as string;
                var occurrences = new HashSet<int>();

                foreach (string output in example.Value)
                {
                    for (int i = input.IndexOf(output); i >= 0; i = input.IndexOf(output, i + 1)) occurrences.Add(i);
                }

                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.SubstringPP), 2, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessEndPosition(GrammarRule rule, DisjunctiveExamplesSpec spec, DisjunctiveExamplesSpec startSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[Grammar.InputSymbol] as string;
                var occurrences = new HashSet<int>();

                foreach (int start in startSpec.DisjunctiveExamples[inputState])
                {
                    foreach (string output in example.Value)
                        if (input.IndexOf(output) == start) occurrences.Add(start + output.Length);
                }

                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.SubstringPL), 2)]
        public DisjunctiveExamplesSpec WitnessL(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var ls = new HashSet<int>();

                foreach(string output in example.Value)
                {
                    ls.Add(output.Length);
                }

                result[inputState] = ls.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        #region Position Witness Functions

        [WitnessFunction(nameof(Semantics.AbsPos), 1)]
        public DisjunctiveExamplesSpec WitnessK(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var v = inputState[rule.Body[0]] as string;
                var positions = new HashSet<int>();

                foreach (int pos in example.Value)
                {
                    positions.Add(pos + 1);
                    positions.Add(pos - v.Length - 1);
                }

                if (positions.Count == 0) return null;
                result[inputState] = positions.Cast<object>();
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.StrPosLeft), 1)]
        [WitnessFunction(nameof(Semantics.StrPosRight), 1)]
        public DisjunctiveExamplesSpec WitnessSS(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var strs = new List<string>();

                foreach (int output in example.Value)
                {
                    if (rule.Id == "StrPosLeft")
                    {
                        strs = getStrings(input, output).ToList();
                    }
                    else
                    {
                        strs = getStrings(input, output, left: false).ToList();
                    }
                }

                if (strs.Count == 0) return null;
                result[inputState] = strs;
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        [WitnessFunction(nameof(Semantics.RelPosLeft), 1)]
        [WitnessFunction(nameof(Semantics.RelPosRight), 1)]
        public DisjunctiveExamplesSpec WitnessR(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var regexes = new List<Regex>();

                foreach (int output in example.Value)
                {
                    if (rule.Id == "RelPosLeft")
                    {
                        regexes = getMatches(input, output).ToList();
                    }
                    else
                    {
                        regexes = getMatches(input, output, len: 1).ToList();
                    }
                }

                if (regexes.Count == 0) return null;
                result[inputState] = regexes;
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion */

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