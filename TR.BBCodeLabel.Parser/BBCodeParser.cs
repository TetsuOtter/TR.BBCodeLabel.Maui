using System;
using System.Collections.Generic;

namespace TR.BBCodeLabel.Parser;

public class BBCodeParser
{
	public static List<BBCodeSpan> Parse(string input)
	{
		List<BBCodeSpan> spans = [];

		Process(input, (text, tags) =>
		{
			spans.Add(new(text, [.. tags]));
		});

		return spans;
	}

	public static void Process(string input, Action<string, IReadOnlyList<BBCodeTag>> action)
	{
		List<BBCodeTokenizer.Token> tokens = BBCodeTokenizer.Tokenize(input);

		List<BBCodeTag> tagList = [];

		for (int i = 0; i < tokens.Count; i++)
		{
			var token = tokens[i];
			switch (token.Type)
			{
				case BBCodeTokenizer.TokenType.Text:
					action(token.Content, tagList);
					break;
				case BBCodeTokenizer.TokenType.TagStart:
					ParseInTag(in tokens, ref i, tagList);
					break;
				case BBCodeTokenizer.TokenType.TagClose:
				case BBCodeTokenizer.TokenType.TagName:
				case BBCodeTokenizer.TokenType.TagAttribute:
				case BBCodeTokenizer.TokenType.TagEnd:
					throw new BBCodeParserException($"Unexpected token: {token} at index {token.Index}");
			}
		}
	}

	/// <summary>
	/// Parses the content inside a tag.
	/// This method is called when a tag start token is encountered.
	/// It processes the tag name and its attributes, and updates the tag stack accordingly.
	/// It also handles closing tags.
	/// If a closing tag is encountered, it pops the corresponding tag from the stack.
	/// If an opening tag is encountered, it pushes the new tag onto the stack.
	/// If the tag is not recognized, it throws a BBCodeParserException.
	/// </summary>
	/// <param name="tokens">tokenization result</param>
	/// <param name="i">index of `tokens`</param>
	/// <param name="tagList">Tag stack</param>
	/// <exception cref="BBCodeParserException">thrown when an unexpected token is encountered</exception>
	private static void ParseInTag(in List<BBCodeTokenizer.Token> tokens, ref int i, List<BBCodeTag> tagList)
	{
		if (++i == tokens.Count)
		{
			throw new BBCodeParserException($"Unexpected end of input after tag start at index {tokens[i - 1].Index}");
		}

		bool isClosingTag = tokens[i].Type == BBCodeTokenizer.TokenType.TagClose;
		if (isClosingTag && ++i == tokens.Count)
		{
			throw new BBCodeParserException($"Unexpected end of input after closing tag at index {tokens[i - 1].Index}");
		}

		if (tokens[i].Type != BBCodeTokenizer.TokenType.TagName)
		{
			throw new BBCodeParserException($"Expected tag name, but got: {tokens[i]} at index {tokens[i].Index}");
		}

		string tagName = tokens[i++].Content;
		List<string> attributes = [];
		for (; i < tokens.Count; i++)
		{
			switch (tokens[i].Type)
			{
				case BBCodeTokenizer.TokenType.TagAttribute:
					attributes.Add(tokens[i].Content);
					break;
				case BBCodeTokenizer.TokenType.TagEnd:
					if (isClosingTag)
					{
						int index = tagList.FindLastIndex(t => t.Name == tagName);
						if (index == -1)
						{
							throw new BBCodeParserException($"Closing tag '{tagName}' not found in stack at index {tokens[i].Index}");
						}
						tagList.RemoveAt(index);
						return;
					}
					Dictionary<string, string> attributesDict = [];
					foreach (var attribute in attributes)
					{
						int index = attribute.IndexOf(BBCodeTokenizer.TAG_ATTR_ASSIGN);
						if (index == -1)
						{
							attributesDict.Add(attribute, string.Empty);
						}
						else
						{
							attributesDict.Add(attribute.Substring(0, index), attribute.Substring(index + 1));
						}
					}
					tagList.Add(new(tagName, attributesDict));
					return;
				default:
					throw new BBCodeParserException($"Unexpected token: {tokens[i]} at index {tokens[i].Index}");
			}
		}
		throw new BBCodeParserException($"Unexpected end of input after tag name '{tagName}' at index {tokens[i - 1].Index}");
	}
}
