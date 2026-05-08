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
		Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

		// Give the app up to 30s after the Appium session starts to render
		// its first frame and populate the accessibility tree. On Android
		// the Appium session is established the instant the launcher
		// activity begins, but MAUI's main page may take several seconds
		// to draw — running tests immediately produces NoSuchElement on
		// every lookup.
		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
		while (DateTime.UtcNow < deadline)
		{
			try
			{
				var src = Driver.PageSource ?? string.Empty;
				if (src.Contains("Header") || src.Contains("LiveEditor") || src.Contains("BBCode"))
					return;
			}
			catch { /* tree may be transiently empty */ }
			Thread.Sleep(500);
		}

		// Dump page source for diagnosis if the app never produced our IDs.
		try
		{
			var src = Driver.PageSource ?? "<null>";
			TestContext.Progress.WriteLine("=== PageSource (first 4000 chars) ===");
			TestContext.Progress.WriteLine(src.Length > 4000 ? src.Substring(0, 4000) : src);
		}
		catch { /* ignore */ }
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

