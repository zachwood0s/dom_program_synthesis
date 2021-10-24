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
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.Children))]
        public static double Children(double node) => node + 1; // Favor children over descendants

        [FeatureCalculator(nameof(Semantics.Descendants))]
        public static double Descendants(double node) => node; 

        [FeatureCalculator(nameof(Semantics.Single))]
        public static double Single(double node) => node;

        [FeatureCalculator("SelectChild")]
        public static double SelectChild(double x, double k) => x + k;

        [FeatureCalculator("Selected")]
        public static double Selected(double match, double rule) => match + rule;

        [FeatureCalculator(nameof(Semantics.MatchTag))]
        public static double MatchTag(double node, double tag) => node + tag;

        [FeatureCalculator(nameof(Semantics.True))]
        public static double True() => 0;

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double ScoreK(int k) => 1.0/k;

        [FeatureCalculator("tag", Method = CalculationMethod.FromLiteral)]
        public static double ScoreTag(string tag) => 0;

    }
}