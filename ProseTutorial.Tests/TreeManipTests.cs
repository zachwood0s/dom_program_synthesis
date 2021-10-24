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

namespace TreeManipulation
{
    [TestClass]
    public class TreeManipTest
    {
        private const string GrammarPath = @"../../../../ProseTutorial/tree_synthesis/grammar/treemanim.grammar";

        private static StructNode TreeNode(string label)
        {
            return TreeNode(label, new Attributes());
        }

        private static StructNode TreeNode(string label, params StructNode[] children)
        {
            return TreeNode(label, new Attributes(), children);
        }

        private static StructNode TreeNode(string label, Attributes attributes, params StructNode[] children)
        {
            var n = StructNode.Create(label, attributes, children);
            foreach (var c in children)
                c.Parent = n;
            return n;
        }

        [TestMethod]
        public void TestLearnChildren()
        {
            //parse grammar file 
            Result<Grammar> grammar = CompileGrammar();
            //configure the prose engine 
            SynthesisEngine prose = ConfigureSynthesis(grammar.Value);

            //create the example
            var inTree = TreeNode("parent",
                            TreeNode("child1"),
                            TreeNode("child2"));

            State input = State.CreateForExecution(grammar.Value.InputSymbol, inTree);
            var childTest1 = new[] { TreeNode("child1"), TreeNode("child2") };
            var examples = new Dictionary<State, object> { { input, childTest1 } };
            var spec = new ExampleSpec(examples);

            //learn the set of programs that satisfy the spec 
            ProgramSet learnedSet = prose.LearnGrammar(spec);

            //run the first synthesized program in the same input and check if 
            //the output is correct
            IEnumerable<ProgramNode> programs = learnedSet.RealizedPrograms.ToList();
            var output = programs.First().Invoke(input) as IEnumerable<object>;

            Assert.IsTrue(childTest1.SequenceEqual(output));

            var inTree2 = TreeNode("parent",
                            TreeNode("child1"),
                            TreeNode("child2",
                                TreeNode("child3")));
            State differentInput = State.CreateForExecution(grammar.Value.InputSymbol, inTree2);
            output = programs.First().Invoke(differentInput) as IEnumerable<object>;
            Assert.IsTrue(new[] { TreeNode("child1"), TreeNode("child2", TreeNode("child3")) }.SequenceEqual(output));
        }

        [TestMethod]
        public void TestLearnDescendants()
        {
            //parse grammar file 
            Result<Grammar> grammar = CompileGrammar();
            //configure the prose engine 
            SynthesisEngine prose = ConfigureSynthesis(grammar.Value);

            //create the example
            var inTree1 = TreeNode("parent",
                            TreeNode("child1"),
                            TreeNode("child2"));

            var inTree2 = TreeNode("parent",
                            TreeNode("child1"),
                            TreeNode("child2",
                                TreeNode("child3")));

            State input = State.CreateForExecution(grammar.Value.InputSymbol, inTree1);
            State input2 = State.CreateForExecution(grammar.Value.InputSymbol, inTree2);
            var childTest1 = new[] { TreeNode("child1"), TreeNode("child2") };
            var childTest2 = new[] { TreeNode("child1"), TreeNode("child2", TreeNode("child3")), TreeNode("child3")};
            var examples = new Dictionary<State, object> { { input, childTest1}, { input2, childTest2 } };
            var spec = new ExampleSpec(examples);

            //learn the set of programs that satisfy the spec 
            ProgramSet learnedSet = prose.LearnGrammar(spec);

            //run the first synthesized program in the same input and check if 
            //the output is correct
            IEnumerable<ProgramNode> programs = learnedSet.RealizedPrograms;
            var output = programs.First().Invoke(input) as IEnumerable<object>;
            Assert.IsTrue(childTest1.SequenceEqual(output));

            output = programs.First().Invoke(input2) as IEnumerable<object>;
            Assert.IsTrue(childTest2.SequenceEqual(output));

            var inTree3 = TreeNode("parent",
                            TreeNode("child1", 
                                TreeNode("child3")),
                            TreeNode("child2"));
            State differentInput = State.CreateForExecution(grammar.Value.InputSymbol, inTree3);
            output = programs.First().Invoke(differentInput) as IEnumerable<object>;
            var childTest3 = new[] { TreeNode("child1", TreeNode("child3")), TreeNode("child3"), TreeNode("child2")};
            Assert.IsTrue(childTest3.SequenceEqual(output));
        }

