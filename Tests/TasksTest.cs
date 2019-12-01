using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace VSE.Rukovoditel.Tests
{
    class TasksTest
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

        private void NavigateToProjectView(string projectId)
        {
            LoginToRukovoditel();
            IWebElement projectsButton = driver.FindElement(By.CssSelector("ul.page-sidebar-menu > li:nth-child(4)"));
            projectsButton.Click();

            //Looking for "Add Project" button
            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("#entity_items_listing66_21 table > tbody > tr")));
            ReadOnlyCollection<IWebElement> projects = driver.FindElements(By.CssSelector("#entity_items_listing66_21 table > tbody > tr"));
            foreach (var webElement in projects)
            {
                var projectNameField = webElement.FindElement(By.CssSelector(".field-158-td > a"));
                if (projectNameField.Text == projectId)
                {
                    projectNameField.Click();
                    break;
                }
            }
            //Looking for "Work hours" field
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("th.fieldtype_input_numeric_comments-th")));
        }

        private void CreateTask(string nameText, string descriptionText = null, string typeValue = "42",
                    string statusValue = "46", string priorityValue = "55")
        {
            IWebElement addTaskButton = driver.FindElement(By.CssSelector("div.entitly-listing-buttons-left > button.btn.btn-primary"));
            addTaskButton.Click();
            WebDriverWait webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            webDriverWait.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);

            SelectElement type = new SelectElement(driver.FindElement(By.Id("fields_167")));
            type.SelectByValue(typeValue);

            IWebElement name = driver.FindElement(By.Id("fields_168"));
            name.SendKeys(nameText);

            SelectElement status = new SelectElement(driver.FindElement(By.Id("fields_169")));
            status.SelectByValue(statusValue);

            SelectElement priority = new SelectElement(driver.FindElement(By.Id("fields_170")));
            priority.SelectByValue(priorityValue);

            if (descriptionText != null)
            {
                driver.SwitchTo().Frame(driver.FindElement(By.TagName("iframe")));
                IWebElement description = driver.FindElement(By.CssSelector("body[contenteditable]"));
                description.SendKeys(descriptionText);
                driver.SwitchTo().DefaultContent();
            }

            IWebElement saveTaskButton = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action"));
            saveTaskButton.Click();

            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("div#entity_items_listing487_22 table > tbody > tr")));
        }

        [Test]
        public void Given_IsOnProjectView_When_CreatesTaskAndOpenInfo_Then_InfoIsCorrectOnInfoView()
        {
            //GIVEN
            NavigateToProjectView("vrad00");

            //WHEN

            CreateTask("vrad00_testTask", "Descriptive text", "42", "46", "55");

            IWebElement infoButton = driver.FindElement(By.CssSelector(".fieldtype_action > a:nth-child(3)"));
            infoButton.Click();

            WebDriverWait webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("div.heading > h4.media-heading")));

            //THEN

            // Check values

            IWebElement infoName = driver.FindElement(By.CssSelector("div.caption"));
            Assert.That(infoName.Text, Is.EqualTo("vrad00_testTask"));

            IWebElement infoDescription = driver.FindElement(By.CssSelector("div.form-group-172 > div.fieldtype_textarea_wysiwyg"));
            Assert.That(infoDescription.Text, Is.EqualTo("Descriptive text"));

            IWebElement infoType = driver.FindElement(By.CssSelector("tr.form-group-167 > td > div"));
            Assert.That(infoType.Text, Is.EqualTo("Task"));

            IWebElement infoStatus = driver.FindElement(By.CssSelector("tr.form-group-169 > td > div"));
            Assert.That(infoStatus.Text, Is.EqualTo("New"));

            IWebElement infoPriority = driver.FindElement(By.CssSelector("tr.form-group-170 > td > div"));
            Assert.That(infoPriority.Text, Is.EqualTo("Medium"));

            // Delete task

            IWebElement moreActions = driver.FindElement(By.CssSelector("div.btn-group >button.btn.btn-default.btn-sm.dropdown-toggle"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(moreActions).MoveToElement(driver.FindElement(By.CssSelector("i.fa.fa-trash-o"))).Click().Build().Perform();

            webDriverWait.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);

            IWebElement deleteTaskConfirm = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action"));
            deleteTaskConfirm.Click();
        }

        private void DeleteAllFilters()
        {
            ReadOnlyCollection<IWebElement> deleteAllFilters = driver.FindElements(By.CssSelector("div.portlet.portlet-filters-preview.noprint > div.portlet-body > ul > li:nth-child(1) > a:nth-child(1) > i"));
            if (deleteAllFilters.Count == 1)
            {
                deleteAllFilters.First().Click();
            }
        }

        private void SetDefaultFilter()
        {
            DeleteAllFilters();
            IWebElement filterDropdown = driver.FindElement(By.ClassName("btn-users-filters")); 
            Actions actions = new Actions(driver);
            actions.MoveToElement(filterDropdown).MoveToElement(driver.FindElement(By.CssSelector(".dropdown-menu i.fa.fa-angle-right"))).Click().Build().Perform();
        }

        [Test]
        public void MultipleTasks()
        {
            //GIVEN

            NavigateToProjectView("vrad00");

            //WHEN

            for (int i = 46; i < 53; i++)
            {
                CreateTask($"{i}_vrad00_task", statusValue: i.ToString());
            }
            SetDefaultFilter();

            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("div.col-md-5.col-sm-12")));
            ReadOnlyCollection<IWebElement> taskElemets = driver.FindElements(By.CssSelector("table.table.table-striped.table-bordered.table-hover > tbody > tr"));
            Assert.That(taskElemets, Has.Count.EqualTo(3));

            IWebElement editFilters = driver.FindElement(By.ClassName("filters-preview-box"));
            editFilters.Click();

            webDriverWait.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);

            IWebElement openFilter = driver.FindElement(By.CssSelector("a.search-choice-close[data-option-array-index=\"1\"]"));
            openFilter.Click();

            IWebElement saveFilters = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action"));
            saveFilters.Click();

            webDriverWait.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed == false);

            //THEN
            //Check New and Waiting
            taskElemets = driver.FindElements(By.CssSelector("table.table.table-striped.table-bordered.table-hover > tbody > tr"));
            foreach (var webElement in taskElemets)
            {
                string statusText = webElement.FindElement(By.ClassName("field-169-td")).Text;
                Assert.That(statusText, Is.EqualTo("New").Or.EqualTo("Waiting"));
            }

            DeleteAllFilters();
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("div.col-md-5.col-sm-12")).Displayed);

            //Check that all tasks are present in the task table
            taskElemets = driver.FindElements(By.CssSelector("table.table.table-striped.table-bordered.table-hover > tbody > tr"));
            Assert.That(taskElemets, Has.Count.EqualTo(7));

            driver.FindElement(By.Id("uniform-select_all_items")).Click();
            webDriverWait.Until(driver => driver.FindElement(By.CssSelector("#uniform-select_all_items > span.checked")));

            IWebElement withSelectedDropdown = driver.FindElement(By.CssSelector(".btn-group > button.btn.btn-default.dropdown-toggle"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(withSelectedDropdown).MoveToElement(driver.FindElement(By.CssSelector("i.fa.fa-trash-o"))).Click().Build().Perform();

            webDriverWait.Until(driver => driver.FindElement(By.Id("ajax-modal")).Displayed);

            IWebElement deleteTaskConfirm = driver.FindElement(By.CssSelector("button.btn.btn-primary.btn-primary-modal-action"));
            deleteTaskConfirm.Click();
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}