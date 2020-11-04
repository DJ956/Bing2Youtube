using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Text.RegularExpressions;

namespace Bing2YoutubeURL
{
    class Program
    {

        private const string YOUTUBE_LINK = "https://www.youtube.com";

        private static Regex REG = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
        
        public static List<string> LoadBing(string path)
        {
            var urls = new List<string>();
            using (var reader = new StreamReader(path))
            {
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    line = line.Replace(" ", "");

                    if (REG.IsMatch(line))
                    {
                        urls.Add(line);
                    }                
                }
            }

            return urls;
        }

        private static string ConvertYoutube(IWebDriver driver, string bingUrl)
        {
            driver.Navigate().GoToUrl(bingUrl);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            wait.Until(e => e.FindElement(By.TagName("a")));
            var aElms = driver.FindElements(By.TagName("a"));

            foreach(var elm in aElms)
            {
                if (elm.GetAttribute("href").Contains(YOUTUBE_LINK))
                {
                    var link = elm.GetAttribute("href");
                    driver.Navigate().GoToUrl(link);                    
                    break;
                }
            }            

            wait.Until(e => e.FindElement(By.TagName("a")));

            return driver.Url;
        } 

        private static void WriteUrls(string path, List<string> urls)
        {
            using(var writer = new StreamWriter(path))
            {
                foreach(var url in urls)
                {
                    writer.WriteLine(url);
                }
                writer.Flush();
            }
        }

        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Usage:Bing2YoutubeURL.exe urlsrc dst");
                return;
            }

            var urls = LoadBing(args[0]);
            var dst = args[1];
            //var urls = LoadBing(@"C:\Users\dexte\Downloads\sample.txt");
            //var dst = "./dst.txt";


            var results = new List<string>();
            using(var driver = new ChromeDriver())
            {
                for(int i = 0; i < urls.Count; i++)
                {
                    try
                    {
                        var yUrl = ConvertYoutube(driver, urls[i]);
                        results.Add(yUrl);
                        Console.WriteLine($"{i}/{urls.Count} [+]:{yUrl}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{i}/{urls.Count} [-]:{urls[i]}");
                        Console.WriteLine(e.Message);
                    }                                        
                }

                WriteUrls(dst, results);
                Console.WriteLine("done.");
                Console.ReadLine();
            }
        }
    }
}
