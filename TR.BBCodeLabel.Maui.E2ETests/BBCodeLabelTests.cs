using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace TR.BBCodeLabel.Maui.E2ETests;

[TestFixture]
public class BBCodeLabelTests
{
	AppiumDriver Driver => AppiumServerLifecycle.Driver
		?? throw new InvalidOperationException("Driver not initialised.");

	static string Platform => (Environment.GetEnvironmentVariable("APPIUM_PLATFORM") ?? "android").ToLowerInvariant();

	IWebElement FindByAutomationId(string automationId)
	{
		try { return Driver.FindElement(MobileBy.AccessibilityId(automationId)); }
		catch (WebDriverException) { /* fall through to scroll-and-retry */ }

		ScrollTo(automationId);
		return Driver.FindElement(MobileBy.AccessibilityId(automationId));
	}

	void ScrollTo(string automationId)
	{
		var args = Platform switch
		{
			"ios" => new Dictionary<string, object>
			{
				{ "direction", "down" },
				{ "predicateString", $"name == '{automationId}'" },
			},
			"android" => new Dictionary<string, object>
			{
				{ "strategy", "accessibility id" },
				{ "selector", automationId },
			},
			_ => null!,
		};
		if (args is null) return;
		try { ((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript("mobile: scroll", args); }
		catch { /* best-effort */ }
	}

	[Test]
	public void Header_DisplaysAppName()
	{
		var header = FindByAutomationId("Header");
		Assert.That(header.Text, Is.EqualTo("TR.BBCodeLabel.Maui"));
	}

	[TestCase("Sample_Bold",        "Bold text")]
	[TestCase("Sample_Italic",      "Italic text")]
	[TestCase("Sample_Underline",   "Underlined")]
	[TestCase("Sample_Strike",      "Strikethrough")]
	[TestCase("Sample_ColorNamed",  "red blue green")]
	[TestCase("Sample_ColorHex",    "hex orange")]
	[TestCase("Sample_ColorRgb",    "rgb pink 0x cyan")]
	[TestCase("Sample_ColorTheme",  "theme aware")]
	[TestCase("Sample_Size",        "12 18 24")]
	[TestCase("Sample_Font",        "Semibold via font tag")]
	[TestCase("Sample_Combined",    "Bold blue italic underline")]
	[TestCase("Sample_Escape",      "literal: [b]not bold[/b]")]
	[TestCase("Sample_Disable",     "bold not bold bold")]
	public void Sample_RendersExpectedPlainText(string automationId, string expected)
	{
		var label = FindByAutomationId(automationId);
		Assert.That(NormalizeWhitespace(label.Text), Is.EqualTo(expected));
	}

	[Test]
	public void LiveEditor_InitialPreview_MatchesPlainText()
	{
		var plain = FindByAutomationId("LivePreview_Plain");
		Assert.That(plain.Text, Is.EqualTo("Hello, World!"));
	}

	[Test]
	public void LiveEditor_TypingUpdatesPreview()
	{
		var editor = FindByAutomationId("LiveEditor");
		editor.Click();
		editor.Clear();
		editor.SendKeys("[i]Live[/i] [b]update[/b]!");

		var plain = FindByAutomationId("LivePreview_Plain");

		WaitUntil(() => plain.Text == "Live update!", TimeSpan.FromSeconds(10));
		Assert.That(plain.Text, Is.EqualTo("Live update!"));
	}

	static string NormalizeWhitespace(string? input)
		=> string.Join(' ', (input ?? string.Empty)
			.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

	static void WaitUntil(Func<bool> condition, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
		{
			try { if (condition()) return; }
			catch { /* retry */ }
			Thread.Sleep(250);
		}
	}
}
