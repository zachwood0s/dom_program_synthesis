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

        [WitnessFunction(nameof(Semantics.Concat), 0)]
        public DisjunctiveExamplesSpec WitnessConcat1(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var possibilities = new List<IReadOnlyList<Node>>();

                int count = 0;
                foreach (IReadOnlyList<Node> output in example.Value)
                {
                    for (var i = 0; i < output.Count - 1; i++)
                    {
                        List<Node> temp = new List<Node>();
                        if (count == 0)
                        {
                            temp.Add(output[i]);
                        }
                        else
                        {
                            IReadOnlyList<Node> previous = possibilities[count - 1];
                            foreach (Node prev in previous)
                            {
                                temp.Add(prev);
                            }

                            temp.Add(output[i]);
                        }

                        possibilities.Add(temp);
                        count++;
                    }
                }
                if (possibilities.Count == 0) return null;
                result[inputState] = possibilities;
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Concat), 1, DependsOnParameters =new[] { 0 })]
        public DisjunctiveExamplesSpec WitnessConcat2(GrammarRule rule, DisjunctiveExamplesSpec spec, DisjunctiveExamplesSpec startSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var possibilities = new List<IReadOnlyList<Node>>();

                foreach(IReadOnlyList<Node> concat1List in startSpec.DisjunctiveExamples[inputState])
                {
                    var temp = from output in example.Value
                               from outNode in (IReadOnlyList<Node>)output
                               where concat1List.All(x => !x.Equals(outNode))
                               select outNode;

                    possibilities.Add(temp.ToList());

                }
                //var output = example.Value as IReadOnlyList<object>;
                //var input = startSpec.DisjunctiveExamples[inputState];
                result[inputState] = possibilities;

            }
            return new DisjunctiveExamplesSpec(result);
        }


        [WitnessFunction(nameof(Semantics.Children), 0)]
        public DisjunctiveExamplesSpec WitnessChildren(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as Node };

                var possibilities = new List<Node>();
                foreach(IReadOnlyList<Node> output in example.Value)
                {
                    var occurrences = input.OfType<Node>()
                                           .RecursiveSelect(x => x.Children)
                                           .Where(x => x.Children.SequenceEqual(output))
                                           .ToList();
                    possibilities.AddRange(occurrences);

                }
                
                if (possibilities.Count == 0) return null;
                result[inputState] = possibilities;
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Descendants), 0, Verify = true)]
        public DisjunctiveExamplesSpec WitnessDescendants(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as Node };

                var occList = new List<Node>();
                foreach (IReadOnlyList<Node> output in example.Value)
                {
                    var occurrences = from i in input.RecursiveSelect(x => x.Children)
                                      let set = new HashSet<object>(Semantics.Descendants(i))
                                      where Semantics.Descendants(i).SequenceEqual(output)
                                      select i;

                    occList.AddRange(occurrences);
                }
                
                if (occList.Count == 0) return null;
                result[inputState] = occList;
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Single), 0)]
        public DisjunctiveExamplesSpec WitnessSingle(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as Node };

                var possibilites = new List<Node>();
                foreach (IReadOnlyList<Node> output in example.Value)
                {
                    if (output.Count > 1) return null; // Not possible to make a node into more than one element
                    var occurrences = input.RecursiveSelect(x => x.Children).Where(x => x.Equals(output[0])).ToList();
                    possibilites.AddRange(occurrences);
                }

                if (possibilites.Count == 0) return null;
                result[inputState] = possibilites;
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