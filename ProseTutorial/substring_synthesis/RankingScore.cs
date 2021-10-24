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

        [FeatureCalculator(nameof(Semantics.SubstringPP))]
        public static double SubstringPP(double s, double start, double end)
        {
            return start * end;
        }

        [FeatureCalculator(nameof(Semantics.SubstringPL))]
        public static double SubstringPL(double s, double pos, double l)
        {
            return pos * l;
        }

        [FeatureCalculator(nameof(Semantics.Date))]
        public static double Date(double s)
        {
            return s * s    ;
        }

        [FeatureCalculator(nameof(Semantics.AbsPos))]
        public static double AbsPos(double s, double k)
        {
            return k;
        }

        [FeatureCalculator(nameof(Semantics.RelPosLeft))]
        public static double RelPosLeft(double s, double r)
        {
            return r;
        }

        [FeatureCalculator(nameof(Semantics.RelPosRight))]
        public static double RelPosRight(double s, double r)
        {
            return r;
        }

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k)
        {
            return 1.0 / Math.Abs(k);
        }

        [FeatureCalculator("l", Method = CalculationMethod.FromLiteral)]
        public static double L(int l)
        {
            return 1.0 / Math.Abs(l);
        }

        [FeatureCalculator("r", Method = CalculationMethod.FromLiteral)]
        public static double R(Regex regex)
        {
            return 1;
        }
    }
}