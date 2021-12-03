using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.DslLibrary;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using Microsoft.ProgramSynthesis.VersionSpace;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Utils;

namespace WebSynthesis.Comparison
{
    [TestClass]
    public class ComparisonTests
    {
        private static TestComparisonObject testObject;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            testObject = new TestComparisonObject();
        }

        [TestCleanup]
        public void Cleanup()
        {
            testObject.Clear();
        }

        [TestMethod]
        public void TestWikipediaTitles()
        {
            testObject.CreateExample("https://en.wikipedia.org/wiki/Program_synthesis", "Program synthesis");

            // Ten random wiki pages
            testObject.CreateTestCase("https://en.wikipedia.org/wiki/Lipovo,_Dobryanka,_Perm_Krai", "Lipovo, Dobryanka, Perm Krai");
            testObject.CreateTestCase("https://en.wikipedia.org/wiki/Family_tree_of_German_monarchs", "Family tree of German monarchs");
            testObject.CreateTestCase("https://en.wikipedia.org/wiki/Rugby_sevens_at_the_2018_Commonwealth_Games_%E2%80%93_Men%27s_tournament", "Rugby sevens at the 2018 Commonwealth Games – Men's tournament");
            testObject.CreateTestCase("https://en.wikipedia.org/wiki/National_Bank_of_China", "National Bank of China");
            testObject.CreateTestCase("https://en.wikipedia.org/wiki/Isabella_of_Aragon,_Duchess_of_Milan", "Isabella of Aragon, Duchess of Milan");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestWikipediaHeaders()
        {
            testObject.CreateExample("https://en.wikipedia.org/wiki/Israeli_Navy_Band", "History");
            testObject.CreateExample("https://en.wikipedia.org/wiki/Program_synthesis", "Origin");


            testObject.RunTest();
        }

        [TestMethod]
        public void TestWikipediaCapitals()
        {
            testObject.CreateExample("https://en.wikipedia.org/wiki/Colorado", "Denver");
            testObject.CreateExample("https://en.wikipedia.org/wiki/Kansas", "Topeka");

            testObject.RunTest();
        }
    }
}