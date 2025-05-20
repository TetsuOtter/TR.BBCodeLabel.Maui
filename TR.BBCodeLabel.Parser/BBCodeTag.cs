using System;
using System.Collections.Generic;
using System.Text;

namespace TR.BBCodeLabel.Parser;

public class BBCodeTag(string name, Dictionary<string, string> attributes) : IEquatable<BBCodeTag>
{
	public string Name { get; } = name.ToLower();

	public Dictionary<string, string> Attributes { get; } = attributes;

	public override bool Equals(object? obj)
		=> ReferenceEquals(this, obj) || (obj is BBCodeTag other && Equals(other));
	public bool Equals(BBCodeTag? other)
	{
		if (other is null)
			return false;

		if (Name != other.Name)
			return false;
		if (Attributes.Count != other.Attributes.Count)
			return false;
		foreach (var attribute in Attributes)
		{
			if (!other.Attributes.TryGetValue(attribute.Key, out var value) || value != attribute.Value)
				return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + Name.GetHashCode();
		foreach (var attribute in Attributes)
		{
			hash = hash * 31 + attribute.Key.GetHashCode();
			hash = hash * 31 + attribute.Value.GetHashCode();
		}
		return hash;
	}

	public override string ToString()
	{
		StringBuilder sb = new();
		sb.Append('[');
		sb.Append(Name);
		foreach (var attribute in Attributes)
		{
			sb.Append(' ');
			sb.Append(attribute.Key);
			sb.Append('=');
			sb.Append(attribute.Value);
		}
		sb.Append(']');
		return sb.ToString();
	}

	public static bool operator ==(BBCodeTag left, BBCodeTag right)
		=> left?.Equals(right) ?? right is null;
	public static bool operator !=(BBCodeTag left, BBCodeTag right)
		=> !(left == right);
}
