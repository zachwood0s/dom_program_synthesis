using Microsoft.ProgramSynthesis;
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
using Microsoft.ProgramSynthesis.Wrangling.Tree;
using HtmlAgilityPack;
using System.Xml.Linq;
using WebSynthesis.TreeManipulation;
using WebSynthesis.RelationalProperties;
using System.Diagnostics;

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

        private RelationalApplicationStrategy _strategy;

        public TestObject(string grammar)
        {
            Examples = new List<Tuple<TIn, TOut>>();
            TestCases = new List<Tuple<TIn, TOut>>();
            _GrammarPath = grammar;
            _strategy = new RelationalApplicationStrategy(grammar);
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
            _strategy.Clear();
            //_prose.ClearLearningCache();
        }

        public void Init(Func<Grammar, IFeature> scoreGen, Func<Grammar, DomainLearningLogic> witnessCreator, params Assembly[] assemblies)
        {
            /*
            Result<Grammar> grammar = compileGrammar(assemblies);

            if (grammar.HasErrors)
                throw new Exception("Grammar failed to compile");


            _grammar = grammar.Value;
            _score = scoreGen(_grammar);

            SynthesisEngine prose = configureSynthesis(_grammar, witnessCreator);
            _prose = prose;
            */
            _strategy.Init(scoreGen, witnessCreator, assemblies);
        }

        public void RunTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            var casted = Examples.Select(x => new Tuple<object, object>(x.Item1, x.Item2));
            var learnedSet = _strategy.GetProgramSet(casted);
            watch.Stop();
            Console.WriteLine($"Created program in {watch.Elapsed.TotalSeconds} secs");
            /*
            var spec = getExampleSpec(_grammar);

            ProgramSet learnedSet = _prose.LearnGrammarTopK(spec, _score, k: 1);
            */
            IEnumerable<ProgramNode> programs = learnedSet.RealizedPrograms;

            if (programs.Count() == 0)
                Assert.Fail("No programs were learned.");

            Console.WriteLine($"Picking best program: {programs.First()}");

            foreach(var example in Examples)
            {
                runRealizedProgramWith(programs.First(), _strategy.Grammar, example.Item1, example.Item2);
            }

            foreach (var example in TestCases)
            {
                runRealizedProgramWith(programs.First(), _strategy.Grammar, example.Item1, example.Item2);
            }
        }

        private void runRealizedProgramWith(ProgramNode program, Grammar grammar, TIn input, TOut output)
        {
            State state = State.CreateForExecution(grammar.InputSymbol, input);
            AssertTruth(output, program.Invoke(state));
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

        protected virtual void AssertTruth(TOut expected, object actual)
        {

            Assert.AreEqual(expected, actual as TOut);
        }
    }

    class SequenceTestObject<TIn, TOut> : TestObject<TIn, IEnumerable<TOut>>
        where TIn : class
        where TOut : class
    {

        public SequenceTestObject(string grammar) 
            : base(grammar) {}

        protected override void AssertTruth(IEnumerable<TOut> expected, object actual)
        {
            Assert.IsTrue(expected.SequenceEqual(actual as IEnumerable<object>));
        }

        public void CreateExample(TIn input, params TOut[] outs)
        {
            base.CreateExample(input, outs.ToList());
        }
        public void CreateTestCase(TIn input, params TOut[] outs)
        {
            base.CreateTestCase(input, outs.ToList());
        }
    }

    class HtmlSequenceTestObject: SequenceTestObject<ProseHtmlNode, ProseHtmlNode>
    {
        public HtmlSequenceTestObject(string grammar) 
            : base(grammar) {}
    }

    class WebscrapeTestObject : SequenceTestObject<ProseHtmlNode, ProseHtmlNode>
    {

        public WebscrapeTestObject(string grammar)
            : base(grammar) { }

        public void CreateExample(string url, params string[] outs)
        {
            var node = ParseFromURL(url);
            var outNodes = outs.Select(ParseFromString).ToList();
            CreateExample(node, outNodes);
        }

        public void CreateTestCase(string url, params string[] outs)
        {
            var node = ParseFromURL(url);
            var outNodes = outs.Select(ParseFromString).ToList();
            CreateTestCase(node, outNodes);
        }

        protected override void AssertTruth(IEnumerable<ProseHtmlNode> expected, object actual)
        {
            Assert.IsTrue(expected.SequenceEqual(actual as IEnumerable<object>));
        }

        private ProseHtmlNode ParseFromString(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return ProseHtmlNode.DeserializeFromHtmlNode(doc.DocumentNode.FirstChild);
        }

        private ProseHtmlNode ParseFromURL(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            doc.OptionOutputAsXml = true;
            var root = doc.DocumentNode;

            return ProseHtmlNode.DeserializeFromHtmlNode(root.SelectSingleNode("html"));
        }
    }
    class JoinedWebscrapeTestObject : SequenceTestObject<ProseHtmlNode, string>
    {

        public JoinedWebscrapeTestObject(string grammar)
            : base(grammar) { }

        public void CreateExample(string url, params string[] outs)
        {
            var node = ParseFromURL(url);
            CreateExample(node, outs);
        }

        public void CreateTestCase(string url, params string[] outs)
        {
            var node = ParseFromURL(url);
            CreateTestCase(node, outs);
        }

        protected override void AssertTruth(IEnumerable<string> expected, object actual)
        {
            Console.WriteLine($"\nExpected: {string.Join(',', expected.ToArray())}");
            Console.WriteLine($"Actual: {string.Join(',', (actual as IEnumerable<object>).ToArray())}");
            Assert.IsTrue(expected.SequenceEqual(actual as IEnumerable<object>));
        }

        private ProseHtmlNode ParseFromURL(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            doc.OptionOutputAsXml = true;
            var root = doc.DocumentNode;

            return ProseHtmlNode.DeserializeFromHtmlNode(root.SelectSingleNode("html"));
        }
    }
}
