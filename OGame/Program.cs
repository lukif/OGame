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
            string user = System.Configuration.ConfigurationManager.AppSettings["user"];
            string password = System.Configuration.ConfigurationManager.AppSettings["password"];
            string server = System.Configuration.ConfigurationManager.AppSettings["server"];

            while (true)
            {
                Console.WriteLine(DateTime.Now);
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

                        DateTime attackTimeDT = new DateTime(now.Year, now.Month, now.Day, attackHour, attackMinute, attackSecond);

                        var diffInSeconds = (attackTimeDT - now).TotalSeconds;

                        while (diffInSeconds >= 60)
                        {
                            Console.WriteLine("Seconds to atttack: " + diffInSeconds);
                            Thread.Sleep(5000);
                            diffInSeconds = (attackTimeDT - DateTime.Now).TotalSeconds;
                        }

                        if (!game.CheckOfLoggedIn())
                        {
                            game.Login(user, password, server);
                        }

                        game.EscapeFromPlanet(attack);

                        while (diffInSeconds > 0)
                        {
                            Thread.Sleep(5000);
                            diffInSeconds = (attackTimeDT - DateTime.Now).TotalSeconds;
                        }

                        Console.WriteLine("Attack is finished.");

                        if (!game.CheckOfLoggedIn())
                        {
                            game.Login(user, password, server);
                        }

                        game.BackOnPlanet(attack);
                    }
                }

                driver.Quit();

                Random r = new Random();
                int randomSleepTime = r.Next(600000, 1000000);
                string waitTo = DateTime.Now.AddSeconds(Math.Round((double)randomSleepTime/1000)).ToString();
                Console.WriteLine("Next check in: " + Math.Round((double)randomSleepTime / 1000) + " seconds. (" + waitTo + ")");
                Thread.Sleep(randomSleepTime);
            }
        }
    }
}
