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
    public class TreeManipTest
    {
        private const string _GrammarPath = @"../../../../ProseTutorial/tree_synthesis/grammar/treemanim.grammar";
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
        public void TestLearnChildren()
        {
            testObject.CreateExample(
                Html("<parent><child1/><child2/></parent>"),

                // Expected results
                Html("<child1/>"),
                Html("<child2/>")
                );

            testObject.CreateTestCase(
                Html("<parent><child1/><child2><child3/></child2></parent>"),

                // Expected results
                Html("<child1/>"),
                Html("<child2><child3/></child2>")
                );

            testObject.RunTest();

        }

        [TestMethod]
        public void TestLearnDescendants()
        {
            /*
            testObject.CreateExample(
                Html("<parent><child1/><child2/></parent>"),
                
                // Expected results
                Html("<child1/>"),
                Html("<child2/>")
                );
                */

            testObject.CreateExample(
                Html("<parent><child1/><child2><child3/></child2></parent>"),

                // Expected results
                Html("<child1/>"),
                Html("<child2><child3/></child2>"),
                Html("<child3/>"));


            testObject.CreateTestCase(
                Html("<parent><child1><child3/></child1><child2/></parent>"),

                // Expected results
                Html("<child1><child3/></child1>"),
                Html("<child3/>"),
                Html("<child2/>")); 

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnKthChild()
        {
            testObject.CreateExample(
                Html("<parent><child1/><childImportant><child3/></childImportant><childImportant id=\"hi\"/></parent>"),

                // Expected results
                Html("<childImportant><child3/></childImportant>"));

            testObject.CreateExample(
                Html("<parent><child1><child3/></child1><secondChild/></parent>"),

                // Expected results
                Html("<secondChild>"));

            testObject.RunTest();
        }
        [TestMethod]
        public void TestLearnKthChildChildren()
        {
            testObject.CreateExample(
                Html("<parent><child1/><child2><this/><that/></child2></parent>"),

                // Expected results
                Html("<this/>"),
                Html("<that/>"));

            testObject.CreateExample(
                Html("<parent><no/><p><yes1/><yes2/></p></parent>"),

                // Expected results
                Html("<yes1>"),
                Html("<yes2>"));

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnRecursiveKthChildren()
        {
            testObject.CreateExample(
                Html("<parent><child1/><child2><this/><that/></child2></parent>"),

                // Expected results
                Html("<this/>"));
                //Html("<that/>"));

            testObject.CreateExample(
                Html("<parent><child1><none1/></child1><secondChild><special/><that/></secondChild><none2/></parent>"),

                // Expected results
                Html("<special/>"));

            testObject.CreateTestCase(
                Html("<parent><no/><p><yes1/><notThis/></p></parent>"),

                // Expected results
                Html("<yes1>"));

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnKthChildByTag()
        {
            // Failing, expect something like:
            // Single(SelectChild(MatchNodes(MatchTag(x, "child1"), Descendants(tree)), 0));
            testObject.CreateExample(
                Html(@"<parent>
                        <childNot/>
                        <child1/>
                        <child1 id='this one'/>
                        <child3/>
                       </parent>"),

                // Expected results
                Html("<child1 id='this one'/>"));

            testObject.CreateExample(
                Html(@"<parent>
                        <nope>
                            <childNot/>
                            <child1 id='first'/>
                        </nope>
                        <firstChild/>
                        <child1 class='that one'/>
                       </parent>"),

                // Expected results
                Html("<child1 class='that one'/>"));

            testObject.CreateTestCase(
                Html("<parent><child2/><nope><nope><child1/><child1 id='last'/></nope></nope></parent>"),

                // Expected results
                Html("<child1 id='last'/>"));

            testObject.RunTest();


        }

        [TestMethod]
        public void TestLearnMatchTag()
        {
            testObject.CreateExample(
                Html("<parent><special/><child2/><special id='hi'/></parent>"),

                // Expected results
                Html("<special/>"),
                Html("<special id='hi'/>"));

            testObject.CreateExample(
                Html("<parent><child2><special/></child2><special id='hi'/><special id='hi2'/></parent>"),

                // Expected results
                Html("<special/>"),
                Html("<special id='hi'/>"),
                Html("<special id='hi2'/>"));

            testObject.CreateTestCase(
                Html("<parent><child1><child3/><special/></child1><child2/></parent>"),

                // Expected results
                Html("<special/>"));

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnConcat()
        {
            testObject.CreateExample(
                Html("<parent><special1/><child2/><special2/></parent>"),

                // Expected results
                Html("<special1/>"),
                Html("<special2/>"));

            testObject.CreateExample(
                Html("<parent><child1><child3/><special/></child1><child2/><child4/></parent>"),

                // Expected results
                Html("<child1><child3/><special/></child1>"),
                Html("<child4/>"));

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnAttribute()
        {
            // Searching for nodes with an "id" attribute
            testObject.CreateExample(
                Html("<parent><special1 wrong='wrong'/><child2 id='hello'/><special2 id='goodbye' notImportant='eh'/></parent>"),

                // Expected
                Html("<child2 id='hello'/>"),
                Html("<special2 id='goodbye' notImportant='eh'/>"));

            testObject.CreateExample(
                Html("<parent><special1 id='hello'/><child2 id='hello'/><special2 id='goodbye' notImportant='eh'/></parent>"),

                Html("<special1 id='hello'/>"),
                Html("<child2 id='hello'/>"),
                Html("<special2 id='goodbye' notImportant='eh'/>"));

            testObject.CreateTestCase(
                Html("<parent><child1 wrong='wrong'><child3/><special wrong='wrong'/></child1><child2 id='hello'/><child4/></parent>"),

                Html("<child2 id='hello'/>"));

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnKthChildByAttribute()
        {
            // Searching for nodes with an "id" attribute
            testObject.CreateExample(
                Html("<parent><special1 wrong='wrong'/><child2 id='hello'/><special2 id='goodbye' notImportant='eh'/></parent>"),

                // Expected
                Html("<special2 id='goodbye' notImportant='eh'/>"));

            testObject.CreateExample(
                Html("<parent><special1 id='hello'/><child2 id='hello'/><special2 id='goodbye' notImportant='eh'/></parent>"),

                Html("<child2 id='hello'/>"));

            testObject.CreateTestCase(
                Html("<parent><child1 wrong='wrong'><child3/><special wrong='wrong'/></child1><notThis id='no'/><child2 id='hello'/><child4/></parent>"),

                Html("<child2 id='hello'/>"));

            testObject.RunTest();
        }

    }
}