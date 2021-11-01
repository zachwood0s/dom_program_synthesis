using Microsoft.ProgramSynthesis.Wrangling.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Tests.Utils;

namespace TreeManipulation
{

    [TestClass]
    public class WebscrapeTreeTests
    {
        private const string _GrammarPath = @"../../../../ProseTutorial/tree_synthesis/grammar/treemanim.grammar";
        private static WebscrapeTestObject testObject;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            testObject = new WebscrapeTestObject(_GrammarPath);

            testObject.Init(
                g => new RankingScore(g),
                g => new WitnessFunctions(g),
                typeof(Semantics).GetTypeInfo().Assembly,
                typeof(Node).GetTypeInfo().Assembly
                );
        }

        [TestCleanup]
        public void Cleanup()
        {
            testObject.Clear();
        }

        [TestMethod]
        public void TestSimpleWebpage()
        {
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/chjung.html", "out1", "out2");
        }
    }
}
