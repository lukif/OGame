using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
                try
                {
                    Console.WriteLine("\n" + DateTime.Now);

                     ChromeOptions options = new ChromeOptions();
                    // options.AddArgument("--headless");
                     options.AddArgument("--silent");
                    // options.AddArgument("--disable-gpu");
                    // options.AddArgument("--log-level=3");
                     ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                     service.SuppressInitialDiagnosticInformation = true;

                    ChromeDriver driver = new ChromeDriver(service, options);
                    driver.Manage().Window.Maximize();

                    OgameAutomation game = new OgameAutomation(driver);
                    game.Login(user, password, server);

                    // FOR TEST
                    //Attack a = new Attack("[3:333:3]", "[3:330:6]", "17:21:30");
                    //Attack b = new Attack("[3:333:3]", "[3:330:6]", "17:22:00", true);

                    //game.EscapeFromPlanet(a);
                    //game.EscapeFromPlanet(b);

                    //game.BackOnPlanet(a);
                    //game.CheckIfLoggedIn();
                    //game.BackOnPlanet(b);

                    List<Attack> attacks = game.GetAttacks();
                    bool areWeAttacked = false;

                    //attacks.Add(a);
                    double timeToFirstAttack = 9999;
                    if (attacks.Count != 0)
                    {
                        int attackHour = Convert.ToInt32(attacks.First().attackTime.Split(':')[0]);
                        int attackMinute = Convert.ToInt32(attacks.First().attackTime.Split(':')[1]);
                        int attackSecond = Convert.ToInt32(attacks.First().attackTime.Split(':')[2]);

                        DateTime now = DateTime.Now;
                        DateTime attackTimeDT = new DateTime(now.Year, now.Month, now.Day, attackHour, attackMinute,
                            attackSecond);

                        timeToFirstAttack = (attackTimeDT - now).TotalSeconds;

                        if (timeToFirstAttack < 0)
                        {
                            attackTimeDT = new DateTime(now.Year, now.Month, now.Day + 1, attackHour, attackMinute,
                                attackSecond);
                            timeToFirstAttack = (attackTimeDT - now).TotalSeconds;
                        }
                    }

                    if (attacks.Count != 0 && timeToFirstAttack <= 660)
                    {
                        areWeAttacked = true;

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

                            if (diffInSeconds < 0)
                            {
                                attackTimeDT = new DateTime(now.Year, now.Month, now.Day + 1, attackHour, attackMinute,
                                    attackSecond);
                                diffInSeconds = (attackTimeDT - now).TotalSeconds;
                            }

                            while (diffInSeconds >= 60)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(DateTime.Now + " - Attacked planet: {0} Time to atttack: {1}s ({2})",
                                    attack.attackedPlanet, Math.Round(diffInSeconds), attack.attackTime);
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
                        DateTime lastAttackTimeDT = new DateTime(now2.Year, now2.Month, now2.Day, lastAttackHour,
                            lastAttackMinute, lastAttackSecond);

                        var lastDiffInSeconds = (lastAttackTimeDT - now2).TotalSeconds;

                        if (lastDiffInSeconds < 0)
                        {
                            lastAttackTimeDT = new DateTime(now2.Year, now2.Month, now2.Day + 1, lastAttackHour,
                                lastAttackMinute, lastAttackSecond);
                            lastDiffInSeconds = (lastAttackTimeDT - now2).TotalSeconds;
                        }

                        while (lastDiffInSeconds > 0)
                        {
                            Thread.Sleep(3000);
                            lastDiffInSeconds = (lastAttackTimeDT - DateTime.Now).TotalSeconds;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("{0} - Waiting for the end of attack ({2}): {1}s", DateTime.Now,
                                Math.Round(lastDiffInSeconds), attacks.Count);
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        foreach (var attack in attacks)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(DateTime.Now + " - Attack {0} is finished.", attacks.IndexOf(attack) + 1);
                            Console.ForegroundColor = ConsoleColor.White;

                            if (!game.CheckIfLoggedIn())
                            {
                                game.Login(user, password, server);
                            }

                            game.BackOnPlanet(attack);
                        }
                    }
                    else if (timeToFirstAttack <= 660)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(DateTime.Now + " - Attack detected on {0} (Moon: {1}) in {2}.", attacks.First().attackedPlanet, attacks.First().moon, timeToFirstAttack);
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    if (!areWeAttacked || game.GetAttacks().Count == 0)
                    {
                        int currentMinute = DateTime.Now.Minute;

                        if (currentMinute == 1)
                        {
                            //game.BuildBattleship();
                            //game.BuildLightFighter();
                        }

                        List<Expedition> expeditions = game.GetExpeditions();

                        int numberOfExpeditionToSend = 3;
                        if (game.CheckIfAdmiralEnabled())
                        {
                            numberOfExpeditionToSend = 4;
                        }

                        if (expeditions.Count < numberOfExpeditionToSend)
                        {
                            int expeditionsCount = expeditions.Count;
                            Random rand = new Random();

                            while (expeditionsCount < numberOfExpeditionToSend)
                            {
                                string system = rand.Next(328, 333).ToString();
                                game.SendExpedition(system);
                                expeditionsCount++;
                            }
                        }

                        if (attacks.Count != 0)
                        {
                            game.GoToFirstMoon(true);
                        }
                        else
                        {
                            game.GoToFirstMoon();
                        }  

                        List<int> resources = game.GetPlanetResources();

                        if (resources[0] > 0 || resources[1] > 0 || resources[2] > 100000)
                        {
                            game.SendResourcesToFirstPlanet(resources[0], resources[1], resources[2]);
                        }

                        //if (DateTime.Now.Hour % 2 == 0 && DateTime.Now.Minute < 15) {}

                        driver.Quit();

                        while (currentMinute % 10 != 0)
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
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    string[] a = new string[0];
                    Main(a);
                }
            }
        }
    }
}
