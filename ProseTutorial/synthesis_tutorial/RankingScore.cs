using System;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Features;

namespace ProseTutorial
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
        }

        [FeatureCalculator(nameof(Semantics.Cat))]
        public static double Cat(double v, double a)
        {
            return 1;
        }

        [FeatureCalculator(nameof(Semantics.Substring))]
        public static double Substring(double v, double start, double end)
        {
            return start * end;
        }

        [FeatureCalculator(nameof(Semantics.AbsPos))]
        public static double AbsPos(double v, double k)
        {
            return k;
        }

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k)
        {
            return 1.0 / Math.Abs(k);
        }

    }
}