        [TestMethod]
        public void TestLearnKthChild()
        {
            //parse grammar file 
            Result<Grammar> grammar = CompileGrammar();
            //configure the prose engine 
            SynthesisEngine prose = ConfigureSynthesis(grammar.Value);

            //create the example
            var inTree1 = TreeNode("parent",
                            TreeNode("child1"),
                            TreeNode("child2"));

            var inTree2 = TreeNode("parent",
                            TreeNode("child1"),
                            TreeNode("child2",
                                TreeNode("child3")));

            State input = State.CreateForExecution(grammar.Value.InputSymbol, inTree1);
            State input2 = State.CreateForExecution(grammar.Value.InputSymbol, inTree2);
            var childTest1 = new[] { TreeNode("child2") };
            var childTest2 = new[] { TreeNode("child2", TreeNode("child3"))};
            var examples = new Dictionary<State, object> { { input, childTest1}, { input2, childTest2 } };
            var spec = new ExampleSpec(examples);

            var scoreFeature = new RankingScore(grammar.Value);
            ProgramSet topPrograms = prose.LearnGrammarTopK(spec, scoreFeature, 1, null);
            ProgramNode topProgram = topPrograms.RealizedPrograms.First();
            var output = topProgram.Invoke(input) as IEnumerable<object>;
            Assert.IsTrue(childTest1.SequenceEqual(output));

            output = topProgram.Invoke(input2) as IEnumerable<object>;
            Assert.IsTrue(childTest2.SequenceEqual(output));

            var inTree3 = TreeNode("parent",
                            TreeNode("child1", 
                                TreeNode("child3")),
                            TreeNode("child2"));
            State differentInput = State.CreateForExecution(grammar.Value.InputSymbol, inTree3);
            output = topProgram.Invoke(differentInput) as IEnumerable<object>;
            Assert.IsTrue(new[] { TreeNode("child2")}.SequenceEqual(output));
        }

        [TestMethod]
        public void TestLearnMatchTag()
        {
            //parse grammar file 
            Result<Grammar> grammar = CompileGrammar();
            //configure the prose engine 
            SynthesisEngine prose = ConfigureSynthesis(grammar.Value);

            //create the example
            var inTree1 = TreeNode("parent",
                            TreeNode("special"),
                            TreeNode("child2"));

            var inTree2 = TreeNode("parent",
                            TreeNode("child1",
                                TreeNode("special")));
                                

            State input = State.CreateForExecution(grammar.Value.InputSymbol, inTree1);
            State input2 = State.CreateForExecution(grammar.Value.InputSymbol, inTree2);
            var childTest1 = new[] { TreeNode("special") };
            var childTest2 = new[] { TreeNode("special") };
            var examples = new Dictionary<State, object> { { input, childTest1}, { input2, childTest2 } };
            var spec = new ExampleSpec(examples);

            var scoreFeature = new RankingScore(grammar.Value);
            ProgramSet topPrograms = prose.LearnGrammarTopK(spec, scoreFeature, 1, null);

            ProgramNode topProgram = topPrograms.RealizedPrograms.First();
            var output = topProgram.Invoke(input) as IEnumerable<object>;
            Assert.IsTrue(childTest1.SequenceEqual(output));

            output = topProgram.Invoke(input2) as IEnumerable<object>;
            Assert.IsTrue(childTest2.SequenceEqual(output));

            var inTree3 = TreeNode("parent",
                            TreeNode("child1", 
                                TreeNode("child3"),
                                TreeNode("special")),
                            TreeNode("child2"));
            State differentInput = State.CreateForExecution(grammar.Value.InputSymbol, inTree3);
            output = topProgram.Invoke(differentInput) as IEnumerable<object>;
            Assert.IsTrue(childTest1.SequenceEqual(output));
        }

        public static SynthesisEngine ConfigureSynthesis(Grammar grammar)
        {
            var witnessFunctions = new WitnessFunctions(grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] { deductiveSynthesis };
            var synthesisConfig = new SynthesisEngine.Config { Strategies = synthesisExtrategies };
            var prose = new SynthesisEngine(grammar, synthesisConfig);
            return prose;
        }

        private static Result<Grammar> CompileGrammar()
        {
            return DSLCompiler.Compile(new CompilerOptions
            {
                InputGrammarText = File.ReadAllText(GrammarPath),
                References = CompilerReference.FromAssemblyFiles(
                    typeof(Semantics).GetTypeInfo().Assembly,
                    typeof(Microsoft.ProgramSynthesis.Wrangling.Tree.Node).GetTypeInfo().Assembly)
            });
        }
    }
}