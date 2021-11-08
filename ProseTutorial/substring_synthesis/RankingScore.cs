using System;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Features;

namespace SubstringSynthesis
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
        }

        [FeatureCalculator(nameof(Semantics.Concat))]
        public static double Concat(double l1, double l2)
        {
            return l1 * l2;
        }

        [FeatureCalculator(nameof(Semantics.Split))]
        public static double Split(double v, double c)
        {
            return v * c;
        }

        [FeatureCalculator(nameof(Semantics.SelectK))]
        public static double SelectK(double list, double k)
        {
            return list * k;
        }

        [FeatureCalculator(nameof(Semantics.SelectRegex))]
        public static double SelectRegex(double list, double r)
        {
            return list * r;
        }

        [FeatureCalculator(nameof(Semantics.TakeFirst))]
        public static double TakeFirst(double list)
        {
            return list;
        }

        [FeatureCalculator(nameof(Semantics.JoinList))]
        public static double JoinList(double list, double c)
        {
            return c;
        }

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k)
        {
            return 1.0 / Math.Abs(k);
        }

        [FeatureCalculator("c", Method = CalculationMethod.FromLiteral)]
        public static double C(char c)
        {
            return 1;
        }

        [FeatureCalculator("r", Method = CalculationMethod.FromLiteral)]
        public static double R(Regex r)
        {
            return 1;
        }
    }
}