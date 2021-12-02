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
        private const string _GrammarPath = @"WebSynthesis.TreeManipulation.grammar";
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

        [TestMethod]
        public void TestEducationWebpage()
        {
            testObject.CreateExample(
                "https://www.cs.purdue.edu/people/faculty/clifton.html",

                //Expected Output
                "Education",
                "PhD, Princeton University, Computer Science  (1991) ");

            testObject.CreateExample(
                "https://www.cs.purdue.edu/people/faculty/chjung.html",

                //Expected Output
                "Education",
                "PhD, Georgia Institute of Technology, Computer Science  (2013) ");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestTOC()
        {
            testObject.CreateExample("https://en.wikipedia.org/wiki/Program_synthesis",
                "<span class='toctext'>Origin</span>", 
                "<span class='toctext'>21st century developments</span>", 
                "<span class='toctext'>The framework of Manna and Waldinger</span>",
                "<span class='toctext'>Proof rules</span>", 
                "<span class='toctext'>Example</span>", 
                "<span class='toctext'>See also</span>", 
                "<span class='toctext'>Notes</span>", 
                "<span class='toctext'>References</span>"
            );
            testObject.RunTest();
        }
    }
}
