using System;

namespace UnityEngine.Purchasing
{
	public class ReceiptParserException : Exception
	{
		public ReceiptParserException()
		{
		}

		public ReceiptParserException(string message)
			: base(message)
		{
		}
	}
}
