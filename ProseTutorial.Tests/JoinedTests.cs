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

namespace WebSynthesis.Joined
{
    [TestClass]
    public class ComparisonTests
    {
        private static JoinedWebscrapeTestObject testObject;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            testObject = new JoinedWebscrapeTestObject("WebSynthesis.Joined.grammar");

            testObject.Init(
                g => new Joined.RankingScore(g),
                g => new Joined.WitnessFunctions(g),
                typeof(Joined.Semantics).GetTypeInfo().Assembly,
                typeof(TreeManipulation.Semantics).GetTypeInfo().Assembly,
                typeof(Substring.Semantics).GetTypeInfo().Assembly,
                typeof(TreeManipulation.Language).GetTypeInfo().Assembly,
                typeof(Substring.Language).GetTypeInfo().Assembly,
                typeof(StringRegion).GetTypeInfo().Assembly,
                typeof(Record).GetTypeInfo().Assembly
                );
        }

        [TestCleanup]
        public void Cleanup()
        {
            testObject.Clear();
        }

        [TestMethod]
        public void TestBasic()
        {
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/chjung.html", "Changhee Jung", "Associate Professor in Computer Science");
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/bgstm.html", "Tony Bergstrom", "Assistant Professor of Practice");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestEducation()
        {
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/clifton.html", "Education", "PhD, Princeton University, Computer Science (1991)");
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/chjung.html", "Education", "PhD, Georgia Institute of Technology, Computer Science (2013)");
            //testObject.CreateTestCase("https://www.cs.purdue.edu/people/faculty/bgstm.html", "PhD, University of Illinois at Urbana-Champaign, Computer Science (2011)");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestFinalPaperGrabPageTitle()
        {
            testObject.CreateExample("https://en.wikipedia.org/wiki/Program_synthesis", "Program synthesis");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestFinalPaperGrabPageSubHeaders()
        {
            testObject.CreateExample("https://en.wikipedia.org/wiki/Program_synthesis", 
                "Origin", "21st century developments", "The framework of Manna and Waldinger", 
                "Proof rules", "Example", "See also", "Notes", "References"
            );

            testObject.RunTest();
        }
    }
}