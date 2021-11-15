using Microsoft.ProgramSynthesis.Wrangling.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Tests.Utils;

namespace WebSynthesis.TreeManipulation
{

    [TestClass]
    public class WebscrapeTreeTests
    {
        private const string _GrammarPath = @"../../../../ProseTutorial/TreeManipulation/grammar/treemanim.grammar";
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
        public void TestSimpleWebpage()
        {
            testObject.CreateExample(
                "https://www.cs.purdue.edu/people/faculty/chjung.html",

                // Expected Output
                "<h1>Changhee Jung</h1>",
                "<h3 style=\"color: #000;\">Associate Professor in Computer Science</h3>");

            testObject.CreateExample(
                "https://www.cs.purdue.edu/people/faculty/bgstm.html",

                // Expected Output
                "<h1>Tony Bergstrom</h1>",
                "<h3 style=\"color: #000;\">Assistant Professor of Practice</h3>");

            testObject.CreateTestCase(
                "https://www.cs.purdue.edu/people/faculty/clifton.html",

                // Expected Output
                "<h1>Christopher W. Clifton</h1>",
                "<h3 style=\"color: #000;\">Professor of Computer Science</h3>");

            testObject.RunTest();
        }
    }
}
