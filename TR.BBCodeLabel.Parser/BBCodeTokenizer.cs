using System;
using System.Collections.Generic;

namespace TR.BBCodeLabel.Parser;

public static class BBCodeTokenizer
{
	public enum TokenType
	{
		/// <summary>
		/// Text content outside of tags
		/// </summary>
		Text,

		/// <summary>
		/// Opening tag start (`[`)
		/// </summary>
		TagStart,

		/// <summary>
		/// Closing tag char (`/`)
		/// </summary>
		TagClose,

		/// <summary>
		/// Tag name (e.g., `b`, `i`, etc.)
		/// </summary>
		TagName,

		/// <summary>
		/// Tag attribute (e.g., `color=red`)
		/// </summary>
		TagAttribute,

		/// <summary>
		/// Tag Tag end (`]`)
		/// </summary>
		TagEnd,
	}
	public readonly struct Token(int index, TokenType type, string content) : IEquatable<Token>
	{
		public readonly int Index = index;
		public readonly TokenType Type = type;
		public readonly string Content = content;

		public Token(int index, TokenType type, char c) : this(index, type, c.ToString()) { }

		public override string ToString()
			=> $"{Type}: '{Content}' at {Index}";

		public override bool Equals(object? obj)
			=> obj is Token other && Equals(other);
		public bool Equals(Token other)
			=> Index == other.Index && Type == other.Type && Content == other.Content;

		public override int GetHashCode()
			=> HashCode.Combine(Index, Type, Content);

		public static bool operator ==(Token left, Token right)
			=> left.Equals(right);

		public static bool operator !=(Token left, Token right)
			=> !(left == right);
	}

	public const char TAG_ESCAPE = '\\';

	public const char TAG_START = '[';
	public const char TAG_END = ']';
	public const char TAG_CLOSE = '/';
	public const char TAG_ATTR_ASSIGN = '=';

	public static List<Token> Tokenize(in string input)
	{
		List<Token> tokens = [];

		int iSpanFrom = 0;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (c == TAG_START)
			{
				if (iSpanFrom != i)
				{
					tokens.Add(new(iSpanFrom, TokenType.Text, input[iSpanFrom..i]));
				}
				TokenizeTag(input, ref i, tokens);
				iSpanFrom = i + 1;
			}
			else if (c == TAG_ESCAPE)
			{
				if (iSpanFrom != i)
				{
					tokens.Add(new(iSpanFrom, TokenType.Text, input[iSpanFrom..i]));
				}
				i++;
				if (i < input.Length)
				{
					tokens.Add(new(i, TokenType.Text, input[i]));
					iSpanFrom = i + 1;
				}
			}
		}

		if (iSpanFrom != input.Length)
		{
			tokens.Add(new(iSpanFrom, TokenType.Text, input[iSpanFrom..]));
		}

		return tokens;
	}

	private static void TokenizeTag(in string content, ref int i, List<Token> tokens)
	{
		// FIXME: Support escaped characters in tag names and attributes
		tokens.Add(new(i++, TokenType.TagStart, TAG_START));

		if (content[i] == TAG_CLOSE)
		{
			tokens.Add(new(i, TokenType.TagClose, TAG_CLOSE));
			i++;
		}

		int iSpanFrom = i;
		bool isNameNotStarted = true;
		for (; i < content.Length; i++)
		{
			char c = content[i];
			if (isNameNotStarted && char.IsWhiteSpace(c))
			{
				iSpanFrom = i + 1;
				continue;
			}
			isNameNotStarted = false;
			if (char.IsWhiteSpace(c) || c == TAG_ATTR_ASSIGN || c == TAG_END)
			{
				tokens.Add(new(iSpanFrom, TokenType.TagName, content[iSpanFrom..i]));
				break;
			}
		}

		iSpanFrom = i;
		for (; i < content.Length; i++)
		{
			char c = content[i];
			if (c == TAG_END)
			{
				if (iSpanFrom != i)
				{
					tokens.Add(new(iSpanFrom, TokenType.TagAttribute, content[iSpanFrom..i]));
				}
				tokens.Add(new(i, TokenType.TagEnd, TAG_END));
				return;
			}
			else if (char.IsWhiteSpace(c))
			{
				if (iSpanFrom != i)
				{
					tokens.Add(new(iSpanFrom, TokenType.TagAttribute, content[iSpanFrom..i]));
				}
				iSpanFrom = i + 1;
			}
		}
	}
}
