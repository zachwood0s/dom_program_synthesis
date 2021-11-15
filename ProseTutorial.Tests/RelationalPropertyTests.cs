using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using Microsoft.ProgramSynthesis.Wrangling.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Utils;

namespace WebSynthesis.TreeManipulation
{
    [TestClass]
    public class RelationalPropertyTests
    {
        private const string _GrammarPath = @"WebSynthesis.TreeManipulation.grammar";
        private static HtmlSequenceTestObject testObject;

        private static StructNode TN(string label)
        {
            return TN(label, new Attributes());
        }

        private static StructNode TN(string label, params StructNode[] children)
        {
            return TN(label, new Attributes(), children);
        }

        private static StructNode TN(string label, Attributes attributes, params StructNode[] children)
        {
            var n = StructNode.Create(label, attributes, children);
            foreach (var c in children)
                c.Parent = n;
            return n;
        }

        private static ProseHtmlNode Html(string htmlText)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlText);
            return ProseHtmlNode.DeserializeFromHtmlNode(doc.DocumentNode.FirstChild);
        }

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            testObject = new HtmlSequenceTestObject(_GrammarPath);

            testObject.Init(
                g => new RankingScore(g), 
                g => new WitnessFunctions(g),
                typeof(Semantics).GetTypeInfo().Assembly,
                typeof(ProseHtmlNode).GetTypeInfo().Assembly
                ) ;
        }

        [TestCleanup]
        public void Cleanup()
        {
            testObject.Clear();
        }

        [TestMethod]
        public void TestLearnChildOrderInvariance()
        {
            //Naive solution is select the first child but child order invariance should apply and force more general requirements.
            testObject.CreateExample(
                Html("<parent><special/><child2/><child3/></parent>"),

                // Expected results
                Html("<special/>"));

            testObject.CreateTestCase(
                Html("<parent><special/><child2/></parent>"),

                // Expected results
                Html("<special/>"));

            testObject.RunTest();
        }


    }
}