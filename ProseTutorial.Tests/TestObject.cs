using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SubstringSynthesis.Tests
{
    class TestObject
    {
        List<Tuple<string, string>> Examples { get; set; }
        List<Tuple<string, string>> TestCases { get; set; }

        private const string _GrammarPath = @"../../../../ProseTutorial/substring_synthesis/grammar/substring.grammar";

        public TestObject()
        {
            Examples = new List<Tuple<string, string>>();
            TestCases = new List<Tuple<string, string>>();
        }

        public void CreateExample(string input, string output)
        {
            Examples.Add(new Tuple<string, string>(input, output));
        }

        public void CreateTestCase(string input, string output)
        {
            TestCases.Add(new Tuple<string, string>(input, output));
        }

        public void RunTest()
        {
            Result<Grammar> grammar = compileGrammar();
            SynthesisEngine prose = configureSynthesis(grammar.Value);
            var spec = getExampleSpec(grammar);

            ProgramSet learnedSet = prose.LearnGrammar(spec);
            IEnumerable<ProgramNode> programs = learnedSet.RealizedPrograms;

            if (programs.Count() == 0)
                Assert.Fail("No programs were learned.");

            foreach(Tuple<string, string> example in Examples)
            {
                runRealizedProgramWith(programs.First(), grammar, example.Item1, example.Item2);
            }

            foreach (Tuple<string, string> example in TestCases)
            {
                runRealizedProgramWith(programs.First(), grammar, example.Item1, example.Item2);
            }
        }

        private void runRealizedProgramWith(ProgramNode program, Result<Grammar> grammar, string input, string output)
        {
            State state = State.CreateForExecution(grammar.Value.InputSymbol, input);
            var programOutput = program.Invoke(state) as string;
            Assert.AreEqual(output, programOutput);
        }

        private ExampleSpec getExampleSpec(Result<Grammar> grammar)
        {
            var examples = new Dictionary<State, object>();
            foreach (Tuple<string, string> example in Examples)
            {
                State state = State.CreateForExecution(grammar.Value.InputSymbol, example.Item1);
                examples.Add(state, example.Item2);
            }
            return new ExampleSpec(examples);
        }

        private SynthesisEngine configureSynthesis(Grammar grammar)
        {
            var witnessFunctions = new WitnessFunctions(grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] { deductiveSynthesis };
            var synthesisConfig = new SynthesisEngine.Config { Strategies = synthesisExtrategies };
            var prose = new SynthesisEngine(grammar, synthesisConfig);
            return prose;
        }

        private Result<Grammar> compileGrammar()
        {
            return DSLCompiler.Compile(new CompilerOptions
            {
                InputGrammarText = File.ReadAllText(_GrammarPath),
                References = CompilerReference.FromAssemblyFiles(typeof(Semantics).GetTypeInfo().Assembly)
            });
        }
    }
}
