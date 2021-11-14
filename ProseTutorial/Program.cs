using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Features;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using WebSynthesis.RelationalProperties;
using WebSynthesis.TreeManipulation;
using WebSynthesis.TreeManipulation;

namespace WebSynthesis
{
    internal class Program
    {
        private static readonly Grammar Grammar = DSLCompiler.Compile(new CompilerOptions
        {
            InputGrammarText = File.ReadAllText("synthesis/grammar/substring.grammar"),
            References = CompilerReference.FromAssemblyFiles(typeof(Program).GetTypeInfo().Assembly)
        }).Value;

        private static SynthesisEngine _prose;

        private static readonly Dictionary<State, object> Examples = new Dictionary<State, object>();
        private static ProgramNode _topProgram;

        private static void Main(string[] args)
        {

            var testObject = new WebscrapeTestObject(@"tree_synthesis/grammar/treemanim.grammar");
            testObject.Init(
                g => new TreeManipulation.LikelihoodScore(g),
                g => new TreeManipulation.WitnessFunctions(g),
                typeof(TreeManipulation.Semantics).GetTypeInfo().Assembly
                );

            testObject.CreateExample(
                "https://www.cs.purdue.edu/people/faculty/chjung.html",

                // Expected Output
                "<h1>Changhee Jung</h1>",
                "<h3 style=\"color: #000;\">Associate Professor in Computer Science</h3>");

            testObject.CreateExample(
                "https://www.cs.purdue.edu/people/faculty/bgstm.html",

                // Expected Output
                "<h1>Tony Bergstrom</h1>",
                "<h3 style=\"color: #000;\">Assistant Professor of Practice</h3>");

            testObject.CreateTestCase(
                "https://www.cs.purdue.edu/people/faculty/clifton.html",

                // Expected Output
                "<h1>Christopher W. Clifton</h1>",
                "<h3 style=\"color: #000;\">Professor of Computer Science</h3>");

            testObject.RunTest();
            Console.ReadKey();

            return;
            _prose = ConfigureSynthesis();
            var menu = @"Select one of the options: 
1 - provide new example
2 - run top synthesized program on a new input
3 - exit";
            var option = 0;
            while (option != 3)
            {
                Console.Out.WriteLine(menu);
                try
                {
                    option = short.Parse(Console.ReadLine());
                }
                catch (Exception)
                {
                    Console.Out.WriteLine("Invalid option. Try again.");
                    continue;
                }

                try
                {
                    RunOption(option);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Something went wrong...");
                    Console.Error.WriteLine("Exception message: {0}", e.Message);
                }
            }
        }

        private static void RunOption(int option)
        {
            switch (option)
            {
                case 1:
                    LearnFromNewExample();
                    break;
                case 2:
                    RunOnNewInput();
                    break;
                default:
                    Console.Out.WriteLine("Invalid option. Try again.");
                    break;
            }
        }

