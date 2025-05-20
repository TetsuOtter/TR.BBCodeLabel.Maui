using System;

namespace TR.BBCodeLabel.Parser;

public class BBCodeParserException : Exception
{
	public BBCodeParserException(string message) : base(message)
	{
	}

	public BBCodeParserException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
