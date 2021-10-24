using System;
using System.Collections;
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
using Microsoft.ProgramSynthesis.Wrangling.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Utils;

namespace TreeManipulation
{
    [TestClass]
    public class TreeManipTest
    {
        private const string _GrammarPath = @"../../../../ProseTutorial/tree_synthesis/grammar/treemanim.grammar";
        private static SequenceTestObject<Node, Node> testObject;

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

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            testObject = new SequenceTestObject<Node, Node>(_GrammarPath);

            testObject.Init(
                g => new RankingScore(g),
                g => new WitnessFunctions(g),
                typeof(Semantics).GetTypeInfo().Assembly,
                typeof(Node).GetTypeInfo().Assembly
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
                TN("parent",
                    TN("child1"),
                    TN("child2")),

                // Expected results
                TN("child1"),
                TN("child2")
                );

            testObject.CreateTestCase(
                TN("parent",
                    TN("child1"),
                    TN("child2", 
                        TN("child3"))),

                // Expected results
                TN("child1"),
                TN("child2",
                    TN("child3"))
                );

            testObject.RunTest();

        }

        [TestMethod]
        public void TestLearnDescendants()
        {
            testObject.CreateExample(
                TN("parent",
                    TN("child1"),
                    TN("child2")),
                
                // Expected results
                TN("child1"),
                TN("child2")
                );

            testObject.CreateExample(
                TN("parent",
                    TN("child1"),
                    TN("child2",
                        TN("child3"))),
                
                // Expected results
                TN("child1"),
                TN("child2", TN("child3")),
                TN("child3")
                );


            testObject.CreateTestCase(
                TN("parent",
                        TN("child1",
                            TN("child3")),
                        TN("child2")),

                // Expected results
                TN("child1", TN("child3")),
                TN("child3"),
                TN("child2")
                );

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnKthChild()
        {
            testObject.CreateExample(
                TN("parent",
                    TN("child1"),
                    TN("child2")),

                // Expected results
                TN("child2")
                );

            testObject.CreateExample(
                TN("parent",
                    TN("child1"),
                    TN("child2",
                        TN("child3"))),

                // Expected results
                TN("child2", TN("child3"))
                );

            testObject.CreateTestCase(
                TN("parent",
                    TN("child1", 
                        TN("child3")),
                    TN("secondChild")),

                // Expected results
                TN("secondChild"));

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnMatchTag()
        {
            testObject.CreateExample(
                TN("parent",
                    TN("special"),
                    TN("child2")),

                TN("special")
                );

            testObject.CreateExample(
                TN("parent",
                    TN("child2",
                        TN("special"))),

                TN("special")
                );

            testObject.CreateTestCase(
                TN("parent",
                    TN("child1", 
                        TN("child3"),
                        TN("special")),
                    TN("child2")),

                TN("special")
                );

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLearnConcat()
        {
            testObject.CreateExample(
                TN("parent",
                    TN("special1"),
                    TN("child2"),
                    TN("special2")),

                TN("special1"),
                TN("special2")
                );

            testObject.CreateTestCase(
                TN("parent",
                    TN("child1", 
                        TN("child3"),
                        TN("special")),
                    TN("child2"),
                    TN("child3")),

                TN("child1", 
                    TN("child3"),
                    TN("special")),
                TN("child3")
                );

            testObject.RunTest();
        }
    }
}