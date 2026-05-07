using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace TR.BBCodeLabel.Maui.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>();

#if ANDROID
		// On Android, MAUI sets AutomationId only as a private View tag —
		// it does NOT propagate to content-description, so Appium's
		// AccessibilityId selector cannot find elements set up purely via
		// AutomationId. Mirror it onto ContentDescription so E2E tests
		// using MobileBy.AccessibilityId(automationId) work out of the box.
		ViewHandler.ViewMapper.AppendToMapping(
			nameof(IView.AutomationId),
			(handler, view) =>
			{
				if (handler.PlatformView is Android.Views.View aView
					&& !string.IsNullOrEmpty(view.AutomationId))
				{
					aView.ContentDescription = view.AutomationId;
				}
			});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
