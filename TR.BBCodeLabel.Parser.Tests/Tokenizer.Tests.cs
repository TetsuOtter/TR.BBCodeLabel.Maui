using NUnit.Framework;

using static TR.BBCodeLabel.Parser.BBCodeTokenizer;

namespace TR.BBCodeLabel.Parser.Tests;

public class TokenizerTests
{
	public static readonly TestCase[] EmptyReturnsEmptyTestCases =
	[
		new("", []),
	];

	public static readonly TestCase[] OneOrLessTagTestCases =
	[
		new("Hello", [new Token(0, TokenType.Text, "Hello")]),
		new("[b]Hello[/b]",
		[
			new Token(0, TokenType.TagStart, '['),
			new Token(1, TokenType.TagName, "b"),
			new Token(2, TokenType.TagEnd, ']'),
			new Token(3, TokenType.Text, "Hello"),
			new Token(8, TokenType.TagStart, '['),
			new Token(9, TokenType.TagClose, '/'),
			new Token(10, TokenType.TagName, "b"),
			new Token(11, TokenType.TagEnd, ']'),
		]),
	];

	public static readonly TestCase[] SpaceInTagTestCases =
	[
		new("[ b ] [b ] [ b] [/b ] [/ b ]",
		[
			new Token(0, TokenType.TagStart, '['),
			new Token(2, TokenType.TagName, "b"),
			new Token(4, TokenType.TagEnd, ']'),
			new Token(5, TokenType.Text, " "),
			new Token(6, TokenType.TagStart, '['),
			new Token(7, TokenType.TagName, "b"),
			new Token(9, TokenType.TagEnd, ']'),
			new Token(10, TokenType.Text, " "),
			new Token(11, TokenType.TagStart, '['),
			new Token(13, TokenType.TagName, "b"),
			new Token(14, TokenType.TagEnd, ']'),
			new Token(15, TokenType.Text, " "),
			new Token(16, TokenType.TagStart, '['),
			new Token(17, TokenType.TagClose, '/'),
			new Token(18, TokenType.TagName, "b"),
			new Token(20, TokenType.TagEnd, ']'),
			new Token(21, TokenType.Text, " "),
			new Token(22, TokenType.TagStart, '['),
			new Token(23, TokenType.TagClose, '/'),
			new Token(25, TokenType.TagName, "b"),
			new Token(27, TokenType.TagEnd, ']'),
		]),
		new("[ b ]Hello[/ b ]",
		[
			new Token(0, TokenType.TagStart, '['),
			new Token(2, TokenType.TagName, "b"),
			new Token(4, TokenType.TagEnd, ']'),
			new Token(5, TokenType.Text, "Hello"),
			new Token(10, TokenType.TagStart, '['),
			new Token(11, TokenType.TagClose, '/'),
			new Token(13, TokenType.TagName, "b"),
			new Token(15, TokenType.TagEnd, ']'),
		]),
	];

	[TestCaseSource(nameof(EmptyReturnsEmptyTestCases))]
	public void EmptyReturnsEmptyTest(TestCase testCase) => TestImpl(testCase);
	[TestCaseSource(nameof(OneOrLessTagTestCases))]
	public void OneOrLessTagTest(TestCase testCase) => TestImpl(testCase);
	[TestCaseSource(nameof(SpaceInTagTestCases))]
	public void SpaceInTagTest(TestCase testCase) => TestImpl(testCase);

	public record struct TestCase(string Input, Token[] Expected);
	static void TestImpl(TestCase testCase)
	{
		var tokens = Tokenize(testCase.Input);
		Assert.That(tokens, Has.Count.EqualTo(testCase.Expected.Length));
		for (int i = 0; i < testCase.Expected.Length; i++)
		{
			Assert.That(tokens[i], Is.EqualTo(testCase.Expected[i]));
		}
	}
}
