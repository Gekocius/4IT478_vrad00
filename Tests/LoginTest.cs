using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace VSE.Rukovoditel.Tests
{
    [TestFixture]
    public class LoginTest
    {
        private ChromeDriver driver;
        private string password;
        private string username;
        private readonly string RUKOVODITEL_URL = "https://digitalnizena.cz/rukovoditel/";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LoadTestData();
        }

        [SetUp]
        public void SetUp()
        {
            driver = new ChromeDriver("chromedriver_win32/");
            driver.Manage().Window.Maximize();
        }

        private void LoadTestData()
        {
            using (var sw = new StreamReader("testData.txt"))
            {
                username = sw.ReadLine();
                password = sw.ReadLine();
            }
        }

        private void NavigateToLoginPage()
        {
            driver.Navigate().GoToUrl(RUKOVODITEL_URL);
        }

        private void Login(string username, string password)
        {
            IWebElement userNameField = driver.FindElement(By.Name("username"));
            IWebElement passwordField = driver.FindElement(By.Name("password"));
            IWebElement loginButton = driver.FindElement(By.CssSelector("button[type=submit]"));

            userNameField.SendKeys(username);
            passwordField.SendKeys(password);
            loginButton.Click();
        }

        [Test]
        public void Given_IsOnLoginPage_Then_LoginUsingValidCredentials_Then_IsLoggedIn()
        {
            //GIVEN
            NavigateToLoginPage();
            Assert.That(driver.FindElement(By.CssSelector("body.login")), Is.Not.Null);

            //WHEN
            Login(username, password);

            //THEN
            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("img.user-photo-header")).Displayed);
        }

        public void Given_IsOnLoginPage_Then_LoginUsingInvalidCredentials_Then_AlertIsDisplayed()
        {
            //GIVEN
            NavigateToLoginPage();
            Assert.That(driver.FindElement(By.CssSelector("body.login")), Is.Not.Null);

            //WHEN
            Login(username, "invalidPassword");

            //THEN
            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("div.alert.alert-danger")).Displayed);
        }

        [Test]
        public void Given_IsLoggedIn_Then_LogOut_Then_IsBackOnLoginPage()
        {
            //GIVEN
            Given_IsOnLoginPage_Then_LoginUsingValidCredentials_Then_IsLoggedIn();

            //WHEN
            Actions actions = new Actions(driver);
            IWebElement dropdown = driver.FindElement(By.CssSelector("li.dropdown.user"));
            actions.MoveToElement(dropdown).MoveToElement(dropdown.FindElement(By.CssSelector("i.fa.fa-sign-out"))).Click().Build().Perform();

            //THEN
            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("body.login")).Displayed);
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}
