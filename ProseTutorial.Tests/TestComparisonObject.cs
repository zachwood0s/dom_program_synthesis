using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.ProgramSynthesis.Transformation.Text;
using Microsoft.ProgramSynthesis.Wrangling;
using Microsoft.ProgramSynthesis.Wrangling.Constraints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ProgramSynthesis.DslLibrary;
using System.Reflection;
using Microsoft.ProgramSynthesis.Utils;
using HtmlAgilityPack;
using System.Net;
using System.Diagnostics;

namespace Tests.Utils
{
    public class TestComparisonObject
    {
        private List<List<Constraint<IRow, object>>> constraints;
        private List<Tuple<InputRow, string[]>> testCases;
        private JoinedWebscrapeTestObject testObject;

        public int ExampleCount { get { return testObject.Examples.Count; } }

        public TestComparisonObject() 
        {
            testObject = new JoinedWebscrapeTestObject("WebSynthesis.Joined.grammar");
            constraints = new List<List<Constraint<IRow, object>>>();
            testCases = new List<Tuple<InputRow, string[]>>();

            testObject.Init(
                g => new WebSynthesis.Joined.RankingScore(g),
                g => new WebSynthesis.Joined.WitnessFunctions(g),
                typeof(WebSynthesis.Joined.Semantics).GetTypeInfo().Assembly,
                typeof(WebSynthesis.TreeManipulation.Semantics).GetTypeInfo().Assembly,
                typeof(WebSynthesis.Substring.Semantics).GetTypeInfo().Assembly,
                typeof(WebSynthesis.TreeManipulation.Language).GetTypeInfo().Assembly,
                typeof(WebSynthesis.Substring.Language).GetTypeInfo().Assembly,
                typeof(StringRegion).GetTypeInfo().Assembly,
                typeof(Record).GetTypeInfo().Assembly
                );
        }

        public void CreateExample(string url, params string[] output)
        {
            CreateWebExample(url, output);
            CreateTextExample(url, output);
        }

        public void CreateWebExample(string url, params string[] output)
        {
            testObject.CreateExample(url, output);
        }

        public void CreateTextExample(string url, params string[] output)
        {
            for(int i = 0; i < output.Length; i++)
            {
                if (constraints.Count <= i)
                {
                    constraints.Add(new List<Constraint<IRow, object>>());
                }
                constraints[i].Add(new Example(getInputRow(url), output[i]));
            }
        }

        public void CreateTestCase(string url, params string[] output)
        {
            testObject.CreateTestCase(url, output);
            testCases.Add(new Tuple<InputRow, string[]>(getInputRow(url), output));
        }

        public void RunTest()
        {
            RunWebTest();
            RunTextTest();
        }

        public void RunWebTest()
        {
            testObject.RunTest();
        }

        public void RunTextTest()
        {
            Console.WriteLine("\nRunning FlashExtract Tests:");

            var watch = new Stopwatch();
            watch.Start();
            List<Program> programs = new List<Program>();
            foreach (List<Constraint<IRow, object>> c in constraints) programs.Add(Learner.Instance.Learn(c));
            watch.Stop();

            Console.WriteLine($"Created program in {watch.Elapsed.TotalSeconds} secs");

            int failedTests = 0;
            for(int i = 0; i < programs.Count; i++)
            {
                if (programs[i] == null)
                {
                    Assert.Fail("Cound not find program!");
                }

                foreach (Tuple<InputRow, string[]> testCase in testCases)
                {
                    string output = programs[i].Run(testCase.Item1) as string;
                    //Console.WriteLine($"\nExpected: {testCase.Item2[i]}");
                    //Console.WriteLine($"Actual: {output}");
                    if (testCase.Item2[i] != output)
                        failedTests++;
                }
            }

            Console.WriteLine($"Tests passed {testCases.Count - failedTests}/{testCases.Count} | {(testCases.Count - failedTests) / (float)testCases.Count}%");
        }

        public void Clear()
        {
            testObject.Clear();
        }

        private InputRow getInputRow(string url)
        {
            WebClient web = new WebClient();
            string htmlString = web.DownloadString(url);
            return new InputRow(htmlString);
        }
    }
}
