using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
            if (bingUrl.Contains(YOUTUBE_LINK)) return bingUrl;

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

        [STAThread]
        static void Main(string[] args)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "URLリスト読み込み";
            openFileDialog.Filter = "URLリスト(*.tx)|*.txt";
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;            

            var saveFileFialog = new SaveFileDialog();
            saveFileFialog.Title = "変換URLリストの保存先";
            saveFileFialog.Filter = "URLリスト(*.tx)|*.txt";
            if (saveFileFialog.ShowDialog() != DialogResult.OK) return;

            var src = openFileDialog.FileName;
            var dst = saveFileFialog.FileName;

            var urls = LoadBing(src);


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
