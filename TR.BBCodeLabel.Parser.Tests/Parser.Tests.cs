using System.Collections.Generic;

using NUnit.Framework;

namespace TR.BBCodeLabel.Parser.Tests;

public class ParserTests
{
	public static readonly TestCase[] EmptyReturnsEmptyTestCases =
	[
		new("", []),
	];

	public static readonly TestCase[] SimpleTextTestCases =
	[
		new("Hello", [new BBCodeSpan("Hello", [])]),
		new("Hello World", [new BBCodeSpan("Hello World", [])]),
		new("Hello\nWorld", [new BBCodeSpan("Hello\nWorld", [])]),
		new("  Spaces  ", [new BBCodeSpan("  Spaces  ", [])]),
		new("123456", [new BBCodeSpan("123456", [])]),
		new("!@#$%^&*()", [new BBCodeSpan("!@#$%^&*()", [])]),
		new("日本語テスト", [new BBCodeSpan("日本語テスト", [])])
	];

	public static readonly TestCase[] EscapedCharacterTestCases =
	[
		new("Hello\\[World", [
			new BBCodeSpan("Hello", []),
			new BBCodeSpan("[", []),
			new BBCodeSpan("World", []),
		]),
		new("\\[b\\]Bold\\[/b\\]", [
			new BBCodeSpan("[", []),
			new BBCodeSpan("b", []),
			new BBCodeSpan("]", []),
			new BBCodeSpan("Bold", []),
			new BBCodeSpan("[", []),
			new BBCodeSpan("/b", []),
			new BBCodeSpan("]", []),
		]),
		new("Text with \\[ and \\] brackets", [
			new BBCodeSpan("Text with ", []),
			new BBCodeSpan("[", []),
			new BBCodeSpan(" and ", []),
			new BBCodeSpan("]", []),
			new BBCodeSpan(" brackets", []),
		]),
		new("\\\\Backslash", [new BBCodeSpan("\\", []), new BBCodeSpan("Backslash", [])])
	];

	public static readonly TestCase[] SimpleTagsTestCases =
	[
		// Single tag
		new(
			"[b]Bold[/b]",
			[new BBCodeSpan("Bold", [new BBCodeTag("b", [])])]
		),

		// Single tag with attributes
		new(
			"[url=example.com]Link[/url]",
			[new BBCodeSpan("Link", [new BBCodeTag("url", new Dictionary<string, string> { [string.Empty] = "example.com" })])]
		),

		// Multiple consecutive tags
		new("[b]Bold[/b][i]Italic[/i]",
			[
				new BBCodeSpan("Bold", [new BBCodeTag("b", [])]),
				new BBCodeSpan("Italic", [new BBCodeTag("i", [])])
			]
		),

		// Text before tags
		new("Plain [b]Bold[/b]",
			[
				new BBCodeSpan("Plain ", []),
				new BBCodeSpan("Bold", [new BBCodeTag("b", [])])
			]
		),

		// Text after tags
		new("[b]Bold[/b] text",
			[
				new BBCodeSpan("Bold", [new BBCodeTag("b", [])]),
				new BBCodeSpan(" text", [])
			]
		),

		// Text between tags
		new("[b]Bold[/b] text [i]Italic[/i]",
			[
				new BBCodeSpan("Bold", [new BBCodeTag("b", [])]),
				new BBCodeSpan(" text ", []),
				new BBCodeSpan("Italic", [new BBCodeTag("i", [])])
			]
		),
	];

	public static readonly TestCase[] NestedTagsTestCases =
	[
		// Simple nesting
		new("[b][i]Bold Italic[/i][/b]",
			[
				new BBCodeSpan("Bold Italic", [
					new BBCodeTag("b", []),
					new BBCodeTag("i", [])
				])
			]
		),

		// Deeper nesting
		new("[b][i][u]Bold Italic Underlined[/u][/i][/b]",
			[
				new BBCodeSpan("Bold Italic Underlined", [
					new BBCodeTag("b", []),
					new BBCodeTag("i", []),
					new BBCodeTag("u", [])
				])
			]
		),

		// Nested with text between
		new("[b]Bold [i]Bold Italic[/i] Bold[/b]",
			[
				new BBCodeSpan("Bold ", [new BBCodeTag("b", [])]),
				new BBCodeSpan("Bold Italic", [
					new BBCodeTag("b", []),
					new BBCodeTag("i", [])
				]),
				new BBCodeSpan(" Bold", [new BBCodeTag("b", [])])
			]
		),
	];