        private static void LearnFromNewExample()
        {
            Console.Out.Write("Provide a new input-output example (e.g., \"(Sumit Gulwani)\",\"Gulwani\"): ");
            try
            {
                string input = Console.ReadLine();
                if (input != null)
                {
                    int startFirstExample = input.IndexOf("\"", StringComparison.Ordinal) + 1;
                    int endFirstExample = input.IndexOf("\"", startFirstExample + 1, StringComparison.Ordinal) + 1;
                    int startSecondExample = input.IndexOf("\"", endFirstExample + 1, StringComparison.Ordinal) + 1;
                    int endSecondExample = input.IndexOf("\"", startSecondExample + 1, StringComparison.Ordinal) + 1;

                    if (startFirstExample >= endFirstExample || startSecondExample >= endSecondExample)
                        throw new Exception(
                            "Invalid example format. Please try again. input and out should be between quotes");

                    string inputExample = input.Substring(startFirstExample, endFirstExample - startFirstExample - 1);
                    string outputExample =
                        input.Substring(startSecondExample, endSecondExample - startSecondExample - 1);

                    State inputState = State.CreateForExecution(Grammar.InputSymbol, inputExample);
                    Examples.Add(inputState, outputExample);
                }
            }
            catch (Exception)
            {
                throw new Exception("Invalid example format. Please try again. input and out should be between quotes");
            }

            var spec = new ExampleSpec(Examples);
            Console.Out.WriteLine("Learning a program for examples:");
            foreach (KeyValuePair<State, object> example in Examples)
                Console.WriteLine("\"{0}\" -> \"{1}\"", example.Key.Bindings.First().Value, example.Value);

            var scoreFeature = new RankingScore(Grammar);
            ProgramSet topPrograms = _prose.LearnGrammarTopK(spec, scoreFeature, 4, null);
            if (topPrograms.IsEmpty)
                throw new Exception("No program was found for this specification.");

            _topProgram = topPrograms.RealizedPrograms.First();
            Console.Out.WriteLine("Top 4 learned programs:");
            var counter = 1;
            foreach (ProgramNode program in topPrograms.RealizedPrograms)
            {
                if (counter > 4) break;
                Console.Out.WriteLine("==========================");
                Console.Out.WriteLine("Program {0}: ", counter);
                Console.Out.WriteLine(program.PrintAST(ASTSerializationFormat.HumanReadable));
                counter++;
            }
        }

        private static void RunOnNewInput()
        {
            if (_topProgram == null)
                throw new Exception("No program was synthesized. Try to provide new examples first.");
            Console.Out.WriteLine("Top program: {0}", _topProgram);

            try
            {
                Console.Out.Write("Insert a new input: ");
                string newInput = Console.ReadLine();
                if (newInput != null)
                {
                    int startFirstExample = newInput.IndexOf("\"", StringComparison.Ordinal) + 1;
                    int endFirstExample = newInput.IndexOf("\"", startFirstExample + 1, StringComparison.Ordinal) + 1;
                    newInput = newInput.Substring(startFirstExample, endFirstExample - startFirstExample - 1);
                    State newInputState = State.CreateForExecution(Grammar.InputSymbol, newInput);
                    Console.Out.WriteLine("RESULT: \"{0}\" -> \"{1}\"", newInput, _topProgram.Invoke(newInputState));
                }
            }
            catch (Exception)
            {
                throw new Exception("The execution of the program on this input thrown an exception");
            }
        }

        public static SynthesisEngine ConfigureSynthesis()
        {
            var witnessFunctions = new WitnessFunctions(Grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] {deductiveSynthesis};
            var synthesisConfig = new SynthesisEngine.Config {Strategies = synthesisExtrategies};
            var prose = new SynthesisEngine(Grammar, synthesisConfig);
            return prose;
        }
    }
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

        private ApplicationStrategy _strategy;

        public TestObject(string grammar)
        {
            Examples = new List<Tuple<TIn, TOut>>();
            TestCases = new List<Tuple<TIn, TOut>>();
            _GrammarPath = grammar;
            _strategy = new ApplicationStrategy(grammar);
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
            var casted = Examples.Select(x => new Tuple<object, object>(x.Item1, x.Item2));
            var learnedSet = _strategy.GetProgramSet(casted);
            /*
            var spec = getExampleSpec(_grammar);

            ProgramSet learnedSet = _prose.LearnGrammarTopK(spec, _score, k: 1);
            */
            IEnumerable<ProgramNode> programs = learnedSet.RealizedPrograms;

            Console.WriteLine($"Picking best program: {programs.First()}");

            foreach (var example in Examples)
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
            Console.WriteLine(program.Invoke(state));
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

    }

    class SequenceTestObject<TIn, TOut> : TestObject<TIn, IEnumerable<TOut>>
        where TIn : class
        where TOut : class
    {

        public SequenceTestObject(string grammar) 
            : base(grammar) {}

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
}