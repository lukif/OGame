using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebPages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Selenium;
using Selenium.Helper;

namespace OGame
{
    class OgameAutomation
    {
        string partOfPlanetName = ConfigurationManager.AppSettings["partOfPlanetName"];

        private readonly ChromeDriver _driver;
        private readonly string _OGameURL;
        private readonly WebDriverWait _wait;
        private List<Attack> _attacks;
        private List<Expedition> _expeditions;
        private List<string> _listOfPlanets;
        private List<string> _planetDestinationWhileEscape = new List<string>();
        private List<int> planetResources;


        public OgameAutomation(ChromeDriver driver)
        {
            _driver = driver;
            _OGameURL = ConfigurationManager.AppSettings["url"]; ;
            _wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 60));
        }

        public void Login(string user, string password, string serverName = "Tarazed")
        {
            _driver.Navigate().GoToUrl(_OGameURL);

            var LoginPopup = _driver.FindElement(By.Id("loginBtn"));
            if (LoginPopup.Text != "Zamknij")
            {
                LoginPopup.Click();
            }

            Thread.Sleep(1000);

            var loginField = _driver.FindElement(By.Id("usernameLogin"));
            var passwordField = _driver.FindElement(By.Id("passwordLogin"));
            var serverSelect = _driver.FindElement(By.Id("serverLogin"));
            var loginButton = _driver.FindElement(By.Id("loginSubmit"));

            loginField.SendKeys(user);
            passwordField.SendKeys(password);
            serverSelect.SendKeys(serverName);
            loginButton.Click();
            Thread.Sleep(2000);
            _listOfPlanets = GetPlanetList();
        }

        public List<Attack> GetAttacks()
        {
            _attacks = new List<Attack>();

            if (_driver.FindElements(By.XPath("//*[@id='eventboxFilled']/p/p/span[contains(.,'Następna')]")).Count > 0)
            {

                var expandEventsList = _driver.FindElement(By.Id("js_eventDetailsClosed"));
                if (expandEventsList.Displayed)
                {
                    expandEventsList.Click();
                }
                Thread.Sleep(1000);

                _wait.Until(ExpectedConditions.ElementExists(By.Id("eventContent")));
                var events = _driver.FindElements(By.XPath("//*[@id='eventContent']/tbody/tr"));

                Console.WriteLine(DateTime.Now + " - Currently we see {0} mission(s) in space.", events.Count);

                foreach (var currentEvent in events)
                {
                    var fromPlanet = currentEvent.FindElement(By.XPath("./td[5]"));
                    var fromPlanetName = currentEvent.FindElement(By.XPath("./td[4]"));
                    if (!_listOfPlanets.Contains(fromPlanet.Text))
                    {
                        string attackTime = currentEvent.FindElement(By.XPath("./td[2]")).Text.Replace(" Czas", "");
                        string attacker = currentEvent.FindElement(By.XPath("./td[5]")).Text;
                        string attackedPlanet = currentEvent.FindElement(By.XPath("./td[9]")).Text;
                        string attackedPlenetMoon = currentEvent.FindElement(By.XPath("./td[8]")).Text;

                        if (attacker != "[3:330:4]")
                        {
                            Thread.Sleep(2000);
                            var toolTip = currentEvent.FindElement(By.XPath("./td[7]/span"));

                            Actions action = new Actions(_driver);
                            action.MoveToElement(toolTip).Perform();
                            toolTip.Click();
                            Thread.Sleep(500);

                            var toolTipTable = _driver.FindElements(By.XPath("//div[3]/div/div[2]/table/tbody/tr"));

                            bool spyAttack = false;

                            if (toolTipTable.Count == 2)
                            {
                                if (toolTipTable[1].Text.Contains("Sonda"))
                                {
                                    spyAttack = true;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(DateTime.Now + " - Spy attack from {0}", attacker);
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }

                            if (!spyAttack)
                            {
                                if (attackedPlenetMoon.Contains("Columbo"))
                                {
                                    _attacks.Add(new Attack(attacker, attackedPlanet, attackTime));
                                }
                                else
                                {
                                    _attacks.Add(new Attack(attacker, attackedPlanet, attackTime, true));
                                }
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine(DateTime.Now + " - Transport from {0} detected", attacker);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                if (_attacks.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(DateTime.Now + " - {0} attack(s). First on {1} (Moon: {2}) - {3}", _attacks.Count, _attacks.First().attackedPlanet, _attacks.First().moon, _attacks.First().attackTime);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.WriteLine(DateTime.Now + " - There is no mission in space at the moment.");
            }

            return _attacks;
        }

        public List<Expedition> GetExpeditions()
        {
            _expeditions = new List<Expedition>();

            if (_driver.FindElements(By.XPath("//*[@id='eventboxFilled']/p/p/span[contains(.,'Następna')]")).Count > 0)
            {

                var expandEventsList = _driver.FindElement(By.Id("js_eventDetailsClosed"));
                if (expandEventsList.Displayed)
                {
                    expandEventsList.Click();
                }
                Thread.Sleep(1000);

                _wait.Until(ExpectedConditions.ElementExists(By.Id("eventContent")));
                var events = _driver.FindElements(By.XPath("//*[@id='eventContent']/tbody/tr"));

                foreach (var currentEvent in events)
                {
                    var destinationPlanet = currentEvent.FindElement(By.XPath("./td[8]"));
                    var destinationDirection = currentEvent.FindElement(By.XPath("./td[7]"));
                    if (destinationPlanet.Text == "Ekspedycja" && destinationDirection.GetAttribute("class") == "icon_movement_reserve")
                    {
                        string system = currentEvent.FindElement(By.XPath("./td[9]")).Text;
                        system = system.Split(':')[1];

                        _expeditions.Add(new Expedition(system));
                    }
                }
            }

            string exp = string.Empty;
            if (_expeditions.Count > 0)
            {

                foreach (var expedition in _expeditions)
                {
                    if (!exp.IsEmpty())
                    {
                        exp += ", ";
                    }
                    exp += expedition.GetSystem();
                }
            }

            Console.WriteLine("{0} - We have {1} expeditions in space ({2})",DateTime.Now, _expeditions.Count, exp);

            return _expeditions;
        }

        public void EscapeFromPlanet(Attack attack)
        {
            bool escape = true;
            if (!attack.moon)
            {
                SelectPlanet(attack.attackedPlanet);
            }
            else if (attack.attackedPlanet.Contains("330:6"))
            {
                GoToFirstMoon(true);
            }
            else
            {
                escape = false;
            }

            if (escape)
            {
                Thread.Sleep(1000);

                var fleetTab = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/a"));
                fleetTab.Click();

                var noShipsOnPlanet = _driver.FindElements(By.XPath("//*[@id='warning']/h3"));
                if (noShipsOnPlanet.Count == 0)
                {
                    if (_driver.FindElements(By.Id("sendall")).Count > 0)
                    {
                        var sendAllButton = _driver.FindElement(By.Id("sendall"));
                        sendAllButton.Click();

                        //var malymysliwiec = _driver.FindElement(By.XPath("//*[@id='military']/li/input"));
                        //malymysliwiec.SendKeys("1");

                        //var leaveSmallShips = _driver.FindElement(By.XPath("//div[@id='battleships']/ul/li"));
                        //if (leaveSmallShips.GetAttribute("class") == "off")
                        //{

                        //}

                        var continueButton1 = _driver.FindElement(By.Id("continue"));

                        if (continueButton1.GetAttribute("Class") == "on")
                        {
                            continueButton1.Click();

                            Thread.Sleep(1000);

                            var usefulLinksSelect =
                                _driver.FindElement(By.XPath("//*[@id='shortcuts']/div[1]/div/span/a"));
                            usefulLinksSelect.Click();

                            var firstPlanet = _driver
                                .FindElements(By.XPath("//ul/li/a[contains(., '" + partOfPlanetName + "')]"))
                                .First();
                            _wait.Until(ExpectedConditions.ElementToBeClickable(firstPlanet));
                            _planetDestinationWhileEscape.Add(firstPlanet.Text);
                            firstPlanet.Click();

                            var speed10 = _driver.FindElement(By.XPath("//*[@id='speedLinks']/a[1]"));
                            speed10.Click();

                            Thread.Sleep(500);
                            var continueButton2 = _driver.FindElement(By.Id("continue"));
                            continueButton2.Click();

                            _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("missionButton4")));
                            var stayOnThePlanetOption = _driver.FindElement(By.Id("missionButton4"));
                            stayOnThePlanetOption.Click();

                            Thread.Sleep(500);
                            var packEverythingOnShips = _driver.FindElement(By.Id("allresources"));
                            packEverythingOnShips.Click();

                            var sendOutFleet = _driver.FindElement(By.Id("start"));
                            sendOutFleet.Click();

                            attack.safe = true;

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(DateTime.Now + " - Fleet from {0} (Moon: {2}) is safe and flies to {1}",
                                attack.attackedPlanet, _planetDestinationWhileEscape.Last(), attack.moon);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(DateTime.Now + " - There is no ship on {0}", attack.attackedPlanet);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now + " - Moon attacked on {0}", attack.attackedPlanet);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void BackOnPlanet(Attack attack)
        {
            _driver.Navigate().GoToUrl("https://s146-pl.ogame.gameforge.com/game/index.php?page=movement");

            _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='inhalt']/div[4]/span[1]/a/span[2]")));

            if (_planetDestinationWhileEscape.Count > 0)
            {
                _planetDestinationWhileEscape[0] = "[" + _planetDestinationWhileEscape.First().Split('[').Last();
            }

            string returnButtonXpath = "//div[span='Stacjonuj']/span[11]/span[a='" + _planetDestinationWhileEscape.First() +
                                       "']/../../span[5]/span[1]/a[contains(., '" + attack.attackedPlanet +
                                       "')]/../../../span[9]/a";

            var aaaaa = _driver.FindElements(By.XPath(returnButtonXpath));
            Console.WriteLine(aaaaa.Count);

            if (_driver.FindElements(By.XPath(returnButtonXpath)).Count != 0)
            {
                var returnButton = _driver.FindElement(By.XPath(returnButtonXpath));

                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

                returnButton.Click();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(DateTime.Now + " - Fleet is flying back from {0} to planet {1}", _planetDestinationWhileEscape.First(), attack.attackedPlanet);
                Console.ForegroundColor = ConsoleColor.White;
                _planetDestinationWhileEscape.RemoveAt(0);
            }
            else
            {
                Console.WriteLine(DateTime.Now + " - Unable to return fleet.");
            }
        }

        public void SelectPlanet(string planetCoords)
        {
            string planetXPath = "//a[span='" + planetCoords + "']";

            var planetToSelect = _driver.FindElement(By.XPath(planetXPath));
            planetToSelect.Click();
        }

        public List<string> GetPlanetList()
        {
            var planets = _driver.FindElements(By.XPath("//*[@id='planetList']/div/a/span[2]"));
            List<string> listOfPlanets = new List<string>();

            foreach (var planet in planets)
            {
               listOfPlanets.Add(planet.Text);
            }

            return listOfPlanets;
        }


        public void GoToFirstMoon(bool attacked = false)
        {
            if (!attacked)
            {
                var moon = _driver.FindElement(By.XPath("//*[@id='planetList']/div/a[2]"));
                moon.Click();
            }
            else
            {
                var moon = _driver.FindElement(By.XPath("//*[@id='planetList']/div/a[3]"));
                moon.Click();
            }
        }

        public void GoToMoon()
        {
            var moon = _driver.FindElement(By.XPath("//*[@id='planetList']/div/a[2]"));
            moon.Click();
        }

        public bool CheckIfLoggedIn()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("window.scrollTo(0,0);");

            var logiLink = _driver.FindElement(By.Id("logoLink"));
            logiLink.Click();

            if (_driver.Url == _OGameURL)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public List<int> GetPlanetResources()
        {
            planetResources = new List<int>();

            int metal = Convert.ToInt32(_driver.FindElement(By.Id("resources_metal")).Text.Replace(".",""));
            int crystal = Convert.ToInt32(_driver.FindElement(By.Id("resources_crystal")).Text.Replace(".", ""));
            int deuterium = Convert.ToInt32(_driver.FindElement(By.Id("resources_deuterium")).Text.Replace(".", ""));

            planetResources.Add(metal);
            planetResources.Add(crystal);
            planetResources.Add(deuterium);

            return planetResources;
        }

        public void SendResourcesToFirstPlanet(int metal, int crystal, int deuter)
        {
            if (deuter > 70000)
            {
                deuter -= 70000;
            }
            else
            {
                deuter = 0;
            }
            int transporters = Math.Abs((metal + crystal + deuter) / 25000);

            var fleetStatusButton1 = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/a"));
            fleetStatusButton1.Click();

            Thread.Sleep(2000);

            var warning1 = _driver.FindElements(By.XPath("//*[@id='warning']/h3"));

            if (warning1.Count == 0)
            {
                var bigTransporter1 = _driver.FindElement(By.XPath("//*[@id='civil']/li[2]"));
                if (bigTransporter1.GetAttribute("class").Contains("on"))
                {
                    var selectBigTransporter = _driver.FindElement(By.XPath("//*[@id='civil']/li[2]/input"));
                    selectBigTransporter.SendKeys(transporters.ToString());
                }

                var continueButton1 = _driver.FindElement(By.Id("continue"));

                if (continueButton1.GetAttribute("class").Contains("on"))
                {
                    continueButton1.Click();
                    Thread.Sleep(1000);

                    var usefulLinksSelect = _driver.FindElement(By.XPath("//*[@id='shortcuts']/div[1]/div/span/a"));
                    usefulLinksSelect.Click();

                    var firstPlanet = _driver
                        .FindElements(By.XPath("//ul/li/a[contains(., '" + partOfPlanetName + "')]"))
                        .First();
                    _wait.Until(ExpectedConditions.ElementToBeClickable(firstPlanet));
                    _planetDestinationWhileEscape.Add(firstPlanet.Text);
                    firstPlanet.Click();

                    Thread.Sleep(500);

                    var continueButton2 = _driver.FindElement(By.Id("continue"));
                    continueButton2.Click();

                    Thread.Sleep(2000);

                    var transportButton = _driver.FindElement(By.Id("missionButton3"));
                    transportButton.Click();

                    var maxMetal = _driver.FindElement(By.XPath("//*[@id='resources']/div[1]/div[2]/a[2]"));
                    maxMetal.Click();
                    var maxCrystal = _driver.FindElement(By.XPath("//*[@id='resources']/div[2]/div[2]/a[2]"));
                    maxCrystal.Click();

                    int deuterToSend = 0;
                    if (deuter > 70000)
                    {
                        var deuterium = _driver.FindElement(By.Id("deuterium"));
                        deuterToSend = deuter - 70000;
                        deuterium.SendKeys((deuterToSend).ToString());
                    }

                    Thread.Sleep(500);
                    var start = _driver.FindElement(By.Id("start"));
                    start.Click();

                    Thread.Sleep(3000);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{0} - Resources ({1}, {2}, {3}) sent to planet", DateTime.Now, metal, crystal, deuterToSend);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }


        public void BuildBattleship()
        {
            foreach (var planet in GetPlanetList())
            {
                SelectPlanet(planet);

                var shipyardButton = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[6]/a/span"));
                shipyardButton.Click();

                Thread.Sleep(3000);

                if (_driver.FindElements(By.XPath("//*[@id='military']/li[4]/div/div/a")).Count > 0)
                {
                    var selectBattleship = _driver.FindElement(By.XPath("//*[@id='military']/li[4]/div/div/a"));

                    if (!selectBattleship.GetAttribute("class").Contains("active"))
                    {
                        selectBattleship.Click();
                    }
                }
                else if (_driver.FindElements(By.XPath("//*[@id='military']/li[4]/div/div/div/a[2]")).Count > 0)
                {
                    var selectBattleship = _driver.FindElement(By.XPath("//*[@id='military']/li[4]/div/div/div/a[2]"));

                    if (!selectBattleship.GetAttribute("class").Contains("active"))
                    {
                        selectBattleship.Click();
                    }
                }
                
                Thread.Sleep(2000);

                string buildButtonXpath = "//*[@id='content']/div[3]/a[@class='build-it']";

                if (_driver.FindElements(By.XPath(buildButtonXpath)).Count > 0)
                {
                    var numberOfShipsToBuild = _driver.FindElement(By.Id("number"));
                    numberOfShipsToBuild.SendKeys("10");

                    var build = _driver.FindElement(By.XPath(buildButtonXpath));
                    build.Click();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("{0} - started to build battleship on {1}.", DateTime.Now, planet);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        public void BuildLightFighter()
        {
            foreach (var planet in GetPlanetList())
            {
                SelectPlanet(planet);

                var shipyardButton = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[6]/a/span"));
                shipyardButton.Click();

                Thread.Sleep(3000);

                if (_driver.FindElements(By.XPath("//*[@id='military']/li[1]/div/div/a")).Count > 0)
                {
                    var selectLightFighter = _driver.FindElement(By.XPath("//*[@id='military']/li[1]/div/div/a"));

                    if (!selectLightFighter.GetAttribute("class").Contains("active"))
                    {
                        selectLightFighter.Click();
                    }
                }
                else if (_driver.FindElements(By.XPath("//*[@id='military']/li[1]/div/div/div/a[2]")).Count > 0)
                {
                    var selectBattleship = _driver.FindElement(By.XPath("//*[@id='military']/li[1]/div/div/div/a[2]"));

                    if (!selectBattleship.GetAttribute("class").Contains("active"))
                    {
                        selectBattleship.Click();
                    }
                }

                Thread.Sleep(2000);

                string buildButtonXpath = "//*[@id='content']/div[3]/a[@class='build-it']";

                if (_driver.FindElements(By.XPath(buildButtonXpath)).Count > 0)
                {
                    var numberOfShipsToBuild = _driver.FindElement(By.Id("number"));
                    numberOfShipsToBuild.SendKeys("50");

                    var build = _driver.FindElement(By.XPath(buildButtonXpath));
                    build.Click();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("{0} - started to build light fighter(s) on {1}.", DateTime.Now, planet);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }


        public void SendExpedition(string expeditionSystem)
        {
            //SelectPlanet(GetPlanetList().First());
            GoToFirstMoon();

            Thread.Sleep(3000);

            var fleetStatusButton = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/a"));
            fleetStatusButton.Click();

            Thread.Sleep(2000);

            var warning = _driver.FindElements(By.XPath("//*[@id='warning']/h3"));

            if (warning.Count == 0)
            {
                var destroyer = _driver.FindElement(By.XPath("//*[@id='military']/li[7]"));
                if (destroyer.GetAttribute("class").Contains("on"))
                {
                    var selectDestroyer = _driver.FindElement(By.XPath("//*[@id='military']/li[7]/input"));
                    selectDestroyer.SendKeys("1");
                }

                var bigTransporter = _driver.FindElement(By.XPath("//*[@id='civil']/li[2]"));
                if (bigTransporter.GetAttribute("class").Contains("on"))
                {
                    var selectBigTransporter = _driver.FindElement(By.XPath("//*[@id='civil']/li[2]/input"));
                    selectBigTransporter.SendKeys("250");
                }

                var sond = _driver.FindElement(By.XPath("//*[@id='civil']/li[5]"));
                if (sond.GetAttribute("class").Contains("on"))
                {
                    var selectSond = _driver.FindElement(By.XPath("//*[@id='civil']/li[5]/input"));
                    selectSond.SendKeys("10");
                }

                var continueButton = _driver.FindElement(By.Id("continue"));

                if (continueButton.GetAttribute("class").Contains("on"))
                {
                    continueButton.Click();
                    Thread.Sleep(1000);

                    var system = _driver.FindElement(By.Id("system"));
                    system.SendKeys(expeditionSystem);

                    var position = _driver.FindElement(By.Id("position"));
                    position.SendKeys("16");

                    Thread.Sleep(500);

                    var continueButton2 = _driver.FindElement(By.Id("continue"));
                    continueButton2.Click();

                    Thread.Sleep(2000);

                    var expeditionButton = _driver.FindElement(By.Id("missionButton15"));
                    expeditionButton.Click();

                    Thread.Sleep(500);
                    var start = _driver.FindElement(By.Id("start"));
                    start.Click();

                    Thread.Sleep(3000);

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("{0} - Expeditoin sent to 3:{1}:16", DateTime.Now, expeditionSystem);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("{0} - Unable to send expedition - Fleet limit achieved", DateTime.Now);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("{0} - Curently there are no ships on the planet", DateTime.Now);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
