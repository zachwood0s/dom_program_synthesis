using System;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Features;
using Microsoft.ProgramSynthesis.Wrangling.Tree;
using Microsoft.ProgramSynthesis.Rules.Concepts;
using Microsoft.ProgramSynthesis.AST;

namespace WebSynthesis.TreeManipulation
{
    public class RankingScore : Feature<double>
    {
        private LikelihoodScore likelihood;
        private ReadabilityScore readability;
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
            likelihood = new LikelihoodScore(grammar);
            readability = new ReadabilityScore(grammar);
            //Microsoft.ProgramSynthesis.Rules.Concepts.
        }
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double a, double b) => LikelihoodScore.Concat(a, b) + ReadabilityScore.Concat(a, b);

        [FeatureCalculator(nameof(Semantics.Children))]
        public static double Children(double node) => LikelihoodScore.Children(node) + ReadabilityScore.Children(node); // Favor children over descendants

        [FeatureCalculator(nameof(Semantics.Descendants))]
        public static double Descendants(double node) => LikelihoodScore.Descendants(node) + ReadabilityScore.Descendants(node); 

        [FeatureCalculator(nameof(Semantics.DescendantsWithTag))]
        public static double DescendantsWithTag(double node, double tag) => LikelihoodScore.DescendantsWithTag(node, tag) + ReadabilityScore.DescendantsWithTag(node, tag); 

        [FeatureCalculator(nameof(Semantics.DescendantsWithAttr))]
        public static double DescendantsWithAttr(double node, double tag) => LikelihoodScore.DescendantsWithAttr(node, tag) + ReadabilityScore.DescendantsWithAttr(node, tag); 

        [FeatureCalculator(nameof(Semantics.DescendantsWithAttrValue), Method = CalculationMethod.FromChildrenNodes)]
        public double DescendantsWithAttrValue(NonterminalNode node, LiteralNode tag, LiteralNode value)
            => likelihood.DescendantsWithAttrValue(node, tag, value) + readability.DescendantsWithAttrValue(node, tag, value); 

        [FeatureCalculator(nameof(Semantics.Single))]
        public static double Single(double node) => LikelihoodScore.Single(node) + ReadabilityScore.Single(node);

        [FeatureCalculator("SelectChild")]
        public static double SelectChild(double x, double k) => LikelihoodScore.SelectChild(x, k) + ReadabilityScore.SelectChild(x, k);

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double ScoreK(int k) => LikelihoodScore.ScoreK(k) + ReadabilityScore.ScoreK(k);

        [FeatureCalculator("tag", Method = CalculationMethod.FromLiteral)]
        public static double ScoreTag(string tag) => LikelihoodScore.ScoreTag(tag) + ReadabilityScore.ScoreTag(tag);

        [FeatureCalculator("attr", Method = CalculationMethod.FromLiteral)]
        public static double ScoreAttr(string attr) => LikelihoodScore.ScoreAttr(attr) + ReadabilityScore.ScoreAttr(attr);

        [FeatureCalculator("value", Method = CalculationMethod.FromLiteral)]
        public static double ScoreValue(string value) => LikelihoodScore.ScoreValue(value) + ReadabilityScore.ScoreValue(value);
    }
    public class LikelihoodScore : Feature<double>
    {
        public LikelihoodScore(Grammar grammar) : base(grammar, "Score")
        {
            //Microsoft.ProgramSynthesis.Rules.Concepts.
        }

        private const int stronglyDiscourage = -10;
        private const int discourage = -2;
        private const int encourage = 2;
        private const int stronglyEncourage = 10;
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double a, double b) => a + b;

        [FeatureCalculator(nameof(Semantics.Children))]
        public static double Children(double node) => node; // Favor children over descendants

        [FeatureCalculator(nameof(Semantics.Descendants))]
        public static double Descendants(double node) => node - 1; 

        [FeatureCalculator(nameof(Semantics.DescendantsWithTag))]
        public static double DescendantsWithTag(double node, double tag) => node + tag; 

        [FeatureCalculator(nameof(Semantics.DescendantsWithAttr))]
        public static double DescendantsWithAttr(double node, double tag) => node + tag + discourage;

        [FeatureCalculator(nameof(Semantics.DescendantsWithAttrValue), Method = CalculationMethod.FromChildrenNodes)]
        public double DescendantsWithAttrValue(NonterminalNode node, LiteralNode tag, LiteralNode value)
        {
            if((string) tag.Value == "style")
            {
                // Strongly strongly discourage the use of style as an attribute
                return node.GetFeatureValue(this) + tag.GetFeatureValue(this) + value.GetFeatureValue(this) - 100;
            }
            else
            {
                // All other attribute values are good
                return node.GetFeatureValue(this) + tag.GetFeatureValue(this) + value.GetFeatureValue(this) + encourage;
            }
        }

        [FeatureCalculator(nameof(Semantics.Single))]
        public static double Single(double node) => node - 1;

        [FeatureCalculator("SelectChild")]
        public static double SelectChild(double x, double k) => x + k + discourage;

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double ScoreK(int k) => k != 0 ? 1 / k : 0;

        [FeatureCalculator("tag", Method = CalculationMethod.FromLiteral)]
        public static double ScoreTag(string tag) => 1 / tag.Length;

        [FeatureCalculator("attr", Method = CalculationMethod.FromLiteral)]
        public static double ScoreAttr(string attr) => 1 / attr.Length;

        [FeatureCalculator("value", Method = CalculationMethod.FromLiteral)]
        public static double ScoreValue(string value) => 1 / value.Length;
    }
    public class ReadabilityScore : Feature<double>
    {
        public ReadabilityScore(Grammar grammar) : base(grammar, "Score")
        {
            //Microsoft.ProgramSynthesis.Rules.Concepts.
        }
        private const int depthPenalty = -10;
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double a, double b) => a + b + depthPenalty;

        [FeatureCalculator(nameof(Semantics.Children))]
        public static double Children(double node) => node + depthPenalty; // Favor children over descendants

        [FeatureCalculator(nameof(Semantics.Descendants))]
        public static double Descendants(double node) => node + depthPenalty; 

        [FeatureCalculator(nameof(Semantics.DescendantsWithTag))]
        public static double DescendantsWithTag(double node, double tag) => node + tag + depthPenalty; 

        [FeatureCalculator(nameof(Semantics.DescendantsWithAttr))]
        public static double DescendantsWithAttr(double node, double tag) => node + tag + depthPenalty; 

        [FeatureCalculator(nameof(Semantics.DescendantsWithAttrValue), Method = CalculationMethod.FromChildrenNodes)]
        public double DescendantsWithAttrValue(NonterminalNode node, LiteralNode tag, LiteralNode value)
            => node.GetFeatureValue(this) + tag.GetFeatureValue(this) + value.GetFeatureValue(this) + depthPenalty; 

        [FeatureCalculator(nameof(Semantics.Single))]
        public static double Single(double node) => node + depthPenalty;

        [FeatureCalculator("SelectChild")]
        public static double SelectChild(double x, double k) => x + k + depthPenalty;

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double ScoreK(int k) => 0;

        [FeatureCalculator("tag", Method = CalculationMethod.FromLiteral)]
        public static double ScoreTag(string tag) => 0;

        [FeatureCalculator("attr", Method = CalculationMethod.FromLiteral)]
        public static double ScoreAttr(string attr) => 0;

        [FeatureCalculator("value", Method = CalculationMethod.FromLiteral)]
        public static double ScoreValue(string value) => 0;
    }
}