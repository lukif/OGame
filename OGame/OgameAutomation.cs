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


        public OgameAutomation(ChromeDriver driver)
        {
            _driver = driver;
            OGameURL = "http://ogame.pl";
            wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 15));
        }

        public void Login(string user, string password, string serverName = "Tarazed")
        {
            _driver.Navigate().GoToUrl(OGameURL);

            var LoginPopup = _driver.FindElement(By.Id("loginBtn"));
            LoginPopup.Click();

            var loginField = _driver.FindElement(By.Id("usernameLogin"));
            var passwordField = _driver.FindElement(By.Id("passwordLogin"));
            var serverSelect = _driver.FindElement(By.Id("serverLogin"));
            var loginButton = _driver.FindElement(By.Id("loginSubmit"));

            loginField.SendKeys(user);
            passwordField.SendKeys(password);
            serverSelect.SendKeys(serverName);
            loginButton.Click();
            Thread.Sleep(2000);
        }

        public List<Attack> GetAttacks()
        {
            attacks = new List<Attack>();

            int aaaa = _driver.FindElements(By.Id("js_eventDetailsClosed")).Count;

            if (_driver.FindElements(By.Id("js_eventDetailsClosed")).Count > 0)
            {
                var expandEventsList = _driver.FindElement(By.Id("js_eventDetailsClosed"));
                expandEventsList.Click();
                Thread.Sleep(1000);

                wait.Until(ExpectedConditions.ElementExists(By.Id("eventContent")));
                var events = _driver.FindElements(By.XPath("//*[@id='eventContent']/tbody/tr"));

                foreach (var currentEvent in events)
                {
                    var fromPlanet = currentEvent.FindElement(By.XPath("//td[4]"));

                    if (!fromPlanet.Text.Contains("Columbo 6"))
                    {
                        string attackTime = currentEvent.FindElement(By.XPath("//td[2]")).Text;
                        string attacker = currentEvent.FindElement(By.XPath("//td[5]")).Text;
                        string attackedPlanet = currentEvent.FindElement(By.XPath("//td[9]")).Text;

                        attacks.Add(new Attack(attacker, attackedPlanet, attackTime));
                    }
                }
            }

            return attacks;
        }

        public void EscapeFromPlanet(Attack attack)
        {
            string coords = attack.attackedPlanet.Replace("[", "").Replace("]", "");
            string galaxy  = attack.attackedPlanet.Split(':')[0];
            string sunSystem = attack.attackedPlanet.Split(':')[1];
            string planetPosition = attack.attackedPlanet.Split(':')[2];


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

                var firstPlanet = _driver.FindElement(By.XPath("//ul/li[2]/a[contains(., 'Columbo')]"));
                firstPlanet.Click();

                var continueButton2 = _driver.FindElement(By.Id("continue"));
                continueButton2.Click();

                var stayOnThePlanetOption = _driver.FindElement(By.Id("missionButton4"));
                stayOnThePlanetOption.Click();

                var packEverythingOnShips = _driver.FindElement(By.Id("allresources"));
                packEverythingOnShips.Click();

                var sendOutFleet = _driver.FindElement(By.Id("start"));
                sendOutFleet.Click();

                Console.WriteLine("Fleet from " + attack.attackedPlanet + " is safe and flies to " + firstPlanet.Text);

                attack.safe = true;
            }
        }

        public void BackOnPlanet(Attack attack)
        {
            var fleetStatusButton = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/span/a"));
            fleetStatusButton.Click();

            if (_driver.FindElements(By.XPath("//span[5]/span[1]/a[contains(., '" + attack.attackedPlanet + "')]/../../../span[9]/a")).Count != 0)
            {
                var returnButton = _driver.FindElement(By.XPath("//span[5]/span[1]/a[contains(., '" + attack.attackedPlanet + "')]/../../../span[9]/a"));
                returnButton.Click();
                Console.WriteLine("Fleet is flying back to planet");
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
    }
}
