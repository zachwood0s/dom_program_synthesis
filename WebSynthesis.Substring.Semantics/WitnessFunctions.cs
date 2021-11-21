using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.DslLibrary;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using Microsoft.ProgramSynthesis.Utils.Caching;

namespace WebSynthesis.Substring
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public WitnessFunctions(Grammar grammar) : base(grammar)
        {
        }

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

        #region Substring

        [WitnessFunction(nameof(Semantics.ToStringRegion), 0)]
        internal DisjunctiveExamplesSpec WitnessStrFromRegion(GrammarRule rule, ExampleSpec spec)
        {
            var ppExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var output = (StringRegion) spec.Examples[input];
                ppExamples[input] = new List<string>() { output.ToString() };
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        [WitnessFunction(nameof(Semantics.ToString), 0)]
        internal DisjunctiveExamplesSpec WitnessRegionFromStr(GrammarRule rule, ExampleSpec spec)
        {
            var ppExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var output = (string)spec.Examples[input];
                ppExamples[input] = new List<StringRegion>() { new StringRegion(output, Token.Tokens) };
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        [WitnessFunction(nameof(Semantics.Substr), 0)]
        internal DisjunctiveExamplesSpec WitnessInputRegion(GrammarRule rule, ExampleSpec spec)
        {
            var ppExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var v = new StringRegion((string)input[Grammar.InputSymbol], Token.Tokens);
                ppExamples[input] = new List<StringRegion>() { v };
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        [WitnessFunction(nameof(Semantics.Substr), 1)]
        internal DisjunctiveExamplesSpec WitnessPositionPair(GrammarRule rule, ExampleSpec spec)
        {
            var ppExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var v = new StringRegion((string) input[Grammar.InputSymbol], Token.Tokens);
                var desiredOutput = (StringRegion)spec.Examples[input];
                var occurrences = new List<object>();
                for (int i = v.Value.IndexOf(desiredOutput.Value, StringComparison.Ordinal);
                     i >= 0;
                     i = v.Value.IndexOf(desiredOutput.Value, i + 1, StringComparison.Ordinal))
                {
                    occurrences.Add(Record.Create(v.Start + (uint?)i, v.Start + (uint?)i + desiredOutput.Length));
                }
                ppExamples[input] = occurrences;
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        [WitnessFunction(nameof(Semantics.AbsPos), 1)]
        internal DisjunctiveExamplesSpec WitnessK(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var kExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var v = new StringRegion((string)input[Grammar.InputSymbol], Token.Tokens);
                var positions = new List<object>();
                foreach (uint pos in spec.DisjunctiveExamples[input])
                {
                    positions.Add((int)pos + 1 - (int)v.Start);
                    positions.Add((int)pos - (int)v.End - 1);
                }
                kExamples[input] = positions;
            }
            return DisjunctiveExamplesSpec.From(kExamples);
        }

        [WitnessFunction(nameof(Semantics.RegPos), 1)]
        internal DisjunctiveExamplesSpec WitnessRegexPair(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var rrExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var v = new StringRegion((string)input[Grammar.InputSymbol], Token.Tokens);
                var regexes = new List<object>();
                foreach (uint pos in spec.DisjunctiveExamples[input])
                {
                    UnboundedCache<Token, TokenMatch> rightMatches;
                    if (!v.Cache.TryGetAllMatchesStartingAt(pos, out rightMatches)) continue;
                    UnboundedCache<Token, TokenMatch> leftMatches;
                    if (!v.Cache.TryGetAllMatchesEndingAt(pos, out leftMatches)) continue;
                    var leftRegexes = RegularExpression.LearnLeftMatches(v, pos, RegularExpression.DefaultTokenCount);
                    var rightRegexes = RegularExpression.LearnRightMatches(v, pos, RegularExpression.DefaultTokenCount);
                    var regexPairs =
                        from l in leftRegexes from r in rightRegexes select (object)Record.Create(l, r);
                    regexes.AddRange(regexPairs);
                }
                rrExamples[input] = regexes;
            }
            return DisjunctiveExamplesSpec.From(rrExamples);
        }

        [WitnessFunction(nameof(Semantics.RegPos), 2, DependsOnParameters = new[] { 1 })]
        internal DisjunctiveExamplesSpec WitnessRegexCount(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec regexBinding)
        {
            var kExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var v = new StringRegion((string)input[Grammar.InputSymbol], Token.Tokens);
                var rr = (Record<RegularExpression, RegularExpression>)regexBinding.Examples[input];
                var ks = new List<object>();
                foreach (uint pos in spec.DisjunctiveExamples[input])
                {
                    var ms = rr.Item1.Run(v).Where(m => rr.Item2.MatchesAt(v, m.Right)).ToArray();
                    int index = ms.BinarySearchBy(m => m.Right.CompareTo(pos));
                    if (index < 0) return null;
                    ks.Add(index + 1);
                    ks.Add(index - ms.Length);
                }
                kExamples[input] = ks;
            }
            return DisjunctiveExamplesSpec.From(kExamples);
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

                foreach (Regex r in RegexUtils.Tokens)
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
                if (start < 0) return c;

                if (start - 1 > 0)
                {
                    c = input[start - 1];
                }
                else if (start + output[index].Count() < input.Length)
                {
                    c = input[start + output[index].Count()];
                }

                if (c == null) return null;
                if (string.Join("", output).Contains((char) c)) return null;
    
                return c;
            }
        }

        #endregion
    }
}