using TR.BBCode.Parser;

namespace TR.BBCodeLabel.Maui.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

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
