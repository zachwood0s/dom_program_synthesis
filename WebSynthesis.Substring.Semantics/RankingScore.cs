using System;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Features;

namespace WebSynthesis.Substring
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
        }

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double a, double b) => LikelihoodScore.Concat(a, b) + ReadabilityScore.Concat(a, b);

        [FeatureCalculator(nameof(Semantics.Split))]
        public static double Split(double v, double c) => LikelihoodScore.Split(v, c) + ReadabilityScore.Split(v, c);

        [FeatureCalculator(nameof(Semantics.SelectK))]
        public static double SelectK(double list, double k) => LikelihoodScore.SelectK(list, k) + ReadabilityScore.SelectK(list, k);

        [FeatureCalculator(nameof(Semantics.SelectRegex))]
        public static double SelectRegex(double list, double r) => LikelihoodScore.SelectRegex(list, r) + ReadabilityScore.SelectRegex(list, r);

        [FeatureCalculator(nameof(Semantics.TakeFirst))]
        public static double TakeFirst(double list) => LikelihoodScore.TakeFirst(list) + ReadabilityScore.TakeFirst(list);

        [FeatureCalculator(nameof(Semantics.JoinList))]
        public static double JoinList(double list, double c) => LikelihoodScore.JoinList(list, c) + ReadabilityScore.JoinList(list, c);

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k) => LikelihoodScore.K(k) + ReadabilityScore.K(k);


        [FeatureCalculator("c", Method = CalculationMethod.FromLiteral)]
        public static double C(char c) => LikelihoodScore.C(c) + ReadabilityScore.C(c);


        [FeatureCalculator("r", Method = CalculationMethod.FromLiteral)]
        public static double R(Regex r) => LikelihoodScore.R(r) + ReadabilityScore.R(r);
    }

    public class LikelihoodScore : Feature<double>
    {
        public LikelihoodScore(Grammar grammar) : base(grammar, "Score")
        {
        }

        private const int stronglyDiscourage = -10;
        private const int discourage = -2;
        private const int encourage = 2;
        private const int stronglyEncourage = 10;

        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double a, double b) => a + b;

        [FeatureCalculator(nameof(Semantics.Split))]
        public static double Split(double v, double c) => v + c;

        [FeatureCalculator(nameof(Semantics.SelectK))]
        public static double SelectK(double list, double k) => list + k + discourage;

        [FeatureCalculator(nameof(Semantics.SelectRegex))]
        public static double SelectRegex(double list, double r) => list + r;

        [FeatureCalculator(nameof(Semantics.TakeFirst))]
        public static double TakeFirst(double list) => list - 1;

        [FeatureCalculator(nameof(Semantics.JoinList))]
        public static double JoinList(double list, double c) => list + c + discourage;

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k) => k != 0 ? 1 / k : 0;

        [FeatureCalculator("c", Method = CalculationMethod.FromLiteral)]
        public static double C(char c) => 1;

        [FeatureCalculator("r", Method = CalculationMethod.FromLiteral)]
        public static double R(Regex r) => 1;

    }

    public class ReadabilityScore : Feature<double>
    {
        public ReadabilityScore(Grammar grammar) : base(grammar, "Score")
        {
        }

        private const int depthPenalty = -10;

        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double a, double b) => a + b + depthPenalty;

        [FeatureCalculator(nameof(Semantics.Split))]
        public static double Split(double v, double c) => v + c + depthPenalty;

        [FeatureCalculator(nameof(Semantics.SelectK))]
        public static double SelectK(double list, double k) => list + k + depthPenalty;

        [FeatureCalculator(nameof(Semantics.SelectRegex))]
        public static double SelectRegex(double list, double r) => list + r + depthPenalty;

        [FeatureCalculator(nameof(Semantics.TakeFirst))]
        public static double TakeFirst(double list) => list + depthPenalty;

        [FeatureCalculator(nameof(Semantics.JoinList))]
        public static double JoinList(double list, double c) => list + c + depthPenalty;

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k) => 0;

        [FeatureCalculator("c", Method = CalculationMethod.FromLiteral)]
        public static double C(char c) => 0;

        [FeatureCalculator("r", Method = CalculationMethod.FromLiteral)]
        public static double R(Regex r) => 0;

    }
}