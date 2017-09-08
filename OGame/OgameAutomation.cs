using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly ChromeDriver _driver;
        private readonly string _OGameURL;
        private readonly WebDriverWait _wait;
        private List<Attack> _attacks;
        private List<string> _listOfPlanets;
        private string _planetDestinationWhileEscape;


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
                var expandEventsList2 = _driver.FindElement(By.Id("js_eventDetailsClosed"));
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
                    if (!_listOfPlanets.Contains(fromPlanet.Text))
                    {
                        string attackTime = currentEvent.FindElement(By.XPath("./td[2]")).Text.Replace(" Czas", "");
                        string attacker = currentEvent.FindElement(By.XPath("./td[5]")).Text;
                        string attackedPlanet = currentEvent.FindElement(By.XPath("./td[9]")).Text;

                        _attacks.Add(new Attack(attacker, attackedPlanet, attackTime));
                    }
                }
                if (_attacks.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(DateTime.Now + " - {0} incoming attack(s) ", _attacks.Count);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.WriteLine(DateTime.Now + " - There is no mission in space at the moment.");
            }

            return _attacks;
        }

        public void EscapeFromPlanet(Attack attack)
        {
            // string coords = attack.attackedPlanet.Replace("[", "").Replace("]", "");
            //string galaxy  = coords.Split(':')[0];
            //string sunSystem = coords.Split(':')[1];
            //string planetPosition = coords.Split(':')[2];

            SelectPlanet(attack.attackedPlanet);

            Thread.Sleep(1000);

            var fleetTab = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/a"));
            fleetTab.Click();

            if (_driver.FindElements(By.Id("sendall")).Count > 0)
            {
                var sendAllButton = _driver.FindElement(By.Id("sendall"));
                sendAllButton.Click();

                var continueButton1 = _driver.FindElement(By.Id("continue"));


                if (continueButton1.GetAttribute("Class") == "on")
                {
                    continueButton1.Click();

                    var usefulLinksSelect = _driver.FindElement(By.XPath("//*[@id='shortcuts']/div[1]/div/span/a"));
                    usefulLinksSelect.Click();

                    _wait.Until(
                        ExpectedConditions.ElementToBeClickable(By.XPath("//ul/li[2]/a[contains(., 'Columbo')]")));
                    var firstPlanet = _driver.FindElement(By.XPath("//ul/li[2]/a[contains(., 'Columbo')]"));
                    _planetDestinationWhileEscape = firstPlanet.Text;
                    firstPlanet.Click();

                    Thread.Sleep(300);
                    var continueButton2 = _driver.FindElement(By.Id("continue"));
                    continueButton2.Click();

                    _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("missionButton4")));
                    var stayOnThePlanetOption = _driver.FindElement(By.Id("missionButton4"));
                    stayOnThePlanetOption.Click();

                    Thread.Sleep(300);
                    var packEverythingOnShips = _driver.FindElement(By.Id("allresources"));
                    packEverythingOnShips.Click();

                    var sendOutFleet = _driver.FindElement(By.Id("start"));
                    sendOutFleet.Click();

                    attack.safe = true;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(DateTime.Now + " - Fleet from {0} is safe and flies to {1}",
                        attack.attackedPlanet, _planetDestinationWhileEscape);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        public void BackOnPlanet(Attack attack)
        {
            var fleetStatusButton = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/span/a"));
            fleetStatusButton.Click();

            if (_planetDestinationWhileEscape != null)
            {
                _planetDestinationWhileEscape = "[" + _planetDestinationWhileEscape.Split('[').Last();
            }

            string returnButtonXpath = "//div[span='Stacjonuj']/span[11]/span[a='" + _planetDestinationWhileEscape +
                                       "']/../../span[5]/span[1]/a[contains(., '" + attack.attackedPlanet +
                                       "')]/../../../span[9]/a";

            if (_driver.FindElements(By.XPath(returnButtonXpath)).Count != 0)
            {
                var returnButton = _driver.FindElement(By.XPath(returnButtonXpath));

                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

                returnButton.Click();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(DateTime.Now + " - Fleet is flying back from {0} to planet {1}", _planetDestinationWhileEscape, attack.attackedPlanet);
                Console.ForegroundColor = ConsoleColor.White;
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

    }
}
