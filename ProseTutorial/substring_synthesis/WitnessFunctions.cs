using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;

namespace SubstringSynthesis
{
    public class WitnessFunctions : DomainLearningLogic
    {
        private static string[] regexParts =
        {
            @"[a-zA-Z]", @"\d", @"\s", @"$", "+",
            @"\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}" // Phone number
        };

        public WitnessFunctions(Grammar grammar) : base(grammar)
        {
        }

        #region Concat Witness Functions

        [WitnessFunction(nameof(Semantics.Concat), 0)]
        public DisjunctiveExamplesSpec WitnessS1(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var output = example.Value as string;
                var strings = new List<string>();

                for (int i = 1; i < output.Length; i++) strings.Add(output.Substring(0, i));

                //if (strings.Count == 0) return null;
                result[inputState] = strings;
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Concat), 1, DependsOnParameters = new[] { 0 })]
        public ExampleSpec WitnessS2(GrammarRule rule, ExampleSpec spec, ExampleSpec s1Spec)
        {
            var result = new Dictionary<State, object>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var output = example.Value as string;
                var s1 = (string) s1Spec.Examples[inputState];
                result[inputState] = output.Substring(s1.Length - 1, output.Length - s1.Length);
            }

            return new ExampleSpec(result);
        }

        #endregion

        #region Substring Witness Fuctions

        [WitnessFunction(nameof(Semantics.SubstringPP), 1)]
        [WitnessFunction(nameof(Semantics.SubstringPL), 1)]
        public DisjunctiveExamplesSpec WitnessStartPosition(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var output = example.Value as string;
                var occurrences = new List<int>();

                for (int i = input.IndexOf(output); i >= 0; i = input.IndexOf(output, i + 1)) occurrences.Add(i);

                //if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.SubstringPP), 2, DependsOnParameters = new[] { 1 })]
        public ExampleSpec WitnessEndPosition(GrammarRule rule, ExampleSpec spec, ExampleSpec startSpec)
        {
            var result = new Dictionary<State, object>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var output = example.Value as string;
                var start = (int)startSpec.Examples[inputState];
                result[inputState] = start + output.Length;
            }

            return new ExampleSpec(result);
        }

        [WitnessFunction(nameof(Semantics.SubstringPL), 2)]
        public ExampleSpec WitnessL(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, object>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                string output = example.Value as string;

                result[inputState] = output.Length;
            }

            return new ExampleSpec(result);
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

                var positions = new List<int>();
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

        [WitnessFunction(nameof(Semantics.RelPosLeft), 1)]
        [WitnessFunction(nameof(Semantics.RelPosRight), 1)]
        public DisjunctiveExamplesSpec WitnessR(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var ids = new List<string>();
                var regexes = new List<Regex>();

                foreach (int output in example.Value)
                {
                    if (rule.Id == "RelPosLeft")
                    {
                        for (int i = input.Length - output; i > 0; i--) ids.Add("[" + input.Substring(output, i) + "]");
                    }
                    else
                    {
                        for (int i = 0; i < output; i++) ids.Add("[" + input.Substring(i, output - i) + "]");
                    }

                    if (rule.Id == "RelPosLeft")
                    {
                        regexes = getMatches(input, output, ids).ToList();
                    }
                    else
                    {
                        regexes = getMatches(input, output, ids, len: 1).ToList();
                    }
                }

                if (regexes.Count == 0) return null;
                result[inputState] = regexes;
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #endregion

        public DisjunctiveExamplesSpec WitnessID(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var ids = new List<string>();

                foreach (int output in example.Value)
                {
                    if (rule.Id == "RelPosLeft")
                    {
                        for (int i = input.Length - output; i > 0; i--) ids.Add(input.Substring(output, i));
                    }
                    else
                    {
                        for (int i = 0; i < output; i++) ids.Add(input.Substring(i, output - i));
                    }
                }

                if (ids.Count == 0) return null;
                result[inputState] = ids;
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        #region Helper Methods

        private static IEnumerable<Regex> getMatches(string input, int output, List<string> ids, int len = 0)
        {
            Queue<string> validRegexes = new Queue<string>(regexParts.Concat(ids));
            bool isSuccess = false;
            int currentLevel = validRegexes.Count, nextLevel = 0, depth = 0;
            while (validRegexes.Count > 0)
            {
                string validRegex = validRegexes.Dequeue();
                List<string> nextRegexes = new List<string>();
                foreach (string regexPart in regexParts)
                {
                    string nextRegexString = validRegex + regexPart;
                    if (isValidRegex(nextRegexString))
                    {
                        Regex nextRegex = new Regex(nextRegexString);
                        MatchCollection mc = nextRegex.Matches(input);

                        if (mc.Count == 1 && mc.First().Index + mc.First().Length * len == output)
                        {
                            //isSuccess = true;
                            yield return nextRegex;
                        }
                        else if (mc.Count > 1)
                        {
                            nextRegexes.Add(nextRegexString);
                        }
                    }
                }

                if (!isSuccess)
                {
                    foreach (string s in nextRegexes) validRegexes.Enqueue(s);
                    nextLevel += nextRegexes.Count;
                }

                currentLevel--;
                if (currentLevel == 0)
                {
                    currentLevel = nextLevel;
                    nextLevel = 0;
                    if (++depth == 5) yield break;
                }
            }
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
        }

        #endregion
    }
}