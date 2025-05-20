using System;
using System.Collections.Generic;

namespace TR.BBCodeLabel.Parser;

public class BBCodeSpan(string content, BBCodeTag[] tags) : IEquatable<BBCodeSpan>
{
	public string Content { get; } = content;
	public BBCodeTag[] Tags { get; } = tags;

	public override bool Equals(object? obj)
		=> ReferenceEquals(this, obj) || (obj is BBCodeSpan other && Equals(other));
	public bool Equals(BBCodeSpan? other)
	{
		if (other is null)
			return false;

		if (Content != other.Content)
			return false;
		if (Tags.Length != other.Tags.Length)
			return false;
		for (int i = 0; i < Tags.Length; i++)
		{
			if (!Tags[i].Equals(other.Tags[i]))
				return false;
		}
		return true;
	}
	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + Content.GetHashCode();
		foreach (var tag in Tags)
		{
			hash = hash * 31 + tag.GetHashCode();
		}
		return hash;
	}
	public override string ToString()
		=> $"Content: {Content}, Tags: [{string.Join(", ", (IEnumerable<BBCodeTag>)Tags)}]";

	public static bool operator ==(BBCodeSpan left, BBCodeSpan right)
		=> left?.Equals(right) ?? right is null;
	public static bool operator !=(BBCodeSpan left, BBCodeSpan right)
		=> !(left == right);
}
