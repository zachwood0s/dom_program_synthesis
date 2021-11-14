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
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
            //Microsoft.ProgramSynthesis.Rules.Concepts.
        }
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double a, double b) => LikelihoodScore.Concat(a, b) + ReadabilityScore.Concat(a, b);

        [FeatureCalculator(nameof(Semantics.Children))]
        public static double Children(double node) => LikelihoodScore.Children(node) + ReadabilityScore.Children(node); // Favor children over descendants

        [FeatureCalculator(nameof(Semantics.Descendants))]
        public static double Descendants(double node) => LikelihoodScore.Descendants(node) + ReadabilityScore.Descendants(node); 

        [FeatureCalculator(nameof(Semantics.Single))]
        public static double Single(double node) => LikelihoodScore.Single(node) + ReadabilityScore.Single(node);

        [FeatureCalculator("SelectChild")]
        public static double SelectChild(double x, double k) => LikelihoodScore.SelectChild(x, k) + ReadabilityScore.SelectChild(x, k);

        [FeatureCalculator("MatchNodes")]
        public static double MatchNodes(double match, double rule) => LikelihoodScore.MatchNodes(match, rule) + ReadabilityScore.MatchNodes(match, rule); 

        [FeatureCalculator(nameof(Semantics.MatchTag))]
        public static double MatchTag(double node, double tag) => LikelihoodScore.MatchTag(node, tag) + ReadabilityScore.MatchTag(node, tag); 

        [FeatureCalculator(nameof(Semantics.MatchAttribute))]
        public static double MatchAttribute(double node, double attr) => LikelihoodScore.MatchAttribute(node, attr) + ReadabilityScore.MatchAttribute(node, attr);

        [FeatureCalculator(nameof(Semantics.KthDescendantWithTag))]
        public static double FirstWithTag(double nodes, double tag, double k) => LikelihoodScore.FirstWithTag(nodes, tag, k) + ReadabilityScore.FirstWithTag(nodes, tag, k); 

        [FeatureCalculator(nameof(Semantics.True))]
        public static double True() => LikelihoodScore.True() + ReadabilityScore.True();

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

        [FeatureCalculator(nameof(Semantics.Single))]
        public static double Single(double node) => node - 1;

        [FeatureCalculator("SelectChild")]
        public static double SelectChild(double x, double k) => x + k + discourage;

        [FeatureCalculator("MatchNodes")]
        public static double MatchNodes(double match, double rule) => match + rule + encourage; // Probably generates a more general solution than indexes

        [FeatureCalculator(nameof(Semantics.MatchTag))]
        public static double MatchTag(double node, double tag) => node + tag;

        [FeatureCalculator(nameof(Semantics.MatchAttribute))]
        public static double MatchAttribute(double node, double attr) => node + attr;

        [FeatureCalculator(nameof(Semantics.KthDescendantWithTag))]
        public static double FirstWithTag(double nodes, double tag, double k) => nodes + tag + k + encourage;

        [FeatureCalculator(nameof(Semantics.True))]
        public static double True() => stronglyDiscourage;

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

        [FeatureCalculator(nameof(Semantics.Single))]
        public static double Single(double node) => node + depthPenalty;

        [FeatureCalculator("SelectChild")]
        public static double SelectChild(double x, double k) => x + k + depthPenalty;

        [FeatureCalculator("MatchNodes")]
        public static double MatchNodes(double match, double rule) => match + rule + depthPenalty; // Worse than just running the rule by itself

        [FeatureCalculator(nameof(Semantics.MatchTag))]
        public static double MatchTag(double node, double tag) => node + tag + depthPenalty;

        [FeatureCalculator(nameof(Semantics.MatchAttribute))]
        public static double MatchAttribute(double node, double attr) => node + attr + depthPenalty;

        [FeatureCalculator(nameof(Semantics.KthDescendantWithTag))]
        public static double FirstWithTag(double nodes, double tag, double k) => nodes + tag + k + depthPenalty;

        [FeatureCalculator(nameof(Semantics.True))]
        public static double True() => depthPenalty;

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