using NUnit.Framework;
using OpenQA.Selenium.Appium;

namespace TR.BBCodeLabel.Maui.E2ETests;

[SetUpFixture]
public class AppiumServerLifecycle
{
	internal static AppiumDriver? Driver { get; private set; }

	[OneTimeSetUp]
	public void SetUp()
	{
		Driver = DriverFactory.Create();
		// Keep ImplicitWait small so FindByAutomationId's retry+swipe loop
		// actually gets to iterate (otherwise each FindElement blocks 15s
		// before throwing and the loop only runs once).
		Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
	}

	[OneTimeTearDown]
	public void TearDown()
	{
		var driver = Driver;
		Driver = null;
		try { driver?.Quit(); }
		catch { /* swallow during teardown */ }
		try { driver?.Dispose(); }
		catch { /* swallow during teardown */ }
	}
}
