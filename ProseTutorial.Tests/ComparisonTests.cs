using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Utils;

namespace WebSynthesis.Comparison
{
    [TestClass]
    public class ComparisonTests
    {
        private static TestComparisonObject testObject;
        private static string[] states = {"Alabama", "Alaska", "Arizona", "Arkansas",
                                       "California", "Colorado", "Connecticut", "Delaware",
                                       "Florida", "Georgia", "Hawaii", "Idaho", "Illinois",
                                       "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana",
                                       "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota",
                                       "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
                                       "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio",
                                       "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina",
                                       "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington",
                                       "West Virginia", "Wisconsin", "Wyoming"};
        private static string[] capitals = {"Montgomery", "Juneau", "Phoenix", "Little Rock", "Sacramento", "Denver",
                                         "Hartford", "Dover", "Tallahassee", "Atlanta", "Honolulu", "Boise", "Springfield",
                                         "Indianapolis", "Des Moines", "Topeka", "Frankfort", "Baton Rouge", "Augusta",
                                         "Annapolis", "Boston", "Lansing", "St. Paul", "Jackson", "Jefferson City", "Helena",
                                         "Lincoln", "Carson City", "Concord", "Trenton", "Santa Fe", "Albany", "Raleigh", "Bismarck",
                                         "Columbus", "Oklahoma City", "Salem", "Harrisburg", "Providence", "Columbia", "Pierre", "Nashville",
                                         "Austin", "Salt Lake City", "Montpelier", "Richmond", "Olympia", "Charleston", "Madison", "Cheyenne"};
        private static List<int> statesI;
        private static Random random;

        private static List<Tuple<string, string>> imbdDirectors;
        private static List<Tuple<string, string>> imbdRelease;
        private static List<Tuple<string, string>> restrauntAddresses;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            testObject = new TestComparisonObject();
            random = new Random();
            statesI = Enumerable.Range(0, 50).ToList();

