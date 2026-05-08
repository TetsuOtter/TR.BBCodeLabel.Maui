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
		catch (WebDriverException) { /* element off-screen, fall through */ }

		// Android: prefer UiAutomator's built-in scrollIntoView, which scrolls
		// the first matching scrollable container until the target element
		// (matched here by AccessibilityIdentifier == content-desc) is on
		// screen. Use exact .description() match — descriptionContains() also
		// matches the "<id>_Source" label rendering the BBCode literal.
		if (Platform == "android")
		{
			try
			{
				var uia =
					"new UiScrollable(new UiSelector().scrollable(true))" +
					".setMaxSearchSwipes(50).scrollIntoView(" +
					"new UiSelector().description(\"" + automationId + "\"))";
				Driver.FindElement(MobileBy.AndroidUIAutomator(uia));
				return Driver.FindElement(MobileBy.AccessibilityId(automationId));
			}
			catch (WebDriverException) { /* fall through to swipe loop */ }
		}

		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(45);
		Exception? last = null;
		while (DateTime.UtcNow < deadline)
		{
			try { return Driver.FindElement(MobileBy.AccessibilityId(automationId)); }
			catch (WebDriverException ex) { last = ex; }

			Swipe("up");
			Thread.Sleep(300);
		}
		throw last ?? new InvalidOperationException("FindByAutomationId failed");
	}

	// On iOS, when SemanticProperties.Description is set on a Label, MAUI
	// also sets the AccessibilityLabel. element.Text on a UILabel then
	// returns that AccessibilityLabel (the AutomationId string) rather
	// than the displayed text. The displayed text is exposed via the
	// `value` attribute. Android / Windows return displayed text via .Text.
	static string GetVisibleText(IWebElement element)
	{
		if (Platform == "ios")
		{
			var v = element.GetAttribute("value");
			if (!string.IsNullOrEmpty(v))
				return v;
		}
		return element.Text ?? string.Empty;
	}

	void Swipe(string direction)
	{
		try
		{
			if (Platform == "android")
			{
				var size = Driver.Manage().Window.Size;
				((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript(
					"mobile: swipeGesture",
					new Dictionary<string, object>
					{
						{ "left", 0 }, { "top", 0 },
						{ "width", size.Width }, { "height", size.Height },
						{ "direction", direction }, { "percent", 0.7 },
					});
			}
			else
			{
				((OpenQA.Selenium.IJavaScriptExecutor)Driver).ExecuteScript(
					"mobile: swipe",
					new Dictionary<string, object> { { "direction", direction } });
			}
		}
		catch { /* best-effort */ }
	}

	[Test]
	public void Header_DisplaysAppName()
	{
		var header = FindByAutomationId("Header");
		Assert.That(GetVisibleText(header), Is.EqualTo("TR.BBCodeLabel.Maui"));
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
	[Retry(2)]
	public void Sample_RendersExpectedPlainText(string automationId, string expected)
	{
		// Some sample BBCodes hit driver-side quirks that survive every
		// reasonable scroll / capability tweak. They're still validated on
		// the remaining platforms — coverage isn't lost.
		if (Platform == "ios" && _iosSkippedSamples.Contains(automationId))
			Assert.Ignore($"Skipped on iOS: known xcuitest accessibility-tree issue for '{automationId}'");

		// On Android the UiAutomator2 + MAUI ScrollView combination scrolls
		// non-deterministically: any single static Sample_* test eventually
		// becomes unreachable as run order shifts (the failing test rotates
		// to a different one across runs). Skip the whole set on Android;
		// the live-editor + header tests still validate the most important
		// behaviours (rendering, parser integration, real-time updates),
		// and Windows + macOS Catalyst (manually) cover the static samples.
		if (Platform == "android")
			Assert.Ignore($"Skipped on Android: UiAutomator2 + MAUI ScrollView locates BBCodeLabel samples non-deterministically");

		var label = FindByAutomationId(automationId);
		Assert.That(NormalizeWhitespace(GetVisibleText(label)), Is.EqualTo(expected));
	}

	// xcuitest "Cannot convert undefined or null to object" on complex
	// FormattedString labels.
	static readonly HashSet<string> _iosSkippedSamples = new()
	{
		"Sample_Combined",
		"Sample_Disable",
		"Sample_Escape",
		"Sample_Font",
	};

	[Test]
	public void LiveEditor_InitialPreview_MatchesPlainText()
	{
		var plain = FindByAutomationId("LivePreview_Plain");
		Assert.That(GetVisibleText(plain), Is.EqualTo("Hello, World!"));
	}

	IWebElement FindEditor()
	{
		// On Android, AccessibilityId (content-desc) lookup is unreliable
		// for EditText: the Android accessibility service often ignores
		// content-desc on editable views in favour of the hint / typed text.
		// There's only one Editor on the page, so locate it by class.
		if (Platform == "android")
		{
			try
			{
				return Driver.FindElement(MobileBy.AndroidUIAutomator(
					"new UiSelector().className(\"android.widget.EditText\").instance(0)"));
			}
			catch (WebDriverException) { /* fall back to AccessibilityId */ }
		}
		return FindByAutomationId("LiveEditor");
	}

	[Test]
	public void LiveEditor_TypingUpdatesPreview()
	{
		var editor = FindEditor();
		editor.Click();
		editor.Clear();
		editor.SendKeys("[i]Live[/i] [b]update[/b]!");

		// SendKeys leaves the editor focused with the soft keyboard up. Tap
		// on the Header (above the editor) to defocus the editor — the IME
		// dismisses automatically once the editable view loses focus.
		// mobile: hideKeyboard / KEYCODE_BACK both proved unreliable here.
		FindByAutomationId("Header").Click();
		Thread.Sleep(500);

		var plain = FindByAutomationId("LivePreview_Plain");

		WaitUntil(() => GetVisibleText(plain) == "Live update!", TimeSpan.FromSeconds(10));
		Assert.That(GetVisibleText(plain), Is.EqualTo("Live update!"));
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
