using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using Microsoft.ProgramSynthesis.Wrangling.Tree;

namespace TreeManipulation
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public class CachedCalculation<TIn, TOut>
            where TOut: class
        {
            private Dictionary<TIn, TOut> _cachedValues;
            private Func<TIn, TOut> _calcFunc;

            public CachedCalculation(Func<TIn, TOut> calcFunc)
            {
                _calcFunc = calcFunc;
                _cachedValues = new Dictionary<TIn, TOut>();
            }
            public TOut GetValue(TIn inputState)
            {
                if(_cachedValues.TryGetValue(inputState, out var res))
                {
                    return res;
                }
                var newVal = _calcFunc(inputState);
                _cachedValues[inputState] = newVal;
                return newVal;
            }
        };

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
                var possibilities = new List<IReadOnlyList<ProseHtmlNode>>();

                int count = 0;
                foreach (IReadOnlyList<ProseHtmlNode> output in example.Value)
                {
                    for (var i = 0; i < output.Count - 1; i++)
                    {
                        var temp = new List<ProseHtmlNode>();
                        if (count == 0)
                        {
                            temp.Add(output[i]);
                        }
                        else
                        {
                            var previous = possibilities[count - 1];
                            foreach (var prev in previous)
                            {
                                temp.Add(prev);
                            }

                            temp.Add(output[i]);
                        }

                        possibilities.Add(temp);
                        count++;
                    }
                }
                if (possibilities.Count == 0) 
                    return null;
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
                var possibilities = new List<IReadOnlyList<ProseHtmlNode>>();

                foreach(IReadOnlyList<ProseHtmlNode> concat1List in startSpec.DisjunctiveExamples[inputState])
                {
                    var temp = (from output in example.Value
                               from outNode in (IReadOnlyList<ProseHtmlNode>)output
                               where concat1List.All(x => !x.Equals(outNode))
                               select outNode).Distinct().ToList();

                    possibilities.Add(temp.ToList());

                }
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
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var possibilities = new List<ProseHtmlNode>();
                foreach(IReadOnlyList<ProseHtmlNode> output in example.Value)
                {
                    var occurrences = input.OfType<ProseHtmlNode>()
                                           .RecursiveSelect(x => x.ChildNodes)
                                           .Where(x => x.ChildNodes.SequenceEqual(output))
                                           .ToList();
                    possibilities.AddRange(occurrences);

                }
                
                if (possibilities.Count == 0) 
                    return null;
                result[inputState] = possibilities.Distinct().ToList();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Descendants), 0)]
        public DisjunctiveExamplesSpec WitnessDescendants(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var occList = new List<ProseHtmlNode>();
                foreach (IReadOnlyList<ProseHtmlNode> output in example.Value)
                {
                    var occurrences = from i in input.RecursiveSelect(x => x.ChildNodes)
                                      where Semantics.Descendants(i).SequenceEqual(output)
                                      select i;

                    occList.AddRange(occurrences);
                }
                
                if (occList.Count == 0) 
                    return null;
                result[inputState] = occList.Distinct().ToList();
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
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var possibilites = new List<ProseHtmlNode>();
                foreach (IReadOnlyList<ProseHtmlNode> output in example.Value)
                {
                    if (output.Count > 1) 
                        return null; // Not possible to make a node into more than one element
                    var occurrences = input.RecursiveSelect(x => x.ChildNodes)
                                           .Where(x => x.Equals(output[0]))
                                           .ToList();
                    possibilites.AddRange(occurrences);
                }

                if (possibilites.Count == 0) 
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }
            return new DisjunctiveExamplesSpec(result);
        }


        private CachedCalculation<ProseHtmlNode, HashSet<string>> allLabels
            = new CachedCalculation<ProseHtmlNode, HashSet<string>>( 
                input => new[] { input }.RecursiveSelect(x => x.ChildNodes)
                                        .Select(x => x.Name)
                                        .ToHashSet()
            );

        [WitnessFunction(nameof(Semantics.MatchTag), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessMatchTag1(GrammarRule rule, ExampleSpec spec, DisjunctiveExamplesSpec startSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as ProseHtmlNode;
                var output = (bool) example.Value;

            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.MatchTag), 1)]
        public DisjunctiveExamplesSpec WitnessMatchTag2(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as ProseHtmlNode;
                var output = (bool) example.Value;

                // Find all of the possible labels in the given input tree
                var labels = allLabels.GetValue(inputState[Grammar.InputSymbol] as ProseHtmlNode);

                if (output)
                {
                    // If this node is supposed to be included in the output, 
                    // then the only possible label is the node's
                    result[inputState] = new[] { input.Name };
                }
                else
                {
                    // If this node is not supposed to be included in the output,
                    // then the possible labels include every label except the node's
                    result[inputState] = labels.Where(x => x != input.Name).ToHashSet();
                }
            }
            return new DisjunctiveExamplesSpec(result);
        }

        private CachedCalculation<ProseHtmlNode, HashSet<ProseAttribute>> allAttributes
            = new CachedCalculation<ProseHtmlNode, HashSet<ProseAttribute>>( 
                input => new[] { input }.RecursiveSelect(x => x.ChildNodes)
                                        .SelectMany(x => x.Attributes)
                                        .ToHashSet()
            );

        [WitnessFunction(nameof(Semantics.MatchAttribute), 1)]
        public DisjunctiveExamplesSpec WitnessMatchAttribute2(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as ProseHtmlNode;
                var output = (bool) example.Value;

                // Find all possible attributes in the tree
                var allAttrs = allAttributes.GetValue(inputState[Grammar.InputSymbol] as ProseHtmlNode);
                var allAttrNames = allAttrs.Select(x => x.Name);
                var attrs = input.Attributes.Select(x => x.Name).ToHashSet();

                if (output)
                {
                    // If this node is supposed to be included in the output,
                    // then the only possible set of attributes is this node's attrs
                    result[inputState] = attrs;
                }
                else
                {
                    // If this node is not supposed to be included in the output,
                    // then the possible labels include every other attribute found in the tree
                    result[inputState] = allAttrNames.Where(x => !attrs.Contains(x)).ToHashSet();
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

        public static bool NodeSequenceEqual(this IEnumerable<HtmlNode> a, IEnumerable<HtmlNode> b)
        {
            if (a.Count() != b.Count())
                return false;

            foreach (var (nodeA, nodeB) in a.Zip(b, Tuple.Create))
            {
                if (!Semantics.NodeEquivalent(nodeA, nodeB))
                {
                    return false;
                }
            }
            return true;
        }
    }
}