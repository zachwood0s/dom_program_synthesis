using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using WebSynthesis.TreeManipulation;

namespace WebSynthesis.Joined
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public WitnessFunctions(Grammar grammar) : base(grammar)
        {
        }
        [WitnessFunction("JoinMap", 1)]
        public DisjunctiveExamplesSpec WitnessLinesMap(GrammarRule rule, ExampleSpec spec)
        {
            var linesExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var possibleNodesForEachText = new List<List<string>>();
                var tree = input[rule.Grammar.InputSymbol] as ProseHtmlNode;
                var selections = spec.Examples[input] as IEnumerable<string>;

                var allNodes = new[] { tree }.RecursiveSelect(x => x.ChildNodes);
                foreach (string example in selections)
                {
                    var nodeTexts = new List<string>();
                    foreach (var n in allNodes)
                    {
                        if (n.Text != null && n.Text.Contains(example))
                        {
                            nodeTexts.Add(n.Text);
                        }
                    }
                    possibleNodesForEachText.Add(nodeTexts);
                }

                /*
                var selectionPrefix = spec.PositiveExamples[input].Cast<StringRegion>();

                foreach (StringRegion example in selectionPrefix)
                {
                    var startLine = GetLine(document, example.Start);
                    var endLine = GetLine(document, example.End);
                    if (startLine == null || endLine == null || startLine != endLine)
                        return null;
                    linesContainingSelection.Add(startLine);
                }
                */

                var combos = GetAllPossibleCombos(possibleNodesForEachText);
                linesExamples[input] = combos.Select(x => x.ToList()).ToList();
            }
            return new DisjunctiveExamplesSpec(linesExamples);
        }

        [WitnessFunction(nameof(Semantics.NodesToStrs), 0)]
        public DisjunctiveExamplesSpec WitnessNodesToStrs(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, IEnumerable<object>> example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var tree = inputState[rule.Grammar.InputSymbol] as ProseHtmlNode;
                var possibilites = new List<List<ProseHtmlNode>>();

                var allNodes = new[] { tree }.RecursiveSelect(x => x.ChildNodes).ToList();
                foreach(IEnumerable<string> strings in example.Value)
                {
                    var nodeList = new List<ProseHtmlNode>();
                    foreach(var str in strings)
                    {
                        var found = allNodes.Where(x => x.Text == str).FirstOrDefault();
                        if (found == null)
                            return null;

                        nodeList.Add(found);
                    }

                    possibilites.Add(nodeList);
                }

                result[inputState] = possibilites.Distinct().ToList();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [ExternLearningLogicMapping("StringSelection")]
        public DomainLearningLogic ExternWitnessFunctionString()
        {
            return new Substring.WitnessFunctions(Grammar.GrammarReferences["Substring"]);
        }

        [ExternLearningLogicMapping("NodeSelection")]
        public DomainLearningLogic ExternWitnessFunctionNode
            => new TreeManipulation.WitnessFunctions(Grammar.GrammarReferences["Tree"]);


        private static IEnumerable<IEnumerable<T>> GetAllPossibleCombos<T>(IEnumerable<IEnumerable<T>> options)
        {
            IEnumerable<IEnumerable<T>> combos = new T[][] { new T[0] };
            foreach(var inner in options)
            {
                combos = from c in combos
                         from i in inner
                         select c.Append(i);
            }
            return combos;
        }
    }
}