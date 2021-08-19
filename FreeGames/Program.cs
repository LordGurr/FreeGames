using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;

using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

using System.Threading;

using System.Threading.Tasks;
using Newtonsoft.Json;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

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
                while (startIndex > start.Length)
                {
                    string game = temp.Substring(startIndex, temp.IndexOf('"', startIndex) - startIndex);
                    int x = temp.IndexOf(free, startIndex);
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
            }
            //for (int i = 0; i < freeGames.Count; i++)
            //{
            //    Console.WriteLine(freeGames[i]);
            //}
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

        private static List<string> TryGetLinks(string url)
        {
            var results = new List<string>();

            //#region getting all possible links
            //_logger.LogDebug("Getting article page source");
            var webGet = new HtmlWeb();
            var htmlDoc = webGet.Load(url);
            //_logger.LogDebug("Debug");

            //var links = htmlDoc.DocumentNode.CssSelect("div.entry-content a");
            ////#endregion

            ////#region add links to list
            //foreach (var each in links)
            //{
            //    //if (!wordList.Exists(x => x == each.InnerText.ToString().ToLower()))
            //    {
            //        string link = each.Attributes["href"].Value.ToString().Split('?')[0];

            //        if (link.Contains("#disqus_thread"))
            //            continue;

            //        //_logger.LogInformation("Get possible link: {0}", link);
            //        results.Add(link);
            //    }
            //}
            //#endregion

            return results;
        }
    }

    /*internal class Program
    {
        // webdriver stuff
        private static IWebDriver _driver;

        private static WebDriverWait _wait;

        // api url for sending telegram messages.
        private const string Url = "https://epic-games-yoinker-api.azurewebsites.net/message/send";

        // user variables
        private static string _username;

        private static string _password;
        private static string _captcha;
        private static string _telegram;

        private static void Main(string[] args)
        {
            // Retrieve the user variables, these should be set through github secrets
            _username = "gronsonorvar@gmail.com"; //Environment.GetEnvironmentVariable("epicname");
            _password = "bjSq5u4UeQcPzDsA"; //Environment.GetEnvironmentVariable("epicpass");
            _captcha = "https://accounts.hcaptcha.com/verify_email/16d41cc9-b789-401d-a51f-0ef1dcea87e2"; //Environment.GetEnvironmentVariable("captcha");

            // optional: search in telegram for the bot "epic games yoinker, send him a message after a while he send you the id"
            _telegram = "Hello"; //Environment.GetEnvironmentVariable("telegram");

            // Check if the arguments are valid.
            if (ValidateArguments() == false)
            {
                return;
            }
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--lang=en");
            // create an instance of the webdriver
            _driver = new ChromeDriver(options);
            // create an instance of the webdriver waiter.
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(50));
            // maximize the window.
            _driver.Manage().Window.Maximize();
            // if the cookie was not retrieved successfully.
            if (GetCookie(_captcha) == false)
            {
                Console.WriteLine("Failed to retrieve authentication cookie.");
                return;
            }

            // Try to login.
            if (Login(_username, _password) == false)
            {
                return;
            }

            Thread.Sleep(5000);

            // Retrieve the game urls.
            foreach (var url in GetFreeGamesUrls())
            {
                var status = Status.Failed;

                for (var i = 0; i < 5; i++)
                {
                    status = ClaimGame(url);

                    if (status == Status.Success)
                    {
                        SendTelegram(url, status);
                        break;
                    }
                    if (status == Status.Owned)
                    {
                        break;
                    }
                }
                if (status == Status.Failed)
                {
                    SendTelegram(url, status);
                }
            }

            Console.WriteLine("process finished");
        }

        private static bool ValidateArguments()
        {
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_captcha))
            {
                Console.WriteLine("missing arguments");

                return false;
            }
            try
            {
                var mailAddress = new MailAddress(_username);
            }
            catch (FormatException)
            {
                Console.WriteLine("email address is not valid");

                return false;
            }

            if (new Regex("^https:\\/\\/accounts.hcaptcha.com\\/verify_email\\/[0-9a-z-]+$").IsMatch(_captcha) == false)
            {
                Console.WriteLine("captcha url is not valid");

                return false;
            }

            return true;
        }

        private static bool GetCookie(string url)
        {
            const int maxTries = 5;

            _driver.Navigate().GoToUrl(url);

            Thread.Sleep(5000);

            for (var i = 0; i < maxTries; i++)
            {
                Console.Write($"{i + 1}/{maxTries} retrieving cookie : ");

                GetElement("//button[@title=\"Klicka för att ställa in cookie för tillgänglighet\"]").Click();  // GetElement("//button[@title=\"Click to set accessibility cookie\"]").Click();

                Thread.Sleep(7500);

                if (_driver.PageSource.Contains("Cookie set."))
                {
                    Console.WriteLine("success");

                    return true;
                }

                Console.WriteLine("failed");
            }

            return false;
        }

        private static void AddEpicCookies()
        {
            _driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(
                name: "HAS_ACCEPTED_AGE_GATE_ONCE",
                value: "true",
                domain: "www.epicgames.com",
                path: "/",
                expiry: DateTime.Now.AddHours(1)
            ));
            _driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(
                name: "OptanonAlertBoxClosed",
                value: "en-US",
                domain: ".epicgames.com",
                path: "/",
                expiry: DateTime.Now.AddHours(1)
            ));
        }

        private static bool Login(string user, string pass)
        {
            const int maxTries = 5;

            _driver.Navigate().GoToUrl("https://www.epicgames.com/id/login/");

            GetElement("//div[@aria-label=\"Sign in with Epic Games\"]").Click();

            AddEpicCookies();

            var loginUrl = _driver.Url;

            for (var i = 0; i < maxTries; i++)
            {
                Console.Write($"{i + 1}/{maxTries} Logging in : ");

                Thread.Sleep(2000);

                var nameField = GetElement("//input[@name=\"email\"]");
                var passField = GetElement("//input[@name=\"password\"]");

                nameField.Clear();
                passField.Clear();

                nameField.SendKeys(user);
                passField.SendKeys(pass);

                Thread.Sleep(1000);

                if (_driver.Url != loginUrl)
                {
                    Console.WriteLine("success");

                    return true;
                }

                try
                {
                    GetElement("//span[text()=\"Log in now\"]").Click();

                    Thread.Sleep(20000);
                }
                catch
                {
                    // ignored
                }

                if (_driver.Url != loginUrl)
                {
                    Console.WriteLine("success");

                    return true;
                }

                Console.WriteLine("failed");
            }

            return false;
        }

        private static IEnumerable<string> GetFreeGamesUrls()
        {
            _driver.Navigate().GoToUrl("https://www.epicgames.com/store/en-US/free-games");

            _wait.Until(x => x.FindElement(By.XPath("//div[@data-component=\"CardGridDesktopBase\"]")).Displayed);

            Thread.Sleep(10000);

            var urls = GetElements("//a[descendant::span[text()='Free Now']]")
                .Select(element => element.GetAttribute("href")).ToList();

            return urls;
        }

        private static Status ClaimGame(string url)
        {
            Console.Write($"claiming {url} : ");

            _driver.Navigate().GoToUrl(url);

            Thread.Sleep(5000);

            if (_driver.PageSource.Contains("Owned</span>"))
            {
                Console.WriteLine("already owned");

                return Status.Owned;
            }

            try
            {
                // Click the get button.
                GetElement("//button[@data-testid=\"purchase-cta-button\"]").Click();
                Thread.Sleep(20000);
                if (_driver.PageSource.ToLower().Contains("please read this agreement carefully"))
                {
                    GetElement("//input[@id=\"agree\"]").Click();
                    Thread.Sleep(1000);
                    GetElement("//button[descendant::span[text()='Accept']]").Click();
                    Thread.Sleep(1000);
                    GetElement("//button[@data-testid=\"purchase-cta-button\"]").Click();
                    Thread.Sleep(20000);
                }

                // Click place order button
                GetElement("//button[@class=\"btn btn-primary\"]").Click();
                Thread.Sleep(20000);
                // click the agree button
                GetElements("//button[@class=\"btn btn-primary\"]")[1].Click();
                Thread.Sleep(5000);

                Console.WriteLine("Claimed");
                return Status.Success;
            }
            catch
            {
                Console.WriteLine("Failed");

                return Status.Failed;
            }
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

        private static IWebElement GetElement(string xPath)
        {
            try
            {
                _wait.Until(x => x.FindElement(By.XPath(xPath)));

                return _driver.FindElement(By.XPath(xPath));
            }
            catch
            {
                return null;
            }
        }

        private static void SendTelegram(string url, Status status)
        {
            Console.Write("Sending telegram message... ");
            if (string.IsNullOrEmpty(_telegram))
            {
                return;
            }
            try
            {
                var messageData = JsonConvert.SerializeObject(new
                {
                    Id = Convert.ToInt32(_telegram),
                    Url = url,
                    Status = status,
                });
                new HttpClient().PostAsync(Url, new StringContent(
                    messageData,
                    Encoding.UTF8,
                    "application/json"
                )).Wait();
                Console.WriteLine("success.");
            }
            catch
            {
                Console.WriteLine("failed.");
            }
        }
    }

    public enum Status
    {
        // only status 0 and 1 can be passed to the telegram api.
        Success = 0,

        Failed = 1,
        Owned = 2,
    }*/
}