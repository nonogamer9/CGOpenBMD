using System.Collections.Generic;

namespace TriangleNet.Log
{
	public sealed class SimpleLog : ILog<SimpleLogItem>
	{
		private List<SimpleLogItem> log = new List<SimpleLogItem>();

		private LogLevel level;

		private static readonly SimpleLog instance;

		public static ILog<SimpleLogItem> Instance
		{
			get
			{
				return instance;
			}
		}

		public IList<SimpleLogItem> Data
		{
			get
			{
				return log;
			}
		}

		public LogLevel Level
		{
			get
			{
				return level;
			}
		}

		static SimpleLog()
		{
			instance = new SimpleLog();
		}

		private SimpleLog()
		{
		}

		public void Add(SimpleLogItem item)
		{
			log.Add(item);
		}

		public void Clear()
		{
			log.Clear();
		}

		public void Info(string message)
		{
			log.Add(new SimpleLogItem(LogLevel.Info, message));
		}

		public void Warning(string message, string location)
		{
			log.Add(new SimpleLogItem(LogLevel.Warning, message, location));
		}

		public void Error(string message, string location)
		{
			log.Add(new SimpleLogItem(LogLevel.Error, message, location));
		}
	}
}
