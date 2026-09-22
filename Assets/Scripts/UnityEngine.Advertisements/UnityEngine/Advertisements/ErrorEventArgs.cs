using System;

namespace UnityEngine.Advertisements
{
	internal class ErrorEventArgs : EventArgs
	{
		public long error { get; private set; }

		public string message { get; private set; }

		public ErrorEventArgs(long error, string message)
		{
			this.error = error;
			this.message = message;
		}
	}
}
