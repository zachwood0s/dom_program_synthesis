using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.DslLibrary;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using Microsoft.ProgramSynthesis.VersionSpace;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Utils;

namespace WebSynthesis.Substring
{
    [TestClass]
    public class SubstringTests
    {
        private const string _GrammarPath = @"WebSynthesis.Substring.grammar";
        private static TestObject<string, string> testObject;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            testObject = new TestObject<string, string>(_GrammarPath);

            testObject.Init(
                g => new RankingScore(g),
                g => new WitnessFunctions(g),
                typeof(Semantics).GetTypeInfo().Assembly,
                typeof(StringRegion).GetTypeInfo().Assembly,
                typeof(Record).GetTypeInfo().Assembly
                );
        }

        [TestCleanup]
        public void Cleanup()
        {
            testObject.Clear();
        }


        [TestMethod]
        public void TestRegex()
        {
            testObject.CreateExample("1270 adffss", "1270");
            testObject.CreateExample("asdf 1271 adffss", "1271");

            testObject.CreateTestCase("as sdq 123", "123");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestSelectK()
        {
            testObject.CreateExample("1270 adffss", "1270");
            testObject.CreateExample("asdf adffss", "asdf");

            testObject.CreateTestCase("asdf 123", "asdf");
            testObject.CreateTestCase("as 123", "as");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestSelectKAndConcat()
        {
            testObject.CreateExample("1 1270 a", "1 a");
            testObject.CreateExample("2 dsfa b", "2 b");

            testObject.CreateTestCase("a 2d c", "a c");
            testObject.CreateTestCase("b dfa d", "b d");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestSelectKAndRegex()
        {
            testObject.CreateExample("1 aa advs", "1,aa");
            testObject.CreateExample("adfa bb 2", "2,bb");

            testObject.CreateTestCase("dfa cc advs f 3 a asd s", "3,cc");
            testObject.CreateTestCase("a dd adf 4", "4,dd");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestConcatRecursion()
        {
            testObject.CreateExample("1 2 3 4", "2-3-1");
            testObject.CreateExample("a b c d", "b-c-a");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestDate()
        {
            testObject.CreateExample("asdf qe 11-01-1111 asdfasdf asde q", "11-01-1111");
            testObject.CreateExample(" 11-01-1111 asasdf asde q", "11-01-1111");

            testObject.CreateTestCase("ase 11-01-1111 asd", "11-01-1111");
            testObject.CreateTestCase(" 1222-01-11 asasdf asde q", "1222-01-11");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestPhoneNumbers()
        {
            testObject.CreateExample("asdf qe (231) 231-1234 adf asdf", "(231) 231-1234");
            testObject.CreateExample("as (111) 875-5678 ad fdasd", "(111) 875-5678");

            testObject.CreateTestCase("asdf dvasae fdasdf (221) 231-1234 asde q", "(221) 231-1234");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestLastNameInEmail()
        {
            testObject.CreateExample("Nancy.FreeHafer@fourthcoffee.com", "FreeHafer");
            //testObject.CreateExample("Nancy.FreeHafer@fourthcoffee.com", "FreeHafer");

            testObject.RunTest();
        }
    }
}