	public static readonly TestCase[] ComplexTagsWithAttributesTestCases =
	[
		// Tag with single attribute
		new("[color=red]Red Text[/color]",
			[
				new BBCodeSpan("Red Text", [
					new BBCodeTag("color", new Dictionary<string, string> { [string.Empty] = "red" })
				])
			]
		),

		// Tag with multiple attributes
		new("[font size=12 color=blue]Blue Text[/font]",
			[
				new BBCodeSpan("Blue Text", [
					new BBCodeTag("font", new Dictionary<string, string> {
						["size"] = "12",
						["color"] = "blue"
					})
				])
			]
		),

		// Tag with empty attribute value
		new("[url=]Empty Link[/url]",
			[
				new BBCodeSpan("Empty Link", [
					new BBCodeTag("url", new Dictionary<string, string> { [string.Empty] = string.Empty })
				])
			]
		),

		// Nested tags with attributes
		new("[b][color=red]Bold Red[/color][/b]",
			[
				new BBCodeSpan("Bold Red", [
					new BBCodeTag("b", []),
					new BBCodeTag("color", new Dictionary<string, string> { [string.Empty] = "red" })
				])
			]
		),
	];

	public static readonly TestCase[] TagsWithWhitespaceTestCases =
	[
		// Spaces in tag names
		new("[ b ]Bold[/ b ]",
			[
				new BBCodeSpan("Bold", [new BBCodeTag("b", [])])
			]
		),

		// Spaces in attributes
		new("[color = red ]Red Text[/color]",
			[
				new BBCodeSpan("Red Text", [
					new BBCodeTag("color", new Dictionary<string, string> {
						[string.Empty] = string.Empty,
						["red"] = string.Empty,
					})
				])
			]
		),

		// Mixed whitespace
		new("[ font  size = 12  color = blue ]Blue Text[/ font ]",
			[
				new BBCodeSpan("Blue Text", [
					new BBCodeTag("font", new Dictionary<string, string> {
						[string.Empty] = string.Empty,
						["size"] = string.Empty,
						["12"] = string.Empty,
						["color"] = string.Empty,
						["blue"] = string.Empty,
					})
				])
			]
		),
	];

	public static readonly TestCase[] IncompleteTagsTestCases =
	[
		// Missing closing tag (should still be parsed as a tag)
		new("[b]Bold",
			[
				new BBCodeSpan("Bold", [new BBCodeTag("b", [])])
			]
		),

		// Empty tag
		new("[b][/b]", []),
	];

	public static readonly TestCase[] CrossNestedTagsTestCases =
	[
		// Cross-nested tags (not properly nested, but parser should handle it)
		new("[b][i]Bold Italic[/b][/i]",
			[
				new BBCodeSpan("Bold Italic", [
					new BBCodeTag("b", []),
					new BBCodeTag("i", [])
				])
			]
		),

		// Multiple cross-nested tags
		new("[a][b][c]Text[/a][/b][/c]",
			[
				new BBCodeSpan("Text", [
					new BBCodeTag("a", []),
					new BBCodeTag("b", []),
					new BBCodeTag("c", [])
				])
			]
		),

		// Multiple cross-nested tags
		new("N[a]A[b]AB[c]ABC[/a]BC[/b]C[/c]N",
			[
				new BBCodeSpan("N", []),
				new BBCodeSpan("A", [new BBCodeTag("a", [])]),
				new BBCodeSpan("AB", [
					new BBCodeTag("a", []),
					new BBCodeTag("b", []),
				]),
				new BBCodeSpan("ABC", [
					new BBCodeTag("a", []),
					new BBCodeTag("b", []),
					new BBCodeTag("c", []),
				]),
				new BBCodeSpan("BC", [
					new BBCodeTag("b", []),
					new BBCodeTag("c", []),
				]),
				new BBCodeSpan("C", [new BBCodeTag("c", [])]),
				new BBCodeSpan("N", [])
			]
		),
	];

	public static readonly TestCase[] SpecialCasesTestCases =
	[
		// Empty content tags
		new("[b][/b][i]Italic[/i]",
			[
				new BBCodeSpan("Italic", [new BBCodeTag("i", [])])
			]
		),

		// Tags with numbers and special characters
		new("[h1]Heading[/h1]",
			[
				new BBCodeSpan("Heading", [new BBCodeTag("h1", [])])
			]
		),

		// Case insensitive tag names
		new("[B]Bold[/b]",
			[
				new BBCodeSpan("Bold", [new BBCodeTag("b", [])])
			]
		),
	];

