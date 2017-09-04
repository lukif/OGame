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
                Console.WriteLine(DateTime.Now.ToString());
                ChromeDriver driver = new ChromeDriver();
                driver.Manage().Window.Maximize();

                OgameAutomation game = new OgameAutomation(driver);

                game.Login("Captain Columbo", "QWEqwe123");
                List<Attack> attacks = game.GetAttacks();

                if (attacks.Count != 0)
                {
                    foreach (var attack in attacks)
                    {
                        string attackTime = attack.attackTime;

                        int attackHour = Convert.ToInt32(attackTime.Replace("[", "").Split(':')[0]);
                        int attackMinute = Convert.ToInt32(attackTime.Replace("[", "").Split(':')[1]);
                        int attackSecond = Convert.ToInt32(attackTime.Replace("[", "").Split(':')[2]);

                        DateTime now = DateTime.Now;

                        DateTime attackTimeDT = new DateTime(now.Year, now.Month, now.Day, attackHour, attackMinute, attackSecond);

                        var diffInSeconds = (attackTimeDT - now).TotalSeconds;

                        while (diffInSeconds >= 30)
                        {
                            Console.WriteLine("Seconds to atttack: " + diffInSeconds);
                            Thread.Sleep(5000);
                            diffInSeconds = (attackTimeDT - DateTime.Now).TotalSeconds;
                        }

                        game.EscapeFromPlanet(attack);

                        while (diffInSeconds > 0)
                        {
                            Thread.Sleep(5000);
                            diffInSeconds = (attackTimeDT - DateTime.Now).TotalSeconds;
                        }

                        Console.WriteLine("Attack is finished.");

                        game.BackOnPlanet(attack);
                    }
                }

                driver.Quit();
                Thread.Sleep(90000);
            }
        }
    }
}
