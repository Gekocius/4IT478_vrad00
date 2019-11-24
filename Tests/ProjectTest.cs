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
    class ProjectTest
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

        private void LoginToRukovoditel()
        {
            driver.Navigate().GoToUrl(RUKOVODITEL_URL);
            IWebElement userNameField = driver.FindElement(By.Name("username"));
            IWebElement passwordField = driver.FindElement(By.Name("password"));
            IWebElement loginButton = driver.FindElement(By.CssSelector("button[type=submit]"));

            userNameField.SendKeys(username);
            passwordField.SendKeys(password);
            loginButton.Click();
        }

        private void LoadTestData()
        {
            using (var sw = new StreamReader("testData.txt"))
            {
                username = sw.ReadLine();
                password = sw.ReadLine();
            }
        }

        [Test]
        public void Given_IsOnProjects_When_TryToSaveProjectWithoutName_Then_ErrorBoxAppears()
        {
            //GIVEN
            LoginToRukovoditel();
            IWebElement projectsButton = driver.FindElement(By.CssSelector("ul.page-sidebar-menu > li:nth-child(4)"));
            projectsButton.Click();

            //WHEN
            IWebElement newProjectButton = driver.FindElement(By.CssSelector("div.entitly-listing-buttons-left > button"));
            newProjectButton.Click();

            var webDriverWaitModal = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            webDriverWaitModal.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);

            IWebElement saveProjectButton = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action"));
            saveProjectButton.Click();

            //THEN
            var webDriverWaitError = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            webDriverWaitError.Until(driver => driver.FindElement(By.Id("form-error-container")).Displayed);

        }

        public void Given_IsOnProjects_When_TryToSaveProjectWithValidInputs_Then_ProjectAppearsInGrid()
        {
            //GIVEN
            LoginToRukovoditel();
            IWebElement projectsButton = driver.FindElement(By.CssSelector("ul.page-sidebar-menu > li:nth-child(4)"));
            projectsButton.Click();

            //WHEN
            string projectId = $"vrad00{new Guid().ToString()}";

            IWebElement newProjectButton = driver.FindElement(By.CssSelector("div.entitly-listing-buttons-left > button"));
            newProjectButton.Click();

            var webDriverWaitModal = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            webDriverWaitModal.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);

            IWebElement projectNameInput;
            IWebElement projectStatusInput;
            IWebElement projectPriorityInput;

            IWebElement saveProjectButton = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action"));
            saveProjectButton.Click();
        }

        [TearDown]
        private void TearDown()
        {
            driver.Quit();
        }
    }
}