            imbdDirectors = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("https://www.imdb.com/title/tt6264654/?ref_=hm_tpks_tt_i_1_pd_tp1_cp", "2021"),
                new Tuple<string, string>("https://www.imdb.com/title/tt2948372/?ref_=ttls_li_tt", "2020"),
                new Tuple<string, string>("https://www.imdb.com/title/tt12801262/?ref_=tt_sims_tt_t_1", "2021"),
                new Tuple<string, string>("https://www.imdb.com/title/tt5109280/?ref_=tt_sims_tt_t_1", "2021"),
                new Tuple<string, string>("https://www.imdb.com/title/tt7146812/?ref_=tt_sims_tt_t_2", "2020"),
                new Tuple<string, string>("https://www.imdb.com/title/tt5848272/?ref_=tt_sims_tt_t_7", "2018"),
                new Tuple<string, string>("https://www.imdb.com/title/tt3606756/?ref_=tt_sims_tt_t_2", "2018"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0317705/?ref_=tt_sims_tt_t_1", "2004"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0120363/?ref_=tt_sims_tt_t_3", "1999"),
                new Tuple<string, string>("https://www.imdb.com/title/tt1979376/?ref_=tt_sims_tt_t_3", "2019"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0435761/?ref_=tt_sims_tt_t_2", "2010"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0266543/?ref_=tt_sims_tt_t_4", "2003"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0198781/?ref_=tt_sims_tt_t_1", "2001"),
                new Tuple<string, string>("https://www.imdb.com/title/tt1049413/?ref_=tt_sims_tt_t_2", "2009"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0910970/?ref_=tt_sims_tt_t_1", "2008")
            };

            imbdRelease = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("https://www.imdb.com/title/tt6264654/?ref_=hm_tpks_tt_i_1_pd_tp1_cp", "Free Guy"),
                new Tuple<string, string>("https://www.imdb.com/title/tt2948372/?ref_=ttls_li_tt", "Soul"),
                new Tuple<string, string>("https://www.imdb.com/title/tt12801262/?ref_=tt_sims_tt_t_1", "Luca"),
                new Tuple<string, string>("https://www.imdb.com/title/tt5109280/?ref_=tt_sims_tt_t_1", "Raya and the Last Dragon"),
                new Tuple<string, string>("https://www.imdb.com/title/tt7146812/?ref_=tt_sims_tt_t_2", "Onward"),
                new Tuple<string, string>("https://www.imdb.com/title/tt5848272/?ref_=tt_sims_tt_t_7", "Ralph Breaks the Internet"),
                new Tuple<string, string>("https://www.imdb.com/title/tt3606756/?ref_=tt_sims_tt_t_2", "Incredibles 2"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0317705/?ref_=tt_sims_tt_t_1", "The Incredibles"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0120363/?ref_=tt_sims_tt_t_3", "Toy Story 2"),
                new Tuple<string, string>("https://www.imdb.com/title/tt1979376/?ref_=tt_sims_tt_t_3", "Toy Story 4"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0435761/?ref_=tt_sims_tt_t_2", "Toy Story 3"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0266543/?ref_=tt_sims_tt_t_4", "Finding Nemo"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0198781/?ref_=tt_sims_tt_t_1", "Monsters, Inc."),
                new Tuple<string, string>("https://www.imdb.com/title/tt1049413/?ref_=tt_sims_tt_t_2", "Up"),
                new Tuple<string, string>("https://www.imdb.com/title/tt0910970/?ref_=tt_sims_tt_t_1", "15WALL·E")
            };

            restrauntAddresses = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("https://restaurants.fiveguys.com/135-s-chauncey-avenue", "(765) 743-3100"),
                //new Tuple<string, string>("https://www.triplexxxfamilyrestaurant.com", "765-743-5373"),
                new Tuple<string, string>("https://locations.chipotle.com/in/west-lafayette/200-w-state-st?utm_source=google&utm_medium=yext&utm_campaign=yext_listings", "(765) 743-4804"),
                new Tuple<string, string>("http://townandgownbistro.com", "(765)-250-3425"),
                new Tuple<string, string>("https://greyhousecoffee.com", "(765) 743-5316"),
                new Tuple<string, string>("https://www.noodlesandi.com/", "765-743-1190"),
                new Tuple<string, string>("https://www.picklelafayette.com/", "(765) 423-9999"),
                new Tuple<string, string>("https://www.eastendmain.com/", "(765) 607-4600"),
                new Tuple<string, string>("https://www.revolution-bbq.com/", "765-767-4227"),
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            testObject.Clear();
            statesI = Enumerable.Range(0, 50).ToList();
        }

        [TestMethod]
        public void TestWikipediaTitles()
        {
            runExampleSet(100, new int[] { 1, 4 }, getRandomWikipediaPage, getWikipediaTitle);
        }

        [TestMethod]
        public void TestWikipediaHeaders()
        {
            runExampleSet(100, new int[] { 1, 4 }, getRandomWikipediaPage, getWikiHeaders);
        }

        [TestMethod]
        public void TestWikipediaCapitals()
        {
            runExampleSet(45, new int[] { 1, 4 }, getRandomStatePage, getRandomStateCapital);
        }

        [TestMethod]
        public void TestWikipediaFirstText()
        {
            runExampleSet(100, new int[] { 1, 4 }, getRandomWikipediaPage, getWikiFirstText);
        }

        //[TestMethod]
        public void TestRestrauntAddresses()
        {
            runExampleSet(5, new int[] { 1, 2 }, getRestrauntURL, getRestraunt);
        }

        [TestMethod]
        public void TestIMBDYear()
        {
            runExampleSet(10, new int[] { 1, 4 }, getIMBDDirectorURL, getIMBDDirector);
        }

        [TestMethod]
        public void TestIMBDTitle()
        {
            runExampleSet(10, new int[] { 1, 4 }, getIMBDReleaseURL, getIMBDRelease);
        }

