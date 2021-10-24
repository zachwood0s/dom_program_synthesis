﻿using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using Microsoft.ProgramSynthesis.Features;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tests.Utils
{
    class TestObject<TIn, TOut>
        where TOut : class
        where TIn : class
    {
        public List<Tuple<TIn, TOut>> Examples { get; private set; }
        public List<Tuple<TIn, TOut>> TestCases { get; private set; }

        private string _GrammarPath;
        private Grammar _grammar;
        private SynthesisEngine _prose;
        private IFeature _score;

        public TestObject(string grammar)
        {
            Examples = new List<Tuple<TIn, TOut>>();
            TestCases = new List<Tuple<TIn, TOut>>();
            _GrammarPath = grammar;
        }


        public void CreateExample(TIn input, TOut output)
        {
            Examples.Add(new Tuple<TIn, TOut>(input, output));
        }

        public void CreateTestCase(TIn input, TOut output)
        {
            TestCases.Add(new Tuple<TIn, TOut>(input, output));
        }

        public void Clear()
        {
            Examples.Clear();
            TestCases.Clear();
            _prose.ClearLearningCache();
        }

        public void Init(Func<Grammar, IFeature> scoreGen, Func<Grammar, DomainLearningLogic> witnessCreator, params Assembly[] assemblies)
        {
            Result<Grammar> grammar = compileGrammar(assemblies);

            if (grammar.HasErrors)
                throw grammar.Exception;


            _grammar = grammar.Value;
            _score = scoreGen(_grammar);

            SynthesisEngine prose = configureSynthesis(_grammar, witnessCreator);
            _prose = prose;
        }

        public void RunTest()
        {
            var spec = getExampleSpec(_grammar);

            ProgramSet learnedSet = _prose.LearnGrammarTopK(spec, _score, k: 1);
            IEnumerable<ProgramNode> programs = learnedSet.RealizedPrograms;

            if (programs.Count() == 0)
                Assert.Fail("No programs were learned.");

            foreach(var example in Examples)
            {
                runRealizedProgramWith(programs.First(), _grammar, example.Item1, example.Item2);
            }

            foreach (var example in TestCases)
            {
                runRealizedProgramWith(programs.First(), _grammar, example.Item1, example.Item2);
            }
        }

        private void runRealizedProgramWith(ProgramNode program, Grammar grammar, TIn input, TOut output)
        {
            State state = State.CreateForExecution(grammar.InputSymbol, input);
            var programOutput = program.Invoke(state) as TOut;
            AssertTruth(output, programOutput);
        }

        private ExampleSpec getExampleSpec(Grammar grammar)
        {
            var examples = new Dictionary<State, object>();
            foreach (var example in Examples)
            {
                State state = State.CreateForExecution(grammar.InputSymbol, example.Item1);
                examples.Add(state, example.Item2);
            }
            return new ExampleSpec(examples);
        }

        private SynthesisEngine configureSynthesis(Grammar grammar, Func<Grammar, DomainLearningLogic> creator)
        {
            var witnessFunctions = creator(grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] { deductiveSynthesis };
            var synthesisConfig = new SynthesisEngine.Config { Strategies = synthesisExtrategies };
            var prose = new SynthesisEngine(grammar, synthesisConfig);
            return prose;
        }

        private Result<Grammar> compileGrammar(params Assembly[] assemblies)
        {
            return DSLCompiler.Compile(new CompilerOptions
            {
                InputGrammarText = File.ReadAllText(_GrammarPath),
                References = CompilerReference.FromAssemblyFiles(assemblies)
            });
        }

        protected virtual void AssertTruth(TOut expected, TOut actual)
        {
            Assert.AreEqual(expected, actual);
        }
    }

    class SequenceTestObject<TIn, TOut> : TestObject<TIn, IEnumerable<TOut>>
        where TIn : class
        where TOut : class
    {

        public SequenceTestObject(string grammar) 
            : base(grammar) {}

        protected override void AssertTruth(IEnumerable<TOut> expected, IEnumerable<TOut> actual)
        {
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        public void CreateExample(TIn input, params TOut[] outs)
        {
            base.CreateExample(input, outs);
        }
        public void CreateTestCase(TIn input, params TOut[] outs)
        {
            base.CreateExample(input, outs);
        }
    }
}