	public static readonly TestCase[] InvalidSyntaxTestCases =
	[
		// Invalid tag name
		new("[123]Invalid[/123]",
			[
				new BBCodeSpan("Invalid", [new BBCodeTag("123", [])])
			]
		),

		// Empty tag name
		new("[=value]Empty Tag[/]",
			[
				new BBCodeSpan("Empty Tag", [new BBCodeTag("", new Dictionary<string, string> { [string.Empty] = "value" })])
			]
		),

		// Multiple equal signs in attribute
		new("[tag attr==value]Text[/tag]",
			[
				new BBCodeSpan("Text", [new BBCodeTag("tag", new Dictionary<string, string> { ["attr"] = "=value" })])
			]
		),

		// Malformed URL tag
		new("[url=http://example.com?a=1&b=2]Link[/url]",
			[
				new BBCodeSpan("Link", [
					new BBCodeTag("url", new Dictionary<string, string> { [string.Empty] = "http://example.com?a=1&b=2" })
				])
			]
		),
	];

	public static readonly TestCase[] MultipleSpanTagsTestCases =
	[
		// Multiple different tags
		new("[b]Bold[/b][i]Italic[/i][u]Underline[/u]",
			[
				new BBCodeSpan("Bold", [new BBCodeTag("b", [])]),
				new BBCodeSpan("Italic", [new BBCodeTag("i", [])]),
				new BBCodeSpan("Underline", [new BBCodeTag("u", [])])
			]
		),

		// Multiple spans with same tag
		new("[b]Bold1[/b] text [b]Bold2[/b]",
			[
				new BBCodeSpan("Bold1", [new BBCodeTag("b", [])]),
				new BBCodeSpan(" text ", []),
				new BBCodeSpan("Bold2", [new BBCodeTag("b", [])])
			]
		),

		// Complex mix of spans and tags
		new("Text [b]Bold [i]BoldItalic[/i][/b] [u]Underline[/u]",
			[
				new BBCodeSpan("Text ", []),
				new BBCodeSpan("Bold ", [new BBCodeTag("b", [])]),
				new BBCodeSpan("BoldItalic", [
						new BBCodeTag("b", []),
						new BBCodeTag("i", [])
				]),
				new BBCodeSpan(" ", []),
				new BBCodeSpan("Underline", [new BBCodeTag("u", [])])
			]
		),
	];

	public static readonly TestCase[] NestedSameTagsTestCases =
	[
		// Nested same tags (should be handled correctly)
		new("[b][b]Double Bold[/b][/b]",
			[
				new BBCodeSpan("Double Bold", [
					new BBCodeTag("b", []),
					new BBCodeTag("b", [])
				])
			]
		),

		// Deeply nested same tags
		new("[b][b][b]Triple Bold[/b][/b][/b]",
			[
				new BBCodeSpan("Triple Bold", [
					new BBCodeTag("b", []),
					new BBCodeTag("b", []),
					new BBCodeTag("b", [])
				])
			]
		),
	];

	public static readonly TestCase[] AttributeHandlingTestCases =
	[
		// Attribute with spaces
		new("[quote author=\"John Doe\"]Quote[/quote]",
			[
				new BBCodeSpan("Quote", [
					new BBCodeTag("quote", new Dictionary<string, string> { ["author"] = "\"John", ["Doe\""] = string.Empty })
				])
			]
		),

		// Attribute with equals sign
		new("[url=http://example.com?param=value]Link[/url]",
			[
				new BBCodeSpan("Link", [
					new BBCodeTag("url", new Dictionary<string, string> { [string.Empty] = "http://example.com?param=value" })
				])
			]
		),

		// Multiple attributes with complex values
		new("[tag a=\"Complex value\" b='Another value']Text[/tag]",
			[
				new BBCodeSpan("Text", [
					new BBCodeTag("tag", new Dictionary<string, string> {
						["a"] = "\"Complex",
						["value\""] = string.Empty,
						["b"] = "'Another",
						["value'"] = string.Empty
					})
				])
			]
		),
	];

	public static readonly TestCase[] QuotedAttributeValuesTestCases =
	[
		// Single quoted values
		new("[quote author='John Doe']Quote[/quote]",
			[
				new BBCodeSpan("Quote", [
					new BBCodeTag("quote", new Dictionary<string, string> { ["author"] = "'John", ["Doe'"] = string.Empty })
				])
			]
		),

		// Double quoted values with special characters
		new("[tag attr=\"value with = sign\"]Text[/tag]",
			[
				new BBCodeSpan("Text", [
					new BBCodeTag("tag", new Dictionary<string, string> { ["attr"] = "\"value", ["with"] = string.Empty, [string.Empty] = string.Empty, ["sign\""] = string.Empty })
				])
			]
		),
	];

