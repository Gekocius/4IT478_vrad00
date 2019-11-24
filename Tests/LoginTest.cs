using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace VSE.Rukovoditel.Tests
{
    [TestFixture]
    class LoginTest
    {
        ChromeDriver Driver;

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void SomeTest()
        {
            ChromeDriver driver = new ChromeDriver("chromedriver_win32/");
            
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://google.com");

            var queryField = driver.FindElement(By.Name("q"));
            queryField.SendKeys("koloběžka");
            queryField.SendKeys(Keys.Enter);
        }
    }
}
