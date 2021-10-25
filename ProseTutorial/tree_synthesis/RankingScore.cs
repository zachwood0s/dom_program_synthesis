using System;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Features;
using Microsoft.ProgramSynthesis.Wrangling.Tree;
using Microsoft.ProgramSynthesis.Rules.Concepts;
using Microsoft.ProgramSynthesis.AST;

namespace TreeManipulation
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
            //Microsoft.ProgramSynthesis.Rules.Concepts.
        }
        private const int discourage = -20;
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double a, double b) => a + b + discourage;

        [FeatureCalculator(nameof(Semantics.Children))]
        public static double Children(double node) => node + 1; // Favor children over descendants

        [FeatureCalculator(nameof(Semantics.Descendants))]
        public static double Descendants(double node) => node; 

        [FeatureCalculator(nameof(Semantics.Single))]
        public static double Single(double node) => node;

        [FeatureCalculator("SelectChild")]
        public static double SelectChild(double x, double k) => x + k;

        [FeatureCalculator("MatchNodes")]
        public static double MatchNodes(double match, double rule) => match + rule; // Worse than just running the rule by itself

        [FeatureCalculator(nameof(Semantics.MatchTag))]
        public static double MatchTag(double node, double tag) => node + tag;

        [FeatureCalculator(nameof(Semantics.MatchAttribute))]
        public static double MatchAttribute(double node, double attr) => node + attr;

        [FeatureCalculator(nameof(Semantics.True))]
        public static double True() => discourage;

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double ScoreK(int k) => k != 0 ? 1.0/k : 0;

        [FeatureCalculator("tag", Method = CalculationMethod.FromLiteral)]
        public static double ScoreTag(string tag) => 0;

        [FeatureCalculator("attr", Method = CalculationMethod.FromLiteral)]
        public static double ScoreAttr(string attr) => 0;

        [FeatureCalculator("value", Method = CalculationMethod.FromLiteral)]
        public static double ScoreValue(string value) => 0;



    }
}