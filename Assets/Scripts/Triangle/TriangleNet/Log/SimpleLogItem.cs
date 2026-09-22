using System;

namespace TriangleNet.Log
{
	public class SimpleLogItem : ILogItem
	{
		private DateTime time;

		private LogLevel level;

		private string message;

		private string info;

		public DateTime Time
		{
			get
			{
				return time;
			}
		}

		public LogLevel Level
		{
			get
			{
				return level;
			}
		}

		public string Message
		{
			get
			{
				return message;
			}
		}

		public string Info
		{
			get
			{
				return info;
			}
		}

		public SimpleLogItem(LogLevel level, string message)
			: this(level, message, "")
		{
		}

		public SimpleLogItem(LogLevel level, string message, string info)
		{
			time = DateTime.Now;
			this.level = level;
			this.message = message;
			this.info = info;
		}
	}
}