	[TestCaseSource(nameof(EmptyReturnsEmptyTestCases))]
	public void EmptyReturnsEmptyTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(SimpleTextTestCases))]
	public void SimpleTextTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(EscapedCharacterTestCases))]
	public void EscapedCharacterTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(SimpleTagsTestCases))]
	public void SimpleTagsTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(NestedTagsTestCases))]
	public void NestedTagsTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(ComplexTagsWithAttributesTestCases))]
	public void ComplexTagsWithAttributesTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(TagsWithWhitespaceTestCases))]
	public void TagsWithWhitespaceTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(IncompleteTagsTestCases))]
	public void IncompleteTagsTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(CrossNestedTagsTestCases))]
	public void CrossNestedTagsTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(SpecialCasesTestCases))]
	public void SpecialCasesTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(InvalidSyntaxTestCases))]
	public void InvalidSyntaxTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(MultipleSpanTagsTestCases))]
	public void MultipleSpanTagsTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(NestedSameTagsTestCases))]
	public void NestedSameTagsTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(AttributeHandlingTestCases))]
	public void AttributeHandlingTest(TestCase testCase) => TestImpl(testCase);

	[TestCaseSource(nameof(QuotedAttributeValuesTestCases))]
	public void QuotedAttributeValuesTest(TestCase testCase) => TestImpl(testCase);

	[Test]
	public void ThrowsOnUnexpectedToken() => Assert.Throws<BBCodeParserException>(() => BBCodeParser.Parse("]Invalid["));
	[Test]
	public void ThrowsOnUnclosedTagBracket() => Assert.Throws<BBCodeParserException>(() => BBCodeParser.Parse("Text [b"));

	[Test]
	public void ThrowsOnUnclosedTagWithAttributes() => Assert.Throws<BBCodeParserException>(() => BBCodeParser.Parse("[url=http://example.com"));

	[Test]
	public void ThrowsOnMismatchedClosingTag() => Assert.Throws<BBCodeParserException>(() => BBCodeParser.Parse("[b]Text[/i]"));

	[Test]
	public void ThrowsOnNotOpenedClosingTag() => Assert.Throws<BBCodeParserException>(() => BBCodeParser.Parse("Text[/b]"));

	public record struct TestCase(string Input, BBCodeSpan[] Expected);

	private static void TestImpl(TestCase testCase)
	{
		var spans = BBCodeParser.Parse(testCase.Input);
		Assert.That(spans, Has.Count.EqualTo(testCase.Expected.Length));

		for (int i = 0; i < testCase.Expected.Length; i++)
		{
			BBCodeSpan expectedSpan = testCase.Expected[i];
			BBCodeSpan actualSpan = spans[i];
			Assert.That(actualSpan.Content, Is.EqualTo(expectedSpan.Content), $"Content mismatch at span {i}");

			AssertEqualsTags($"span {i}", expectedSpan.Tags, actualSpan.Tags);
		}
	}
	private static void AssertEqualsTags(string at, BBCodeTag[] expected, BBCodeTag[] actual)
	{
		Assert.That(actual, Has.Length.EqualTo(expected.Length));
		for (int i = 0; i < expected.Length; i++)
		{
			BBCodeTag expectedTag = expected[i];
			BBCodeTag actualTag = actual[i];
			Assert.That(actualTag.Name, Is.EqualTo(expectedTag.Name), $"Tag name mismatch at {at} tag {i}");
			AssertEqualsAttributes($"{at} tag {i}", expectedTag.Attributes, actualTag.Attributes);
		}
	}
	private static void AssertEqualsAttributes(string at, Dictionary<string, string> expected, Dictionary<string, string> actual)
	{
		Assert.That(actual, Has.Count.EqualTo(expected.Count), $"Attribute count mismatch at {at}");
		foreach (var expectedAttribute in expected)
		{
			using (Assert.EnterMultipleScope())
			{
				string key = expectedAttribute.Key;
				Assert.That(actual.ContainsKey(key), Is.True, $"Missing attribute key '{key}' at {at}");
				Assert.That(actual[key], Is.EqualTo(expectedAttribute.Value), $"Attribute value mismatch for key '{key}' at {at}");
			}
		}
	}
}
