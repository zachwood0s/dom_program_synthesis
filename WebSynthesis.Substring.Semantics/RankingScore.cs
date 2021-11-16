using System;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.DslLibrary;
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

        [FeatureCalculator(nameof(Semantics.Substr))]
        public static double SubStr(double x, double pp) => LikelihoodScore.SubStr(x, pp) + ReadabilityScore.SubStr(x, pp);

        [FeatureCalculator(nameof(Semantics.ToString))]
        public static double ToString(double reg) => LikelihoodScore.ToString(reg) + ReadabilityScore.ToString(reg);

        [FeatureCalculator(nameof(Semantics.ToStringRegion))]
        public static double ToStringRegion(double str) => LikelihoodScore.ToStringRegion(str) + ReadabilityScore.ToStringRegion(str);

        [FeatureCalculator("PosPair")]
        public static double PosPair(double pp1, double pp2) => LikelihoodScore.PosPair(pp1, pp2) + ReadabilityScore.PosPair(pp1, pp2);

        [FeatureCalculator(nameof(Semantics.AbsPos))]
        public static double AbsPos(double x, double k) => LikelihoodScore.AbsPos(x, k) + ReadabilityScore.AbsPos(x, k);

        [FeatureCalculator(nameof(Semantics.RegPos))]
        public static double RegPos(double x, double rr, double k) => LikelihoodScore.RegPos(x, rr, k) + ReadabilityScore.RegPos(x, rr, k);

        [FeatureCalculator("BoundaryPair")]
        public static double BoundaryPair(double r1, double r2) => LikelihoodScore.BoundaryPair(r1, r2) + ReadabilityScore.BoundaryPair(r1, r2);

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k) => LikelihoodScore.K(k) + ReadabilityScore.K(k);

        [FeatureCalculator("c", Method = CalculationMethod.FromLiteral)]
        public static double C(char c) => LikelihoodScore.C(c) + ReadabilityScore.C(c);

        [FeatureCalculator("r", Method = CalculationMethod.FromLiteral)]
        public static double R(Regex r) => LikelihoodScore.R(r) + ReadabilityScore.R(r);

        [FeatureCalculator("re", Method = CalculationMethod.FromLiteral)]
        public static double RE(RegularExpression r) => LikelihoodScore.RE(r) + ReadabilityScore.RE(r);
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

        [FeatureCalculator(nameof(Semantics.Substr))]
        public static double SubStr(double x, double pp) => x + pp + discourage;

        [FeatureCalculator(nameof(Semantics.ToString))]
        public static double ToString(double reg) => reg;

        [FeatureCalculator(nameof(Semantics.ToStringRegion))]
        public static double ToStringRegion(double str) => str;

        [FeatureCalculator("PosPair")]
        public static double PosPair(double pp1, double pp2) => pp1 * pp2;

        [FeatureCalculator(nameof(Semantics.AbsPos))]
        public static double AbsPos(double x, double k) => x * k;

        [FeatureCalculator(nameof(Semantics.RegPos))]
        public static double RegPos(double x, double rr, double k) => rr * k;

        [FeatureCalculator("BoundaryPair")]
        public static double BoundaryPair(double r1, double r2) => r1 + r2;

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k) => k != 0 ? 1 / k : 0;

        [FeatureCalculator("c", Method = CalculationMethod.FromLiteral)]
        public static double C(char c) => 1;

        [FeatureCalculator("r", Method = CalculationMethod.FromLiteral)]
        public static double R(Regex r) => 1;

        [FeatureCalculator("re", Method = CalculationMethod.FromLiteral)]
        public static double RE(RegularExpression r) => 1;

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

        [FeatureCalculator(nameof(Semantics.Substr))]
        public static double SubStr(double x, double pp) => x + pp + depthPenalty;

        [FeatureCalculator(nameof(Semantics.ToString))]
        public static double ToString(double reg) => reg + depthPenalty;

        [FeatureCalculator(nameof(Semantics.ToStringRegion))]
        public static double ToStringRegion(double str) => str + depthPenalty;

        [FeatureCalculator("PosPair")]
        public static double PosPair(double pp1, double pp2) => pp1 * pp2 + depthPenalty;

        [FeatureCalculator(nameof(Semantics.AbsPos))]
        public static double AbsPos(double x, double k) => k + depthPenalty;

        [FeatureCalculator(nameof(Semantics.RegPos))]
        public static double RegPos(double x, double rr, double k) => rr * k + depthPenalty;

        [FeatureCalculator("BoundaryPair")]
        public static double BoundaryPair(double r1, double r2) => r1 + r2 + depthPenalty;

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k) => 0;

        [FeatureCalculator("c", Method = CalculationMethod.FromLiteral)]
        public static double C(char c) => 0;

        [FeatureCalculator("r", Method = CalculationMethod.FromLiteral)]
        public static double R(Regex r) => 0;

        [FeatureCalculator("re", Method = CalculationMethod.FromLiteral)]
        public static double RE(RegularExpression r) => 0;

    }
}