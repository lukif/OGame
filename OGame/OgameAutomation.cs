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
            Thread.Sleep(3000);
        }

        public bool CheckIfEnbemyAttacks()
        {
            if (_driver.FindElements(By.XPath("//*[@id='js_eventDetailsClosed']")).Count > 0)
            {
                var expandEventsList = _driver.FindElement(By.XPath("//*[@id='js_eventDetailsClosed']"));
                expandEventsList.Click();
                Thread.Sleep(3000);

                wait.Until(ExpectedConditions.ElementExists(By.Id("eventContent")));
                var events = _driver.FindElements(By.XPath("//*[@id='eventContent']/tbody/tr"));

                foreach (var currentEvent in events)
                {
                    var fromPlanet = currentEvent.FindElement(By.XPath("//td[4]"));

                    if (!fromPlanet.Text.Contains("Columbo 2"))
                    {
                        string attacker = currentEvent.FindElement(By.XPath("//td[5]")).Text;
                        string attackedPlanet = currentEvent.FindElement(By.XPath("//td[9]")).Text;

                        Console.WriteLine(attacker + " ATTACKS US!!!");

                        EscapeFromPlanet(attackedPlanet);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void EscapeFromPlanet(string attackedPlanetCoordinates)
        {
            attackedPlanetCoordinates = attackedPlanetCoordinates.Replace("[", "").Replace("]", "");
            string galaxy  = attackedPlanetCoordinates.Split(':')[0];
            string sunSystem = attackedPlanetCoordinates.Split(':')[1];
            string planetNumber = attackedPlanetCoordinates.Split(':')[2];

            var fleetTab = _driver.FindElement(By.XPath("//*[@id='menuTable']/li[8]/a"));
            fleetTab.Click();

            var sendAllButton = _driver.FindElement(By.Id("sendall"));
            sendAllButton.Click();

            var continueButton1 = _driver.FindElement(By.Id("continue"));
            continueButton1.Click();



            //*[@id="continue"]/span

        }

        public void BackOnPlanet()
        {

        }
    }
}
