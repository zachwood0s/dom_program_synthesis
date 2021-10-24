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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubstringSynthesis.Tests;

namespace SubstringSynthesis
{
    [TestClass]
    public class SubstringTests
    {
        [TestMethod]
        public void TestNumbers()
        {
            TestObject testObject = new TestObject();

            testObject.CreateExample("After landing at 1270 ", "1270");
            testObject.CreateExample("1221 asdf", "1221");

            testObject.CreateTestCase("----123 asdf", "123");
            testObject.CreateTestCase("1233456asdf", "1233456");
            testObject.CreateTestCase("123", "123");
            testObject.CreateTestCase("in 2011. He stood without success f", "2011");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestDate()
        {
            TestObject testObject = new TestObject();

            testObject.CreateExample("asdf qe 11/01/1111 asdfasdf asde q", "11/01/1111");
            testObject.CreateExample(" 11-01-1111 asasdf asde q", "11-01-1111");

            testObject.CreateTestCase("ase 11-01-1111 asd", "11-01-1111");
            testObject.CreateTestCase(" 1222-01-11 asasdf asde q", "1222-01-11");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestPhoneNumbers()
        {
            TestObject testObject = new TestObject();

            testObject.CreateExample("asdf qe (231) 231-1234 13411faad 123", "(231) 231-1234");
            testObject.CreateExample("as (111) 875-5678 ad 123", "(111) 875-5678");

            testObject.CreateTestCase("asdf dvasae fdasdf 1222-01-11 (221) 231-1234 asde q", "(221) 231-1234");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestConcat()
        {
            TestObject testObject = new TestObject();

            testObject.CreateExample("1-2-3", "123");
            testObject.CreateExample("aa-b-c", "aabc");

            testObject.CreateTestCase("3-2-1", "321");

            testObject.RunTest();
        }

        [TestMethod]
        public void TestPrefix()
        {
            TestObject testObject = new TestObject();

            testObject.CreateExample("asdfa fda Dr. Smith asdfa", "Dr. Smith");
            testObject.CreateExample("1;sdf Dr. jeff", "Dr. jeff");

            testObject.CreateTestCase(" Dr. bob ", "Dr. bob");

            testObject.RunTest();
        }
    }
}