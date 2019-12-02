using System;
using System.Collections.ObjectModel;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
            
            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("img.user-photo-header")).Displayed);
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

            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            //Looking for "Add Project" button
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("div.entitly-listing-buttons-left > button")).Displayed);

            //WHEN
            IWebElement newProjectButton = driver.FindElement(By.CssSelector("div.entitly-listing-buttons-left > button"));
            newProjectButton.Click();

            webDriverWait.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);

            IWebElement saveProjectButton = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action"));
            saveProjectButton.Click();

            //THEN
            webDriverWait.Until(driver => driver.FindElement(By.Id("form-error-container")).Displayed);

        }

        [Test]
        public void Given_IsOnProjects_When_TryToSaveProjectWithValidInputs_Then_NewProjectIsInTable()
        {
            //GIVEN
            LoginToRukovoditel();
            IWebElement projectsButton = driver.FindElement(By.CssSelector("ul.page-sidebar-menu > li:nth-child(4)"));
            projectsButton.Click();
            
            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            //Looking for "Add Project" button
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("div.entitly-listing-buttons-left > button")).Displayed);

            //WHEN
            string projectId = $"vrad00{Guid.NewGuid().ToString()}";
            string date = $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}";

            IWebElement newProjectButton = driver.FindElement(By.CssSelector("div.entitly-listing-buttons-left > button"));
            newProjectButton.Click();

            webDriverWait.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);

            SelectElement projectPrioritySelect = new SelectElement(driver.FindElementById("fields_156"));
            projectPrioritySelect.SelectByValue("35");

            SelectElement projectStatusSelect = new SelectElement(driver.FindElementById("fields_157"));
            projectStatusSelect.SelectByValue("37");

            IWebElement projectNameInput = driver.FindElementById("fields_158");
            projectNameInput.SendKeys(projectId);

            IWebElement projectStartDateInput = driver.FindElementById("fields_159");
            projectStartDateInput.SendKeys(date);

            IWebElement saveProjectButton = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action"));
            saveProjectButton.Click();

            //THEN
            webDriverWait.Until(driver => driver.FindElement(By.ClassName("fieldtype_action-th")));
            FindAndDeleteProject(projectId);

        }

        private void FindAndDeleteProject(string projectId)
        {
            //Starting on Tasks view
            IWebElement projectsBreadcrumb =  driver.FindElement(By.CssSelector("ul.page-breadcrumb > li:nth-child(1) > a"));
            projectsBreadcrumb.Click();
            //Looking for "Add Project" button
            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("#entity_items_listing66_21 table > tbody > tr")));
            ReadOnlyCollection<IWebElement> projects = driver.FindElements(By.CssSelector("#entity_items_listing66_21 table > tbody > tr"));
            bool projectFoundInTable = false;
            foreach (var webElement in projects)
            {
                if (webElement.FindElement(By.ClassName("field-158-td")).Text == projectId)
                {
                    //Confirm that we have found the project
                    projectFoundInTable = true;
                    //Delete button
                    webElement.FindElement(By.CssSelector(".fieldtype_action > a:nth-child(1)")).Click();
                    webDriverWait.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);
                    //Confirm delete button
                    driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action")).Click();
                }                
            }
            Assert.That(projectFoundInTable, Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}
