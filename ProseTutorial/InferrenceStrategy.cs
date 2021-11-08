using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Features;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace RelationalProperties
{
    /// <summary>
    /// This will handle the actual compilation 
    /// </summary>
    public class ApplicationStrategy
    {
        protected string _GrammarPath;
        protected Grammar _grammar;
        protected SynthesisEngine _prose;
        protected IFeature _score;

        public void Init(Func<Grammar, IFeature> scoreGen, Func<Grammar, DomainLearningLogic> witnessCreator, params Assembly[] assemblies)
        {
            Result<Grammar> grammar = CompileGrammar(assemblies);

            if (grammar.HasErrors)
                throw new Exception("Grammar failed to compile");


            _grammar = grammar.Value;
            _score = scoreGen(_grammar);

            SynthesisEngine prose = ConfigureSynthesis(_grammar, witnessCreator);
            _prose = prose;
        }
        private Result<Grammar> CompileGrammar(params Assembly[] assemblies)
        {
            return DSLCompiler.Compile(new CompilerOptions
            {
                InputGrammarText = File.ReadAllText(_GrammarPath),
                References = CompilerReference.FromAssemblyFiles(assemblies)
            });
        }
        private SynthesisEngine ConfigureSynthesis(Grammar grammar, Func<Grammar, DomainLearningLogic> creator)
        {
            var witnessFunctions = creator(grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] { deductiveSynthesis };
            var synthesisConfig = new SynthesisEngine.Config { Strategies = synthesisExtrategies };
            var prose = new SynthesisEngine(grammar, synthesisConfig);
            return prose;
        }

        public virtual ProgramSet GetProgramSet(IEnumerable<Tuple<object, object>> examples, HashSet<IRelationalProperty> properties, CancellationToken ct = default)
        {
            var pExamples = PerturbExamples(examples, properties);
            var spec = GetExampleSpec(pExamples);
            return _prose.LearnGrammarTopK(spec, _score, k: 1, cancel: ct);
        }

        public virtual ProgramSet GetProgramSetTimed(IEnumerable<Tuple<object, object>> examples, HashSet<IRelationalProperty> properties, int millisecondTimeBound)
        {
            var ts = new CancellationTokenSource();
            CancellationToken ct = ts.Token;

            ProgramSet output = null; 
            var worker = new Thread(new ThreadStart(() => output = GetProgramSet(examples, properties, ct)));

            worker.Start();

            if(!worker.Join(millisecondTimeBound))
            {
                // Kill the worker if the timebound has been exceeded
                ts.Cancel();
                Console.WriteLine("Canceled");
            }
            return output;
        }

        private ExampleSpec GetExampleSpec(IEnumerable<Tuple<object, object>> examples)
        {
            var mapping = new Dictionary<State, object>();
            foreach (var example in examples)
            {
                State state = State.CreateForExecution(_grammar.InputSymbol, example.Item1);
                mapping.Add(state, example.Item2);
            }
            return new ExampleSpec(mapping);
        }

        private HashSet<Tuple<object, object>> PerturbExamples(IEnumerable<Tuple<object, object>> examples, HashSet<IRelationalProperty> properties)
        {
            var outSet = new HashSet<Tuple<object, object>>();
            foreach (var (input, output) in examples)
            {
                foreach(var prop in properties)
                {
                    outSet.UnionWith(prop.ApplyProperty(input, output).ToHashSet());
                }
            }
            return outSet;
        }


    }
}
