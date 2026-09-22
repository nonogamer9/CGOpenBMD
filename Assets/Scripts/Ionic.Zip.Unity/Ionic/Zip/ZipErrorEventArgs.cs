using System;

namespace Ionic.Zip
{
	public class ZipErrorEventArgs : ZipProgressEventArgs
	{
		private Exception _exc;

		public Exception Exception
		{
			get
			{
				return _exc;
			}
		}

		public string FileName
		{
			get
			{
				return base.CurrentEntry.LocalFileName;
			}
		}

		private ZipErrorEventArgs()
		{
		}

		internal static ZipErrorEventArgs Saving(string archiveName, ZipEntry entry, Exception exception)
		{
			ZipErrorEventArgs e = new ZipErrorEventArgs();
			e.EventType = ZipProgressEventType.Error_Saving;
			e.ArchiveName = archiveName;
			e.CurrentEntry = entry;
			e._exc = exception;
			return e;
		}
	}
}
