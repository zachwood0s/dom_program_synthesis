using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Wrangling.Tree;

namespace TreeManipulation
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public WitnessFunctions(Grammar grammar) : base(grammar)
        {
        }


        [WitnessFunction(nameof(Semantics.Children), 0)]
        public DisjunctiveExamplesSpec WitnessChildren(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as Node };
                var output = example.Value as IReadOnlyList<object>;
                var occurrences = input.OfType<Node>()
                                       .RecursiveSelect(x => x.Children)
                                       .Where(x => x.Children.SequenceEqual(output))
                                       .ToList();
                
                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Descendants), 0, Verify = true)]
        public DisjunctiveExamplesSpec WitnessDescendants(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as Node };
                var output = example.Value as IReadOnlyList<object>;
                var outSet = new HashSet<object>(output);

                var occurrences = from i in input.RecursiveSelect(x => x.Children)
                                  let set = new HashSet<object>(Semantics.Descendants(i))
                                  //where Semantics.Descendants(i).SequenceEqual(output)
                                  where set.Intersect(outSet).SequenceEqual(outSet)
                                  select i;

                var occList = occurrences.ToList();
                
                if (occList.Count == 0) return null;
                result[inputState] = occList.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Single), 0)]
        public DisjunctiveExamplesSpec WitnessSingle(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as Node };
                var output = example.Value as IReadOnlyList<object>;

                var occurrences = input.RecursiveSelect(x => x.Children).Where(x => x.Equals(output[0])).ToList();

                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }


        /*
        [WitnessFunction(nameof(Semantics.MatchTag), 0)]
        public DisjunctiveExamplesSpec WitnessMatchTag1(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as Node };
                result[inputState] = input.RecursiveSelect(x => x.Children).ToList();
            }
            return new DisjunctiveExamplesSpec(result);
        }
        */

        [WitnessFunction(nameof(Semantics.MatchTag), 1)]
        public DisjunctiveExamplesSpec WitnessMatchTag2(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as Node;
                var output = (bool) example.Value;

                var allLabels = new[] { inputState[Grammar.InputSymbol] as Node }.RecursiveSelect(x => x.Children)
                                                                                 .Select(x => x.Label).ToList();

                if (output)
                {
                    result[inputState] = new[] { input.Label };
                }
                else
                {
                    result[inputState] = allLabels.Where(x => x != input.Label).ToList();
                }
            }
            return new DisjunctiveExamplesSpec(result);
        }
    }

    public static class IEnumerableExtensions
    {
        public static IEnumerable<TSource> RecursiveSelect<TSource>(
            this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> childSelector)
        {
            var stack = new Stack<IEnumerator<TSource>>();
            var enumerator = source.GetEnumerator();

            try
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        TSource element = enumerator.Current;
                        yield return element;

                        stack.Push(enumerator);
                        enumerator = childSelector(element).GetEnumerator();
                    }
                    else if (stack.Count > 0)
                    {
                        enumerator.Dispose();
                        enumerator = stack.Pop();
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
            finally
            {
                enumerator.Dispose();

                while (stack.Count > 0) // Clean up in case of an exception.
                {
                    enumerator = stack.Pop();
                    enumerator.Dispose();
                }
            }
        }
    }
}