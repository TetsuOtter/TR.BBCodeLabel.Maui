using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Windows;

namespace TR.BBCodeLabel.Maui.E2ETests;

internal static class DriverFactory
{
	public const string PlatformEnvVar = "APPIUM_PLATFORM";
	public const string ServerUrlEnvVar = "APPIUM_SERVER_URL";
	public const string AppPathEnvVar = "APP_PATH";
	public const string AppPackageEnvVar = "APP_PACKAGE";
	public const string AppActivityEnvVar = "APP_ACTIVITY";
	public const string DeviceNameEnvVar = "APPIUM_DEVICE_NAME";
	public const string PlatformVersionEnvVar = "APPIUM_PLATFORM_VERSION";
	public const string UdidEnvVar = "APPIUM_UDID";

	public static AppiumDriver Create()
	{
		string platform = (Environment.GetEnvironmentVariable(PlatformEnvVar) ?? "android").ToLowerInvariant();
		Uri serverUri = new(Environment.GetEnvironmentVariable(ServerUrlEnvVar) ?? "http://127.0.0.1:4723/");

		return platform switch
		{
			"android" => CreateAndroidDriver(serverUri),
			"ios" => CreateIosDriver(serverUri),
			"windows" => CreateWindowsDriver(serverUri),
			_ => throw new NotSupportedException($"Unknown platform '{platform}' (set env var '{PlatformEnvVar}' to one of: android, ios, windows)"),
		};
	}

	static AndroidDriver CreateAndroidDriver(Uri serverUri)
	{
		var options = new AppiumOptions
		{
			AutomationName = "UiAutomator2",
			PlatformName = "Android",
			DeviceName = Environment.GetEnvironmentVariable(DeviceNameEnvVar) ?? "Android Emulator",
		};

		string? platformVersion = Environment.GetEnvironmentVariable(PlatformVersionEnvVar);
		if (!string.IsNullOrEmpty(platformVersion))
			options.PlatformVersion = platformVersion;

		string? appPath = Environment.GetEnvironmentVariable(AppPathEnvVar);
		string? appPackage = Environment.GetEnvironmentVariable(AppPackageEnvVar);

		if (!string.IsNullOrEmpty(appPath))
		{
			options.App = appPath;
		}
		else if (!string.IsNullOrEmpty(appPackage))
		{
			options.AddAdditionalAppiumOption("appPackage", appPackage);
			string? appActivity = Environment.GetEnvironmentVariable(AppActivityEnvVar);
			if (!string.IsNullOrEmpty(appActivity))
				options.AddAdditionalAppiumOption("appActivity", appActivity);
		}
		else
		{
			throw new InvalidOperationException($"Either '{AppPathEnvVar}' (path to .apk) or '{AppPackageEnvVar}' must be set.");
		}

		options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
		options.AddAdditionalAppiumOption("appium:autoGrantPermissions", true);

		return new AndroidDriver(serverUri, options, TimeSpan.FromMinutes(3));
	}

	static IOSDriver CreateIosDriver(Uri serverUri)
	{
		string? appPath = Environment.GetEnvironmentVariable(AppPathEnvVar)
			?? throw new InvalidOperationException($"Env var '{AppPathEnvVar}' must point to the built .app bundle for iOS.");

		var options = new AppiumOptions
		{
			AutomationName = "XCUITest",
			PlatformName = "iOS",
			DeviceName = Environment.GetEnvironmentVariable(DeviceNameEnvVar) ?? "iPhone 15",
			App = appPath,
		};

		string? platformVersion = Environment.GetEnvironmentVariable(PlatformVersionEnvVar);
		if (!string.IsNullOrEmpty(platformVersion))
			options.PlatformVersion = platformVersion;

		string? udid = Environment.GetEnvironmentVariable(UdidEnvVar);
		if (!string.IsNullOrEmpty(udid))
			options.AddAdditionalAppiumOption("appium:udid", udid);

		options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
		options.AddAdditionalAppiumOption("appium:wdaLaunchTimeout", 600000);
		options.AddAdditionalAppiumOption("appium:wdaConnectionTimeout", 600000);
		options.AddAdditionalAppiumOption("appium:wdaStartupRetries", 4);
		options.AddAdditionalAppiumOption("appium:wdaStartupRetryInterval", 20000);
		options.AddAdditionalAppiumOption("appium:simulatorStartupTimeout", 600000);
		options.AddAdditionalAppiumOption("appium:useNewWDA", false);
		options.AddAdditionalAppiumOption("appium:showXcodeLog", true);
		options.AddAdditionalAppiumOption("appium:autoAcceptAlerts", true);
		options.AddAdditionalAppiumOption("appium:waitForQuiescence", false);
		options.AddAdditionalAppiumOption("appium:reduceMotion", true);
		options.AddAdditionalAppiumOption("appium:waitForIdleTimeout", 0);
		options.AddAdditionalAppiumOption("appium:simpleIsVisibleCheck", true);

		return new IOSDriver(serverUri, options, TimeSpan.FromMinutes(15));
	}

	static WindowsDriver CreateWindowsDriver(Uri serverUri)
	{
		string? appPath = Environment.GetEnvironmentVariable(AppPathEnvVar)
			?? throw new InvalidOperationException($"Env var '{AppPathEnvVar}' must point to the .exe (or AppId) of the Windows app.");

		var options = new AppiumOptions
		{
			AutomationName = "Windows",
			PlatformName = "Windows",
			DeviceName = "WindowsPC",
			App = appPath,
		};

		options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);

		return new WindowsDriver(serverUri, options, TimeSpan.FromMinutes(3));
	}
}
