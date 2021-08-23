using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.IO;

namespace FreeGames
{
    internal class Program
    {
        private const string fileName = "Latest.txt";

        private static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _driver = new ChromeDriver();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(50));
            string[] last = new string[1];
            if (!File.Exists(fileName))
            {
                File.Create(fileName);
            }
            else
            {
                //last = File.ReadAllLines(fileName);
                StreamReader reader = File.OpenText(fileName);
                string temp = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                last = temp.Split("\r\n");
            }
            List<string> freeGames = GetCurrentAndUpcomming().ToList();
            List<string> links = GetCurrentAndUpcommingLinks().ToList();
            //_driver.Close();

            Console.Clear();
            List<double> timeTaken = new List<double>();
            timeTaken.Add(stopwatch.Elapsed.TotalSeconds);
            Console.WriteLine("Time taken: " + timeTaken[timeTaken.Count - 1]);

            for (int i = 0; i < freeGames.Count; i++)
            {
                freeGames[i] = freeGames[i].Replace("https://www.epicgames.com/store/en-US/p/", "");
                freeGames[i] = freeGames[i].Replace("-", " ");
                freeGames[i] = SetUpperCase(freeGames[i]);
                if (last.Length <= i || freeGames[i] != last[i])
                {
                    Console.WriteLine(freeGames[i]);
                }
            }
            using (WebClient client = new WebClient())
            {
                string temp = client.DownloadString("https://www.gog.com/");
                string start = @"ng-href=""/game/";
                int startIndex = temp.IndexOf(start) + start.Length;
                if (startIndex > start.Length)
                {
                    string game = temp.Substring(startIndex, temp.IndexOf('"', startIndex) - startIndex);
                    links.Add("http://www.gog.com/game/" + game);
                    game = game.Replace('_', ' ');
                    freeGames.Add("Gog: " + SetUpperCase(game));
                    if (last.Length <= freeGames.Count() - 1 || freeGames[freeGames.Count() - 1] != last[freeGames.Count() - 1])
                    {
                        Console.WriteLine(freeGames[freeGames.Count() - 1]);
                    }
                }
                start = "href=\"http://www.gog.com/promo/";
                startIndex = temp.IndexOf(start) + start.Length;
                string free = "                                Free                            ";
                while (startIndex > start.Length)  // >0.00</span></span><span
                {
                    string game = temp.Substring(startIndex, temp.IndexOf('"', startIndex) - startIndex);
                    if (temp.IndexOf(free, startIndex) < startIndex + 6500 && temp.IndexOf(free, startIndex) > 0)
                    {
                        links.Add("http://www.gog.com/promo/" + game);
                        game = game.Replace('_', ' ');
                        freeGames.Add("Gog: " + SetUpperCase(game));
                        if (last.Length <= freeGames.Count() - 1 || freeGames[freeGames.Count() - 1] != last[freeGames.Count() - 1])
                        {
                            Console.WriteLine(freeGames[freeGames.Count() - 1]);
                        }
                    }
                    startIndex = temp.IndexOf(start, startIndex) + start.Length;
                }
                start = ">0.00</span></span><span";
                startIndex = temp.IndexOf(start) + start.Length;
                free = "</div></div><div class=\"big-spot__title\">";
                while (startIndex > start.Length)  // >0.00</span></span><span
                {
                    int nameIndex = temp.IndexOf(free, startIndex - 2000) + free.Length;
                    string game = temp.Substring(nameIndex, temp.IndexOf('<', nameIndex) - nameIndex).Trim();
                    links.Add("http://www.gog.com/game/" + game.ToLower().Replace(' ', '_'));
                    game = game.Replace('_', ' ');
                    freeGames.Add("Gog: " + SetUpperCase(game));
                    if (last.Length <= freeGames.Count() - 1 || freeGames[freeGames.Count() - 1] != last[freeGames.Count() - 1])
                    {
                        Console.WriteLine(freeGames[freeGames.Count() - 1]);
                    }
                    startIndex = temp.IndexOf(start, startIndex) + start.Length;
                }
            }
            File.WriteAllLines(fileName, freeGames);
            //timeTaken.Add(stopwatch.Elapsed.TotalSeconds);
            //Console.WriteLine("Time taken: " + timeTaken[timeTaken.Count - 1]);
            //for (int i = 0; i < 20; i++)
            //{
            //    stopwatch.Restart();
            //    freeGames = GetCurrentAndUpcomming().ToList();
            //    timeTaken.Add(stopwatch.Elapsed.TotalSeconds);
            //    Console.WriteLine("Time taken: " + timeTaken[timeTaken.Count - 1]);
            //}
            //Console.WriteLine("Average: " + Average(timeTaken));
            //timeTaken.RemoveAt(0);
            //Console.WriteLine("Average without first: " + Average(timeTaken));
            Console.ReadKey(true);
            _driver.Quit();
            _driver.Dispose();
            Environment.Exit(0);
        }

        private static double Average(List<double> time)
        {
            double total = 0;
            for (int i = 0; i < time.Count; i++)
            {
                total += time[i];
            }
            return total / (double)time.Count;
        }

        private static string SetUpperCase(string str)
        {
            string[] words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (!char.IsUpper(words[i][0]))
                {
                    char character = char.ToUpper(words[i][0]);
                    words[i] = words[i].Remove(0, 1);
                    words[i] = words[i].Insert(0, character.ToString());
                }
            }
            str = string.Empty;
            for (int i = 0; i < words.Length; i++)
            {
                str += words[i] + " ";
            }
            return str;
        }

        private static IWebDriver _driver;
        private static WebDriverWait _wait;

        private static IEnumerable<string> GetFreeGamesUrls()
        {
            _driver.Navigate().GoToUrl("https://www.epicgames.com/store/en-US/free-games");

            _wait.Until(x => x.FindElement(By.XPath("//div[@data-component=\"CardGridDesktopBase\"]")).Displayed);

            Thread.Sleep(10000);

            var urls = GetElements("//a[descendant::span[text()='Free Now']]")
                .Select(element => element.GetAttribute("href")).ToList();

            return urls;
        }

        private static IEnumerable<string> GetCurrentAndUpcomming()
        {
            _driver.Navigate().GoToUrl("https://www.epicgames.com/store/en-US/free-games");

            _wait.Until(x => x.FindElement(By.XPath("//div[@data-component=\"CardGridDesktopBase\"]")).Displayed);

            Thread.Sleep(100);

            var urls = GetElements("//a[descendant::span[text()='Free Now']]")
                .Select(element => element.GetAttribute("href")).ToList();
            for (int i = 0; i < urls.Count; i++)
            {
                urls[i] = "Current: " + urls[i];
            }
            var comeUrls = GetElements("//a[descendant::span[text()='Coming Soon']]")
                .Select(element => element.GetAttribute("href")).ToList();
            for (int i = 0; i < comeUrls.Count; i++)
            {
                comeUrls[i] = "Upcoming: " + comeUrls[i];
            }
            urls.AddRange(comeUrls);
            return urls;
        }

        private static IEnumerable<string> GetCurrentAndUpcommingLinks()
        {
            if (_driver.Url != "https://www.epicgames.com/store/en-US/free-games")
            {
                _driver.Navigate().GoToUrl("https://www.epicgames.com/store/en-US/free-games");

                _wait.Until(x => x.FindElement(By.XPath("//div[@data-component=\"CardGridDesktopBase\"]")).Displayed);

                Thread.Sleep(100);
            }
            var urls = GetElements("//a[descendant::span[text()='Free Now']]")
                .Select(element => element.GetAttribute("href")).ToList();
            var comeUrls = GetElements("//a[descendant::span[text()='Coming Soon']]")
                .Select(element => element.GetAttribute("href")).ToList();
            urls.AddRange(comeUrls);
            return urls;
        }

        private static List<IWebElement> GetElements(string xPath)
        {
            try
            {
                _wait.Until(x => x.FindElements(By.XPath(xPath)));

                return _driver.FindElements(By.XPath(xPath)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine($"element not found : {xPath}");

                throw;
            }
        }
    }
}