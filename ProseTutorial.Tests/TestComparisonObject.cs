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
        private List<Constraint<IRow, object>> constraints;
        private List<Tuple<InputRow, string>> testCases;
        private JoinedWebscrapeTestObject testObject;

        public TestComparisonObject() 
        {
            testObject = new JoinedWebscrapeTestObject("WebSynthesis.Joined.grammar");
            constraints = new List<Constraint<IRow, object>>();
            testCases = new List<Tuple<InputRow, string>>();

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
            constraints.Add(new Example(getInputRow(url), converOutputStr(output)));
        }

        public void CreateTestCase(string url, params string[] output)
        {
            testObject.CreateTestCase(url, output);
            testCases.Add(new Tuple<InputRow, string>(getInputRow(url), converOutputStr(output)));
        }

        public void RunTest()
        {
            RunWebTest();
            RunTextTest();
        }

        public void RunWebTest()
        {
            Console.WriteLine("\nRunning Webscrape Tests:");
            testObject.RunTest();
        }

        public void RunTextTest()
        {
            Console.WriteLine("\nRunning FlashExtract Tests:");
            var watch = new Stopwatch();
            watch.Start();
            Program program = Learner.Instance.Learn(constraints);
            watch.Stop();
            Console.WriteLine($"Created program in {watch.Elapsed.TotalSeconds} secs");

            if (program == null)
            {
                Console.WriteLine("Cound not find program!");
                return;
            }

            foreach(Tuple<InputRow, string> testCase in testCases)
            {
                string output = program.Run(testCase.Item1) as string;
                Console.WriteLine($"\nExpected: {testCase.Item2}");
                Console.WriteLine($"Actual: {output}");
                Assert.AreEqual(testCase.Item2, output);
            }
        }

        public void Clear()
        {

        }

        private InputRow getInputRow(string url)
        {
            WebClient web = new WebClient();
            string htmlString = web.DownloadString(url);
            return new InputRow(htmlString);
        }

        private string converOutputStr(params string[] output)
        {
            return string.Join(",", output);
        }
    }
}
