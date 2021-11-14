using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Utils;

namespace WebSynthesis.Joined
{
    [TestClass]
    public class JoinedTests
    {
        private const string _GrammarPath = @"../../../../ProseTutorial/joined_synthesis/grammar/joined.grammar";
        private static WebscrapeTestObject testObject;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            testObject = new WebscrapeTestObject(_GrammarPath);

            testObject.Init(
                g => new RankingScore(g),
                g => new WitnessFunctions(g),
                typeof(Semantics).GetTypeInfo().Assembly
                );
        }

        [TestCleanup]
        public void Cleanup()
        {
            testObject.Clear();
        }

        [TestMethod]
        public void TestPrefix()
        {
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/chjung.html", "Changhee Jung");

            testObject.RunTest();
        }
    }
}