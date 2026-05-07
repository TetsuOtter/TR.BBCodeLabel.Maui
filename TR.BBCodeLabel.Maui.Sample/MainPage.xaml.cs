using TR.BBCode.Parser;

namespace TR.BBCodeLabel.Maui.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
#if ANDROID
		// Android: AutomationId only sets a private View tag — it does NOT
		// propagate to content-description, so Appium's accessibility id
		// selector finds nothing. Mirror it onto SemanticProperties.Description
		// here (Android maps that to ContentDescription).
		// Done in code-behind, not XAML, so iOS isn't affected: on iOS
		// SemanticProperties.Description sets accessibilityLabel, which would
		// shadow the displayed text from element.Text in tests.
		MirrorAutomationIdsToDescription(this);
#endif
	}

#if ANDROID
	static void MirrorAutomationIdsToDescription(IVisualTreeElement root)
	{
		foreach (var child in root.GetVisualChildren())
		{
			if (child is VisualElement ve && !string.IsNullOrEmpty(ve.AutomationId))
			{
				if (string.IsNullOrEmpty(SemanticProperties.GetDescription(ve)))
					SemanticProperties.SetDescription(ve, ve.AutomationId);
			}
			MirrorAutomationIdsToDescription(child);
		}
	}
#endif

	void OnLiveEditorTextChanged(object? sender, TextChangedEventArgs e)
	{
		string text = e.NewTextValue ?? string.Empty;
		LivePreview.BBCodeText = text;
		LivePreviewPlain.Text = SafeToPlainText(text);
	}

	static string SafeToPlainText(string bbcode)
	{
		if (string.IsNullOrEmpty(bbcode))
			return string.Empty;
		try
		{
			return BBCodeParser.Parse(bbcode).ToPlainText();
		}
		catch (BBCodeParserException)
		{
			return bbcode;
		}
	}
}
