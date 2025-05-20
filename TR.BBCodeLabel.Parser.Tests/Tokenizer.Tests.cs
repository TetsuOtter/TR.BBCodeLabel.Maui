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

	public static readonly TestCase[] NestingAndAttributeTestCases =
	[
		// タグが入れ違いにネストするケース
		new("[a][b][/a][/b]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "a"),
				new Token(2, TokenType.TagEnd, ']'),
				new Token(3, TokenType.TagStart, '['),
				new Token(4, TokenType.TagName, "b"),
				new Token(5, TokenType.TagEnd, ']'),
				new Token(6, TokenType.TagStart, '['),
				new Token(7, TokenType.TagClose, '/'),
				new Token(8, TokenType.TagName, "a"),
				new Token(9, TokenType.TagEnd, ']'),
				new Token(10, TokenType.TagStart, '['),
				new Token(11, TokenType.TagClose, '/'),
				new Token(12, TokenType.TagName, "b"),
				new Token(13, TokenType.TagEnd, ']'),
			]),
		// 属性付きタグ
		new("[url=example.com]text[/url]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "url"),
				new Token(4, TokenType.TagAttribute, "=example.com"),
				new Token(16, TokenType.TagEnd, ']'),
				new Token(17, TokenType.Text, "text"),
				new Token(21, TokenType.TagStart, '['),
				new Token(22, TokenType.TagClose, '/'),
				new Token(23, TokenType.TagName, "url"),
				new Token(26, TokenType.TagEnd, ']'),
			]),
		// 属性付きタグ（属性keyが空のケース）
		new("[url=abc]text[/url]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "url"),
				new Token(4, TokenType.TagAttribute, "=abc"),
				new Token(8, TokenType.TagEnd, ']'),
				new Token(9, TokenType.Text, "text"),
				new Token(13, TokenType.TagStart, '['),
				new Token(14, TokenType.TagClose, '/'),
				new Token(15, TokenType.TagName, "url"),
				new Token(18, TokenType.TagEnd, ']'),
			]),
		// 属性付きタグ（属性keyもvalueも空のケース）
		new("[url=]text[/url]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "url"),
				new Token(4, TokenType.TagAttribute, "="),
				new Token(5, TokenType.TagEnd, ']'),
				new Token(6, TokenType.Text, "text"),
				new Token(10, TokenType.TagStart, '['),
				new Token(11, TokenType.TagClose, '/'),
				new Token(12, TokenType.TagName, "url"),
				new Token(15, TokenType.TagEnd, ']'),
			]),
		// 属性値が空
		new("[foo=]bar[/foo]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "foo"),
				new Token(4, TokenType.TagAttribute, "="),
				new Token(5, TokenType.TagEnd, ']'),
				new Token(6, TokenType.Text, "bar"),
				new Token(9, TokenType.TagStart, '['),
				new Token(10, TokenType.TagClose, '/'),
				new Token(11, TokenType.TagName, "foo"),
				new Token(14, TokenType.TagEnd, ']'),
			]),
		// 属性が複数
		new("[tag a=1 b=2]x[/tag]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "tag"),
				new Token(5, TokenType.TagAttribute, "a=1"),
				new Token(9, TokenType.TagAttribute, "b=2"),
				new Token(12, TokenType.TagEnd, ']'),
				new Token(13, TokenType.Text, "x"),
				new Token(14, TokenType.TagStart, '['),
				new Token(15, TokenType.TagClose, '/'),
				new Token(16, TokenType.TagName, "tag"),
				new Token(19, TokenType.TagEnd, ']'),
			]),
		// 属性にスペースや記号
		new("[tag a=\"1 2\" b='x=y']z[/tag]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "tag"),
				new Token(5, TokenType.TagAttribute, "a=\"1"),
				new Token(10, TokenType.TagAttribute, "2\""),
				new Token(13, TokenType.TagAttribute, "b='x=y'"),
				new Token(20, TokenType.TagEnd, ']'),
				new Token(21, TokenType.Text, "z"),
				new Token(22, TokenType.TagStart, '['),
				new Token(23, TokenType.TagClose, '/'),
				new Token(24, TokenType.TagName, "tag"),
				new Token(27, TokenType.TagEnd, ']'),
			]),
		// タグ名や属性名に数字・大文字
		new("[A1 B2=3]x[/A1]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "A1"),
				new Token(4, TokenType.TagAttribute, "B2=3"),
				new Token(8, TokenType.TagEnd, ']'),
				new Token(9, TokenType.Text, "x"),
				new Token(10, TokenType.TagStart, '['),
				new Token(11, TokenType.TagClose, '/'),
				new Token(12, TokenType.TagName, "A1"),
				new Token(14, TokenType.TagEnd, ']'),
			]),
		// 不正なタグ
		new("[b Hello[/b]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "b"),
				new Token(3, TokenType.TagAttribute, "Hello[/b"),
				new Token(11, TokenType.TagEnd, ']'),
			]),
		// 属性の=が複数
		new("[tag a==b==c]x[/tag]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "tag"),
				new Token(5, TokenType.TagAttribute, "a==b==c"),
				new Token(12, TokenType.TagEnd, ']'),
				new Token(13, TokenType.Text, "x"),
				new Token(14, TokenType.TagStart, '['),
				new Token(15, TokenType.TagClose, '/'),
				new Token(16, TokenType.TagName, "tag"),
				new Token(19, TokenType.TagEnd, ']'),
			]),
		// 閉じタグのみ
		new("[/b]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagClose, '/'),
				new Token(2, TokenType.TagName, "b"),
				new Token(3, TokenType.TagEnd, ']'),
			]),
		// 開始タグのみ
		new("[b]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "b"),
				new Token(2, TokenType.TagEnd, ']'),
			]),
		// タグの直後にテキストがない
		new("[b][/b]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "b"),
				new Token(2, TokenType.TagEnd, ']'),
				new Token(3, TokenType.TagStart, '['),
				new Token(4, TokenType.TagClose, '/'),
				new Token(5, TokenType.TagName, "b"),
				new Token(6, TokenType.TagEnd, ']'),
			]),
		// タグの直後に別タグ
		new("[b][i][/i][/b]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "b"),
				new Token(2, TokenType.TagEnd, ']'),
				new Token(3, TokenType.TagStart, '['),
				new Token(4, TokenType.TagName, "i"),
				new Token(5, TokenType.TagEnd, ']'),
				new Token(6, TokenType.TagStart, '['),
				new Token(7, TokenType.TagClose, '/'),
				new Token(8, TokenType.TagName, "i"),
				new Token(9, TokenType.TagEnd, ']'),
				new Token(10, TokenType.TagStart, '['),
				new Token(11, TokenType.TagClose, '/'),
				new Token(12, TokenType.TagName, "b"),
				new Token(13, TokenType.TagEnd, ']'),
			]),
		// 属性値に記号
		new("[tag a=1!@#]x[/tag]",
			[
				new Token(0, TokenType.TagStart, '['),
				new Token(1, TokenType.TagName, "tag"),
				new Token(5, TokenType.TagAttribute, "a=1!@#"),
				new Token(11, TokenType.TagEnd, ']'),
				new Token(12, TokenType.Text, "x"),
				new Token(13, TokenType.TagStart, '['),
				new Token(14, TokenType.TagClose, '/'),
				new Token(15, TokenType.TagName, "tag"),
				new Token(18, TokenType.TagEnd, ']'),
			]),
	];

	[TestCaseSource(nameof(EmptyReturnsEmptyTestCases))]
	public void EmptyReturnsEmptyTest(TestCase testCase) => TestImpl(testCase);
	[TestCaseSource(nameof(OneOrLessTagTestCases))]
	public void OneOrLessTagTest(TestCase testCase) => TestImpl(testCase);
	[TestCaseSource(nameof(SpaceInTagTestCases))]
	public void SpaceInTagTest(TestCase testCase) => TestImpl(testCase);
	[TestCaseSource(nameof(NestingAndAttributeTestCases))]
	public void NestingAndAttributeTest(TestCase testCase) => TestImpl(testCase);

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
