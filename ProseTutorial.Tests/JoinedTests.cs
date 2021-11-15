using System;
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
                typeof(Substring.Language).GetTypeInfo().Assembly
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
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/bgstm.html", "Tony Bergstrom");
            /*
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/chjung.html", "Changhee Jung", "Associate Professor in Computer Science");
            testObject.CreateExample("https://www.cs.purdue.edu/people/faculty/bgstm.html", "Tony Bergstrom", "Assistant Professor of Practice");
            */

            testObject.RunTest();
        }

    }
}