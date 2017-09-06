using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Selenium;
using Selenium.Helper;

namespace OGame
{
    class OgameAutomation
    {
        private ChromeDriver _driver;
        private string OGameURL;
        private WebDriverWait wait;
        private List<Attack> attacks;
        private List<string> listOfPlanets;
        private string planetDestinationWhileEscape;


        public OgameAutomation(ChromeDriver driver)
        {
            _driver = driver;
            OGameURL = "http://ogame.pl";
            wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 60));
        }

        public void Login(string user, string password, string serverName = "Tarazed")
        {
            _driver.Navigate().GoToUrl(OGameURL);

            var LoginPopup = _driver.FindElement(By.Id("loginBtn"));
            if (LoginPopup.Text != "Zamknij")
            {
                LoginPopup.Click();
            }

            var loginField = _driver.FindElement(By.Id("usernameLogin"));
            var passwordField = _driver.FindElement(By.Id("passwordLogin"));
            var serverSelect = _driver.FindElement(By.Id("serverLogin"));
            var loginButton = _driver.FindElement(By.Id("loginSubmit"));

            loginField.SendKeys(user);
            passwordField.SendKeys(password);
            serverSelect.SendKeys(serverName);
            loginButton.Click();
            Thread.Sleep(2000);
            listOfPlanets = GetPlanetsList();
        }

        public List<Attack> GetAttacks()
        {
            attacks = new List<Attack>();

            if (_driver.FindElements(By.Id("eventboxBlank")).Count == 0)
            {
                var expandEventsList = _driver.FindElement(By.Id("js_eventDetailsClosed"));
                expandEventsList.Click();
                Thread.Sleep(1000);

                wait.Until(ExpectedConditions.ElementExists(By.Id("eventContent")));
                var events = _driver.FindElements(By.XPath("//*[@id='eventContent']/tbody/tr"));

                Console.WriteLine("Currently we have {0} mission(s) in space.", events.Count);

                foreach (var currentEvent in events)
                {
                    var fromPlanet = currentEvent.FindElement(By.XPath("./td[5]"));
                    if (!listOfPlanets.Contains(fromPlanet.Text))
                    {
                        string attackTime = currentEvent.FindElement(By.XPath("./td[2]")).Text.Replace(" Czas", "");
                        string attacker = currentEvent.FindElement(By.XPath("./td[5]")).Text;
                        string attackedPlanet = currentEvent.FindElement(By.XPath("./td[9]")).Text;

                        attacks.Add(new Attack(attacker, attackedPlanet, attackTime));
                    }
                }
            }
            else
            {
                Console.WriteLine("There is no mission in space at the moment.");
            }

            return attacks;
        }

        public void EscapeFromPlanet(Attack attack)
        {
            string coords = attack.attackedPlanet.Replace("[", "").Replace("]", "");
            string galaxy  = coords.Split(':')[0];
            string sunSystem = coords.Split(':')[1];
            string planetPosition = coords.Split(':')[2];

            SelectPlanet(attack.attackedPlanet);

            Thread.Sleep(1000);

            var fleetTab = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/a"));
            fleetTab.Click();

            var sendAllButton = _driver.FindElement(By.Id("sendall"));
            sendAllButton.Click();

            var continueButton1 = _driver.FindElement(By.Id("continue"));

            if (continueButton1.GetAttribute("Class") == "on")
            {
                continueButton1.Click();

                var usefulLinksSelect = _driver.FindElement(By.XPath("//*[@id='shortcuts']/div[1]/div/span/a"));
                usefulLinksSelect.Click();

                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//ul/li[2]/a[contains(., 'Columbo')]")));
                var firstPlanet = _driver.FindElement(By.XPath("//ul/li[2]/a[contains(., 'Columbo')]"));
                planetDestinationWhileEscape = firstPlanet.Text;
                firstPlanet.Click();

                Thread.Sleep(300);
                var continueButton2 = _driver.FindElement(By.Id("continue"));
                continueButton2.Click();

                wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("missionButton4")));
                var stayOnThePlanetOption = _driver.FindElement(By.Id("missionButton4"));
                stayOnThePlanetOption.Click();

                Thread.Sleep(300);
                var packEverythingOnShips = _driver.FindElement(By.Id("allresources"));
                packEverythingOnShips.Click();

                var sendOutFleet = _driver.FindElement(By.Id("start"));
                sendOutFleet.Click();

                attack.safe = true;

                Console.WriteLine("Fleet from " + attack.attackedPlanet + " is safe and flies to " + planetDestinationWhileEscape);
            }
        }

        public void BackOnPlanet(Attack attack)
        {
            var fleetStatusButton = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/span/a"));
            fleetStatusButton.Click();

            planetDestinationWhileEscape = planetDestinationWhileEscape.Split('[').Last();

            string returnButtonXpath = "//div[span='Stacjonuj']/span[11]/span[a='[" + planetDestinationWhileEscape +
                                       "']/../../span[5]/span[1]/a[contains(., '" + attack.attackedPlanet +
                                       "')]/../../../span[9]/a";

            if (_driver.FindElements(By.XPath(returnButtonXpath)).Count != 0)
            {
                var returnButton = _driver.FindElement(By.XPath(returnButtonXpath));
                returnButton.Click();

                Console.WriteLine("Fleet is flying back from [" + planetDestinationWhileEscape + " to planet " + attack.attackedPlanet);
            }
            else
            {
                Console.WriteLine("Unable to return fleet.");
            }
        }

        public void SelectPlanet(string planetCoords)
        {
            string planetXPath = "//a[span='" + planetCoords + "']";
            var planetToSelect = _driver.FindElement(By.XPath(planetXPath));
            planetToSelect.Click();
        }

        public List<string> GetPlanetsList()
        {
            var planets = _driver.FindElements(By.XPath("//*[@id='planetList']/div/a/span[2]"));
            List<string> listOfPlanets = new List<string>();

            foreach (var planet in planets)
            {
               listOfPlanets.Add(planet.Text);
            }

            return listOfPlanets;
        }

        public bool CheckOfLoggedIn()
        {
            var logiLink = _driver.FindElement(By.Id("logoLink"));
            logiLink.Click();

            if (_driver.Url == "https://pl.ogame.gameforge.com/")
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
