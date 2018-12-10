using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenQA;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using DiscordWebhookLib;

namespace EZMonitor
{   
    class Program
    {
        //Variables//
        static List<SiteItem> ItemList = new List<SiteItem>();
        static string[] SupremeLinks = { "jackets", "shirts", "tops_sweaters", "sweatshirts", "pants", "hats", "accessories", "shoes", "skate" };
        static IWebDriver driver;

        //Exit Events//
        private delegate bool ConsoleCtrlHandlerDelegate(int sig);
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handler, bool add);
        static ConsoleCtrlHandlerDelegate _consoleCtrlHandler;

        //Functions//
        static void ColorLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static bool ExistsInList(SiteItem item)
        {
            bool x = false;
            foreach(SiteItem Item in ItemList)
            {
                if(Item.Name == item.Name && Item.Color == item.Color)
                {
                    x = true;
                }
            }
            return x;
        }

        private static bool IsElementPresent(IWebElement element)
        {
            try
            {
                element.FindElement(By.ClassName("sold_out_tag"));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            driver.Quit();
        }

        static void Main(string[] args)
        {
            //EXIT HANDLER//
            _consoleCtrlHandler += s =>
            {
                driver.Quit();
                return false;
            };
            SetConsoleCtrlHandler(_consoleCtrlHandler, true);

            //CHROME SETTINGS//
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless"); //windowless
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2); //no images
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            driver = new ChromeDriver(chromeDriverService, options);
            DiscordWebhookExecutor executor = new DiscordWebhookExecutor("[Insert Webhook Link]");

            while (true)
            {
                foreach(string supremelink in SupremeLinks)
                {                   
                    driver.Navigate().GoToUrl("https://supremenewyork.com/shop/all/" + supremelink);
                    var Container = driver.FindElement(By.Id("container"));
                    foreach (var article in Container.FindElements(By.TagName("article")))
                    {
                        var ItemName = article.FindElement(By.TagName("p")).FindElement(By.ClassName("name-link")).Text;
                        var ItemColor = article.FindElement(By.TagName("h1")).FindElement(By.ClassName("name-link")).Text;
                        bool SoldOut = IsElementPresent(article);
                        var ItemLink = article.FindElement(By.TagName("a")).GetAttribute("href");

                        SiteItem temp = new SiteItem(ItemName, ItemColor, SoldOut);
                        if(ExistsInList(temp))
                        {
                            int index = ItemList.FindIndex(a => a.Name == ItemName && a.Color == ItemColor);
                            bool oldSoldOut = ItemList[index].SoldOut;

                            if (SoldOut != oldSoldOut)
                            {
                                ItemList.RemoveAt(index);
                                ItemList.Add(temp);
                                if(!SoldOut)
                                {
                                    ColorLine("[RESTOCK] [" + DateTime.Now + "] " + ItemName + " " + ItemColor, ConsoleColor.Green);                                  
                                    executor.Execute("[RESTOCK] [" + DateTime.Now + "] " + ItemName + " " + ItemColor + "Link: " + ItemLink, "EZMonitor", null, false, false);
                                }
                                else
                                {
                                    ColorLine("[SOLD OUT]" + ItemName + " " + ItemColor, ConsoleColor.Red);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("[Item] " + ItemName + " " + ItemColor + " " + SoldOut.ToString());
                            ItemList.Add(temp);
                        }
                    }
                }                  
            }
        }
    }
}
