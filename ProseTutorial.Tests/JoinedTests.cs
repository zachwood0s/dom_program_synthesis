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
        private const string _GrammarPath = @"../../../../ProseTutorial/joined_synthesis/grammar/joined.grammar";
        private static WebscrapeTestObject testObject;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            // Load substring
            var substringGrammar = Utils.LoadGrammar("WebSynthesis.Substring.grammar", 
                CompilerReference.FromAssemblyFiles(typeof(Substring.Semantics).GetTypeInfo().Assembly));


            var treeGrammar = Utils.LoadGrammar("WebSynthesis.TreeManipulation.grammar",
                CompilerReference.FromAssemblyFiles(typeof(TreeManipulation.Semantics).GetTypeInfo().Assembly));

            var joinedGrammar = Utils.LoadGrammar("WebSynthesis.Joined.grammar",
                CompilerReference.FromAssemblyFiles(typeof(Joined.Semantics).GetTypeInfo().Assembly,
                                                    typeof(TreeManipulation.Semantics).GetTypeInfo().Assembly,
                                                    typeof(Substring.Semantics).GetTypeInfo().Assembly,
                                                    typeof(TreeManipulation.Language).GetTypeInfo().Assembly,
                                                    typeof(Substring.Language).GetTypeInfo().Assembly));

            testObject = new WebscrapeTestObject("WebSynthesis.Joined.grammar");

            testObject.Init(
                g => new RankingScore(g),
                g => new WitnessFunctions(g),
                typeof(Semantics).GetTypeInfo().Assembly,
                typeof(WebSynthesis.Substring.Semantics).GetTypeInfo().Assembly,
                typeof(WebSynthesis.TreeManipulation.Semantics).GetTypeInfo().Assembly
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