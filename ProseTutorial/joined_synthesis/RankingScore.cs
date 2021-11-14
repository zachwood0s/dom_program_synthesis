using System;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Features;

namespace WebSynthesis.Joined
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
        }

        [FeatureCalculator(nameof(Semantics.NodesToStrs))]
        public static double NodesToStrs(double nodes)
        {
            return nodes;
        }

        [FeatureCalculator(nameof(Semantics.StrToTree))]
        public static double StrToTree(double url)
        {
            return url;
        }

        //[FeatureCalculator("c", Method = CalculationMethod.FromLiteral)]
        public static double C(char c)
        {
            return 1;
        }
    }
}