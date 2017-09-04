using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Selenium;
using Selenium.Helper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;


namespace OGame
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                ChromeDriver driver = new ChromeDriver();
                driver.Manage().Window.Maximize();

                OgameAutomation game = new OgameAutomation(driver);
                game.Login("Captain Columbo", "QWEqwe123");
                game.CheckIfEnbemyAttacks();

                driver.Quit();

                Thread.Sleep(600000);
                Console.WriteLine("Everything went fine.");
                Console.ReadKey();
            }
        }
    }
}
