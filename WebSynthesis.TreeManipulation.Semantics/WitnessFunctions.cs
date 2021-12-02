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

namespace WebSynthesis.TreeManipulation
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public class CachedCalculation<TIn, TOut>
            where TOut: class
        {
            private Dictionary<TIn, TOut> _cachedValues;
            private Func<TIn, TOut> _calcFunc;
            private int _cacheHit = 0;
            private int _cacheMiss = 0;

            public CachedCalculation(Func<TIn, TOut> calcFunc)
            {
                _calcFunc = calcFunc;
                _cachedValues = new Dictionary<TIn, TOut>();
            }
            public TOut GetValue(TIn inputState)
            {
                if(_cachedValues.TryGetValue(inputState, out var res))
                {
                    _cacheHit++;
                    return res;
                }
                _cacheMiss++;

                var newVal = _calcFunc(inputState);
                _cachedValues[inputState] = newVal;
                return newVal;
            }
            public override string ToString()
            {
                return $"{_cacheHit} / {_cacheMiss}";
            }
        };

        public WitnessFunctions(Grammar grammar) : base(grammar)
        {
        }

        #region Concat

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
                        if (i == 0)
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
                                let o = (IReadOnlyList<ProseHtmlNode>) output
                                where o.ContainsSubsequence(concat1List)
                                from outNode in (IReadOnlyList<ProseHtmlNode>)output
                                where concat1List.All(x => !x.Equals(outNode))
                                select outNode).Distinct().ToList();

                    possibilities.Add(temp.ToList());

                }
                result[inputState] = possibilities;

            }
            return new DisjunctiveExamplesSpec(result);
        }

        #endregion


        #region Children/Descendants/Single

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

        [WitnessFunction(nameof(Semantics.Children), 0)]
        public DisjunctiveExamplesSpec WitnessChildrenSubseq(GrammarRule rule, DisjunctiveSubsequenceSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<IEnumerable<object>>> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var possibilities = new List<ProseHtmlNode>();
                foreach(IEnumerable<object> outputList in example.Value)
                {
                    var occurrences = input.OfType<ProseHtmlNode>()
                                           .RecursiveSelect(x => x.ChildNodes)
                                           .Where(x => outputList.All(x.ChildNodes.Contains))
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

        #endregion

        private CachedCalculation<ProseHtmlNode, HashSet<string>> allLabels
            = new CachedCalculation<ProseHtmlNode, HashSet<string>>( 
                input => new[] { input }.RecursiveSelect(x => x.ChildNodes)
                                        .Select(x => x.Name)
                                        .ToHashSet()
            );

        private IEnumerable<List<ProseHtmlNode>> PossibleLists(ProseHtmlNode input, string tag)
        {
            var allNodes = new[] { input }.RecursiveSelect(x => x.ChildNodes);
            foreach(var node in allNodes)
            {
                yield return Semantics.Descendants(node).ToList();
                yield return Semantics.Children(node).ToList();
            }
        }

        #region Descendants With Tag

        private CachedCalculation<Tuple<ProseHtmlNode, string>, List<Tuple<ProseHtmlNode, List<ProseHtmlNode>>>> descendentsByTag
            = new CachedCalculation<Tuple<ProseHtmlNode, string>, List<Tuple<ProseHtmlNode, List<ProseHtmlNode>>>>(
                input => new[] { input.Item1 }.RecursiveSelect(x => x.ChildNodes)
                         .Select(x => Tuple.Create(x, Semantics.DescendantsWithTag(x, input.Item2).ToList()))
                         .ToList()
            );

        [WitnessFunction(nameof(Semantics.DescendantsWithTag), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessDescendantsWithTagSubseq(GrammarRule rule, DisjunctiveSubsequenceSpec spec, ExampleSpec tagSpec)
        {
            // Basically the same thing as Descendants but for a single node now
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<IEnumerable<object>>> example in spec.Examples)
            {
                State inputState = example.Key;
                var tag = tagSpec.Examples[inputState] as string;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var occList = new List<ProseHtmlNode>();
                foreach (IEnumerable<object> output in example.Value)
                {
                    var descendents = descendentsByTag.GetValue(Tuple.Create(input[0], tag));
                    var occurrences = from i in descendents
                                      where i.Item2.ContainsSubsequence(output)
                                      select i.Item1;

                    occList.AddRange(occurrences);
                }
                
                if (occList.Count == 0) 
                    return null;
                result[inputState] = occList.Distinct().ToList();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithTag), 1)]
        public DisjunctiveExamplesSpec WitnessDescendantsWithTag1Subseq(GrammarRule rule, DisjunctiveSubsequenceSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<IEnumerable<object>>> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };
                var possibilites = new List<string>();
                foreach (IEnumerable<object> output in example.Value)
                {
                    var distinct = output.Cast<ProseHtmlNode>().Select(x => x.Name).Distinct();
                    if (distinct.Count() > 1)
                        return null;

                    possibilites.Add(distinct.First());
                }

                if (possibilites.Count == 0)
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithTag), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessDescendantsWithTag(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec tagSpec)
        {
            // Basically the same thing as Descendants but for a single node now
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var tag = tagSpec.Examples[inputState] as string;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var occList = new List<ProseHtmlNode>();
                foreach (IEnumerable<ProseHtmlNode> output in example.Value)
                {
                    var occurrences = from i in input.RecursiveSelect(x => x.ChildNodes)
                                      where Semantics.Descendants(i).Where(x => x.Name == tag).SequenceEqual(output)
                                      select i;

                    occList.AddRange(occurrences);
                }
                
                if (occList.Count == 0) 
                    return null;
                result[inputState] = occList.Distinct().ToList();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithTag), 1)]
        public DisjunctiveExamplesSpec WitnessDescendantsWithTag1(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };
                var possibilites = new List<string>();
                foreach (IEnumerable<ProseHtmlNode> output in example.Value)
                {
                    var distinct = output.Select(x => x.Name).Distinct();
                    if (distinct.Count() > 1)
                        return null;

                    possibilites.Add(distinct.First());
                }

                if (possibilites.Count == 0)
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        #endregion

        #region Descendants With Attr

        private CachedCalculation<Tuple<ProseHtmlNode, string>, List<Tuple<ProseHtmlNode, List<ProseHtmlNode>>>> descendentsByAttr
            = new CachedCalculation<Tuple<ProseHtmlNode, string>, List<Tuple<ProseHtmlNode, List<ProseHtmlNode>>>>(
                input => new[] { input.Item1 }.RecursiveSelect(x => x.ChildNodes)
                         .Select(x => Tuple.Create(x, Semantics.DescendantsWithAttr(x, input.Item2).ToList()))
                         .ToList()
            );

        [WitnessFunction(nameof(Semantics.DescendantsWithAttr), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttrSubseq(GrammarRule rule, DisjunctiveSubsequenceSpec spec, ExampleSpec tagSpec)
        {
            // Basically the same thing as Descendants but for a single node now
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<IEnumerable<object>>> example in spec.Examples)
            {
                State inputState = example.Key;
                var attr = tagSpec.Examples[inputState] as string;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var occList = new List<ProseHtmlNode>();
                foreach (IEnumerable<object> output in example.Value)
                {
                    var descendents = descendentsByAttr.GetValue(Tuple.Create(input[0], attr));
                    var occurrences = from i in descendents
                                      where i.Item2.ContainsSubsequence(output)
                                      select i.Item1;

                    occList.AddRange(occurrences);
                }
                
                if (occList.Count == 0) 
                    return null;
                result[inputState] = occList.Distinct().ToList();
            }
            //Console.WriteLine(descendentsByAttr);
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithAttr), 1)]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttr1Subseq(GrammarRule rule, DisjunctiveSubsequenceSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<IEnumerable<object>>> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };
                var possibilites = new List<string>();
                foreach (IEnumerable<object> output in example.Value)
                {
                    var attrSets = output.Cast<ProseHtmlNode>().Select(x => x.Attributes);
                    var resSet = new HashSet<string>(attrSets.First().Select(x => x.Name));
                    foreach(var attrSet in attrSets)
                    {
                        var set = new HashSet<string>(attrSet.Select(x => x.Name));
                        resSet.IntersectWith(set);
                    }
                    if (resSet.Count == 0)
                        return null;

                    possibilites.AddRange(resSet);
                }

                if (possibilites.Count == 0)
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithAttr), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttr(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec tagSpec)
        {
            // Basically the same thing as Descendants but for a single node now
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var attr = tagSpec.Examples[inputState] as string;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var occList = new List<ProseHtmlNode>();
                foreach (IEnumerable<ProseHtmlNode> output in example.Value)
                {
                    var descendents = descendentsByAttr.GetValue(Tuple.Create(input[0], attr));
                    var occurrences = from i in descendents
                                      where i.Item2.SequenceEqual(output)
                                      select i.Item1;

                    occList.AddRange(occurrences);
                }
                
                if (occList.Count == 0) 
                    return null;
                result[inputState] = occList.Distinct().ToList();
            }
            //Console.WriteLine(descendentsByAttr);
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithAttr), 1)]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttr1(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };
                var possibilites = new List<string>();
                foreach (IEnumerable<ProseHtmlNode> output in example.Value)
                {
                    var attrSets = output.Select(x => x.Attributes);
                    var resSet = new HashSet<string>(attrSets.First().Select(x => x.Name));
                    foreach(var attrSet in attrSets)
                    {
                        var set = new HashSet<string>(attrSet.Select(x => x.Name));
                        resSet.IntersectWith(set);
                    }
                    if (resSet.Count == 0)
                        return null;

                    possibilites.AddRange(resSet);
                }

                if (possibilites.Count == 0)
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        #endregion

        #region Descendants Attribute Value

        private CachedCalculation<Tuple<ProseHtmlNode, string, string>, List<Tuple<ProseHtmlNode, List<ProseHtmlNode>>>> descendentsByAttrValue
            = new CachedCalculation<Tuple<ProseHtmlNode, string, string>, List<Tuple<ProseHtmlNode, List<ProseHtmlNode>>>>(
                input => new[] { input.Item1 }.RecursiveSelect(x => x.ChildNodes)
                         .Select(x => Tuple.Create(x, Semantics.Descendants(x).Where(i => i[input.Item2]?.Value == input.Item3).ToList()))
                         .ToList()
            );

        [WitnessFunction(nameof(Semantics.DescendantsWithAttrValue), 0, DependsOnParameters = new[] { 1, 2 })]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttrValueSubseq(GrammarRule rule, DisjunctiveSubsequenceSpec spec, ExampleSpec tagSpec, ExampleSpec valueSpec)
        {
            // Basically the same thing as Descendants but for a single node now
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<IEnumerable<object>>> example in spec.Examples)
            {
                State inputState = example.Key;
                var attr = tagSpec.Examples[inputState] as string;
                var attrValue = valueSpec.Examples[inputState] as string;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var occList = new List<ProseHtmlNode>();
                foreach (IEnumerable<object> output in example.Value)
                {
                    var occurrences = from i in input.RecursiveSelect(x => x.ChildNodes)
                                      where Semantics.DescendantsWithAttrValue(i, attr, attrValue).ContainsSubsequence(output)
                                      select i;

                    occList.AddRange(occurrences);
                }
                
                if (occList.Count == 0) 
                    return null;
                result[inputState] = occList.Distinct().ToList();
            }
            //Console.WriteLine(descendentsByAttrValue);
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithAttrValue), 1)]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttrValue1Subseq(GrammarRule rule, DisjunctiveSubsequenceSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<IEnumerable<object>>> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };
                var possibilites = new List<string>();
                foreach (IEnumerable<object> output in example.Value)
                {
                    var attrSets = output.Cast<ProseHtmlNode>().Select(x => x.Attributes);
                    var resSet = new HashSet<string>(attrSets.First().Select(x => x.Name));
                    foreach(var attrSet in attrSets)
                    {
                        var set = new HashSet<string>(attrSet.Select(x => x.Name));
                        resSet.IntersectWith(set);
                    }
                    if (resSet.Count == 0)
                        return null;

                    possibilites.AddRange(resSet);
                }

                if (possibilites.Count == 0)
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithAttrValue), 2, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttrValue2Subseq(GrammarRule rule, DisjunctiveSubsequenceSpec spec, ExampleSpec tagSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<IEnumerable<object>>> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };
                var tag = tagSpec.Examples[inputState] as string;
                var possibilites = new List<string>();
                foreach (IEnumerable<object> output in example.Value)
                {
                    foreach(var o in output.Cast<ProseHtmlNode>())
                    {
                        if (o[tag] == null)
                            return null;

                        possibilites.Add(o[tag].Value.Value);
                    }
                }

                if (possibilites.Count == 0)
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithAttrValue), 0, DependsOnParameters = new[] { 1, 2 })]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttrValue(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec tagSpec, ExampleSpec valueSpec)
        {
            // Basically the same thing as Descendants but for a single node now
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var attr = tagSpec.Examples[inputState] as string;
                var attrValue = valueSpec.Examples[inputState] as string;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };

                var occList = new List<ProseHtmlNode>();
                foreach (IEnumerable<ProseHtmlNode> output in example.Value)
                {
                    var occurrences = from i in input.RecursiveSelect(x => x.ChildNodes)
                                      where Semantics.DescendantsWithAttrValue(i, attr, attrValue).SequenceEqual(output)
                                      select i;

                    occList.AddRange(occurrences);
                }
                
                if (occList.Count == 0) 
                    return null;
                result[inputState] = occList.Distinct().ToList();
            }
            Console.WriteLine(descendentsByAttrValue);
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithAttrValue), 1)]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttrValue1(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };
                var possibilites = new List<string>();
                foreach (IEnumerable<ProseHtmlNode> output in example.Value)
                {
                    var attrSets = output.Select(x => x.Attributes);
                    var resSet = new HashSet<string>(attrSets.First().Select(x => x.Name));
                    foreach(var attrSet in attrSets)
                    {
                        var set = new HashSet<string>(attrSet.Select(x => x.Name));
                        resSet.IntersectWith(set);
                    }
                    if (resSet.Count == 0)
                        return null;

                    possibilites.AddRange(resSet);
                }

                if (possibilites.Count == 0)
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.DescendantsWithAttrValue), 2, DependsOnParameters = new[] { 1 })]
        public DisjunctiveExamplesSpec WitnessDescendantsWithAttrValue2(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec tagSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var tag = tagSpec.Examples[inputState] as string;
                var input = new[] { inputState[Grammar.InputSymbol] as ProseHtmlNode };
                var possibilites = new List<string>();
                foreach (IEnumerable<ProseHtmlNode> output in example.Value)
                {
                    foreach(var o in output)
                    {
                        if (o[tag] == null)
                            return null;

                        possibilites.Add(o[tag].Value.Value);
                    }
                }

                if (possibilites.Count == 0)
                    return null;
                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        #endregion
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

        public static bool ContainsSubsequence<T>(this IEnumerable<T> parent, IEnumerable<T> target)
        {
            var pattern = target.ToArray();
            var source = new LinkedList<T>();

            foreach(var element in parent)
            {
                source.AddLast(element);
                if(source.Count == pattern.Length)
                {
                    if (source.SequenceEqual(pattern))
                        return true;
                    source.RemoveFirst();
                }
            }
            return false;
        }
    }
}