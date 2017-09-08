using System;
using System.Collections.Generic;
using System.Configuration;
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
            string user = ConfigurationManager.AppSettings["user"];
            string password = ConfigurationManager.AppSettings["password"];
            string server = ConfigurationManager.AppSettings["server"];

            while (true)
            {
                Console.WriteLine("\n" + DateTime.Now);
                ChromeDriver driver = new ChromeDriver();
                driver.Manage().Window.Maximize();

                OgameAutomation game = new OgameAutomation(driver);

                game.Login(user, password, server);
                List<Attack> attacks = game.GetAttacks();

                if (attacks.Count != 0)
                {
                    foreach (var attack in attacks)
                    {
                        string attackTime = attack.attackTime;

                        int attackHour = Convert.ToInt32(attackTime.Split(':')[0]);
                        int attackMinute = Convert.ToInt32(attackTime.Split(':')[1]);
                        int attackSecond = Convert.ToInt32(attackTime.Split(':')[2]);

                        DateTime now = DateTime.Now;
                        DateTime attackTimeDT = new DateTime(now.Year, now.Month, now.Day, attackHour, attackMinute,
                            attackSecond);

                        var diffInSeconds = (attackTimeDT - now).TotalSeconds;

                        while (diffInSeconds >= 60)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(DateTime.Now + " - Time to atttack: {0}s", Math.Round(diffInSeconds));
                            Console.ForegroundColor = ConsoleColor.White;
                            Thread.Sleep(5000);
                            diffInSeconds = (attackTimeDT - DateTime.Now).TotalSeconds;
                        }

                        if (!game.CheckIfLoggedIn())
                        {
                            game.Login(user, password, server);
                        }

                        game.EscapeFromPlanet(attack);
                    }

                    string lastAttackTime = attacks.Last().attackTime;

                    int lastAttackHour = Convert.ToInt32(lastAttackTime.Split(':')[0]);
                    int lastAttackMinute = Convert.ToInt32(lastAttackTime.Split(':')[1]);
                    int lastAttackSecond = Convert.ToInt32(lastAttackTime.Split(':')[2]);

                    DateTime now2 = DateTime.Now;
                    DateTime lastAttackTimeDT = new DateTime(now2.Year, now2.Month, now2.Day, lastAttackHour, lastAttackMinute, lastAttackSecond);

                    var lastDiffInSeconds = (lastAttackTimeDT - now2).TotalSeconds;

                    while (lastDiffInSeconds > 0)
                    {
                        Thread.Sleep(3000);
                        lastDiffInSeconds = (lastAttackTimeDT - DateTime.Now).TotalSeconds;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0} - Waiting for the end of attack: {1}s", DateTime.Now, Math.Round(lastDiffInSeconds));
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    foreach (var attack in attacks)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(DateTime.Now + " - Attack {0} is finished." , attacks.IndexOf(attack)+1);
                        Console.ForegroundColor = ConsoleColor.White;

                        if (!game.CheckIfLoggedIn())
                        {
                            game.Login(user, password, server);
                        }

                        game.BackOnPlanet(attack);
                    }
                }

                if (!game.CheckIfLoggedIn())
                {
                    game.Login(user, password, server);
                }

                if (game.GetAttacks().Count == 0)
                {
                    game.BuildBattleship();

                    driver.Quit();

                    int currentMinute = DateTime.Now.Minute;

                    while (currentMinute % 15 != 0)
                    {
                        Thread.Sleep(5000);
                        currentMinute = DateTime.Now.Minute;
                    }
                    Thread.Sleep(60000);
                }
                else
                {
                    driver.Quit();
                }
            }
        }
    }
}
