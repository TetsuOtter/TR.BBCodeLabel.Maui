using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using TR.BBCode.Parser;

namespace TR.BBCodeLabel.Maui;

public class BBCodeLabel : Label
{
	static readonly Dictionary<string, Color> _colorCache = [];
	static BBCodeLabel()
	{
		foreach (var prop in typeof(Colors).GetProperties())
		{
			if (prop.PropertyType == typeof(Color))
			{
				var color = prop.GetValue(null) as Color;
				if (color is not null && color != Colors.Transparent)
					_colorCache.Add(prop.Name.ToLower(), color);
			}
		}
	}

	public Color DefaultLightThemeTextColor { get; set; } = Colors.Black;
	public Color DefaultDarkThemeTextColor { get; set; } = Colors.White;

	public static readonly BindableProperty BBCodeTextProperty = BindableProperty.Create(
		nameof(BBCodeText),
		typeof(string),
		typeof(BBCodeLabel),
		defaultValue: string.Empty,
		defaultBindingMode: BindingMode.OneWay
	);

	public string BBCodeText
	{
		get => (string)GetValue(BBCodeTextProperty);
		set
		{
			SetValue(BBCodeTextProperty, value);
			OnTextChanged(value);
		}
	}

	private new string Text => string.Empty;

	private void OnTextChanged(in string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			FormattedText = null;
			return;
		}

		FormattedString formattedString = new();
		BBCodeParser.Process(text, (spanText, tags) =>
		{
			Span span = new()
			{
				Text = spanText,
			};
			Color lightThemeColor = DefaultLightThemeTextColor;
			Color darkThemeColor = DefaultDarkThemeTextColor;
			foreach (var tag in tags)
			{
				switch (tag.Name)
				{
					case BBCodeConstants._TAG_BOLD:
						span.FontAttributes |= FontAttributes.Bold;
						break;
					case BBCodeConstants._TAG_ITALIC:
						span.FontAttributes |= FontAttributes.Italic;
						break;
					case BBCodeConstants._TAG_UNDERLINE:
						span.TextDecorations |= Microsoft.Maui.TextDecorations.Underline;
						break;
					case BBCodeConstants._TAG_STRIKETHROUGH:
						span.TextDecorations |= Microsoft.Maui.TextDecorations.Strikethrough;
						break;
					case BBCodeConstants._TAG_COLOR:
						tag.Attributes.TryGetValue(string.Empty, out string? colorStr);
						tag.Attributes.TryGetValue(BBCodeConstants.ATTR_LIGHT, out string? lightColorStr);
						lightColorStr ??= colorStr;
						if (!string.IsNullOrEmpty(lightColorStr))
						{
							lightThemeColor = GetColor(lightColorStr, lightThemeColor);
						}
						if (tag.Attributes.TryGetValue(BBCodeConstants.ATTR_DARK, out string? darkColorStr))
						{
							darkThemeColor = GetColor(darkColorStr, darkThemeColor);
						}
						break;
					case BBCodeConstants._TAG_SIZE:
						if (tag.Attributes.TryGetValue(string.Empty, out string? size))
							span.FontSize = double.Parse(size);
						break;
					case BBCodeConstants._TAG_FONT:
						if (tag.Attributes.TryGetValue(string.Empty, out string? fontFamily))
							span.FontFamily = fontFamily;
						break;
				}
			}
			span.SetAppThemeColor(Span.TextColorProperty, lightThemeColor, darkThemeColor);
			formattedString.Spans.Add(span);
		});

		FormattedText = formattedString;
	}

	[return: NotNullIfNotNull(nameof(defaultColor))]
	static Color? GetColor(string? color, Color? defaultColor)
	{
		try
		{
			if (string.IsNullOrEmpty(color))
				return defaultColor;

			if (color.StartsWith('#'))
				return Color.FromArgb(color);

			if (color.StartsWith("0x"))
				return Color.FromArgb(color[2..]);

			if (color.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && color.EndsWith(')'))
			{
				var parts = color[4..^1].Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 3)
				{
					return Color.FromRgb(
						byte.Parse(parts[0]),
						byte.Parse(parts[1]),
						byte.Parse(parts[2])
					);
				}
			}
			else if (color.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase) && color.EndsWith(')'))
			{
				var parts = color[5..^1].Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 5)
				{
					return Color.FromRgba(
						byte.Parse(parts[0]),
						byte.Parse(parts[1]),
						byte.Parse(parts[2]),
						byte.Parse(parts[3])
					);
				}
			}

			if (_colorCache.TryGetValue(color.ToLower(), out var cachedColor))
				return cachedColor;

			return defaultColor;
		}
		catch (Exception)
		{
			return defaultColor;
		}
	}
}