        private static void runExampleSet(int testCount, int[] exampleRange, Func<Tuple<string, string>> urlGen, Func<string, string, string[]> outputGen)
        {
            for (int i = 0; i < exampleRange[0]; i++)
            {
                Tuple<string, string> page = urlGen();
                string[] output = outputGen(page.Item1, page.Item2);
                testObject.CreateExample(page.Item1, output);
            }

            for (int i = 0; i < testCount; i++)
            {
                Tuple<string, string> page = urlGen();
                string[] output = outputGen(page.Item1, page.Item2);
                testObject.CreateTestCase(page.Item1, output);
            }

            for (int i = exampleRange[0]; i <= exampleRange[1]; i++)
            {
                if (i != exampleRange[0])
                {
                    Tuple<string, string> page = urlGen();
                    string[] output = outputGen(page.Item1, page.Item2);
                    testObject.CreateExample(page.Item1, output);
                }
          
                Console.WriteLine($"\nRunning tests with {testObject.ExampleCount} examples.");
                testObject.RunWebTest();
                //testObject.RunTextTest();
            }
        }

        #region Wiki Title Generators

        private static Tuple<string, string> getRandomWikipediaPage()
        {
            WebClient web = new WebClient();
            string html = web.DownloadString("http://en.wikipedia.org/wiki/Special:Random");
            string url = Regex.Match(html, @"<link rel=""canonical"" href=""(?<URL>[\s\S]*?)"">", RegexOptions.IgnoreCase).Groups["URL"].Value;
            return new Tuple<string, string> (url.Split('"')[0], html);
        }

        private static string[] getWikipediaTitle(string url, string html)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//h1[@class='firstHeading']");
            string text = nodes.First().InnerHtml;
            return new string[] { text };
        }

        #endregion

        #region Wiki State Capital Generators

        private static Tuple<string, string> getRandomStatePage()
        {
            int index = random.Next(0, statesI.Count);
            string url = "https://en.wikipedia.org/wiki/" + states[statesI[index]];
            return new Tuple<string, string>(url, index.ToString());
        }

        private static string[] getRandomStateCapital(string url, string html)
        {
            int index = int.Parse(html);
            string[] cap = new string[] { capitals[statesI[index]] };
            statesI.RemoveAt(index);
            return cap;
        }

        #endregion

        #region Wiki Header Generators

        private static string[] getWikiHeaders(string url, string html)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//span[@class='mw-headline']");
            if (nodes == null) return new string[] { };
            List<string> headers = new List<string>();
            nodes.ToList().ForEach(n => headers.Add(n.InnerHtml));
            return headers.ToArray();
        }

        #endregion

        #region Wiki First Text

        private static string[] getWikiFirstText(string url, string html)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//li[@id='footer-info-lastmod']");
            string text = nodes.First().InnerText;
            return new string[] { text.Split(",")[0] };
        }

        #endregion

        #region Manual Test Gens

        private static Tuple<string, string> getIMBDDirectorURL()
        {
            return new Tuple<string, string>(imbdDirectors.First().Item1, "");
        }

        private static string[] getIMBDDirector(string url, string html)
        {
            string[] ret = new string[] { imbdDirectors.First().Item2 };
            imbdDirectors.Remove(imbdDirectors.First());
            return ret;
        }

        private static Tuple<string, string> getIMBDReleaseURL()
        {
            return new Tuple<string, string>(imbdRelease.First().Item1, "");
        }

        private static string[] getIMBDRelease(string url, string html)
        {
            string[] ret = new string[] { imbdRelease.First().Item2 };
            imbdRelease.Remove(imbdRelease.First());
            return ret;
        }

        private static Tuple<string, string> getRestrauntURL()
        {
            return new Tuple<string, string>(restrauntAddresses.First().Item1, "");
        }

        private static string[] getRestraunt(string url, string html)
        {
            string[] ret = new string[] { restrauntAddresses.First().Item2 };
            restrauntAddresses.Remove(restrauntAddresses.First());
            return ret;
        }

        #endregion
    }
}