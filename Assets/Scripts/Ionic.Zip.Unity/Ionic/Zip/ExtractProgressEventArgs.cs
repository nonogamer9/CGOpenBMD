namespace Ionic.Zip
{
	public class ExtractProgressEventArgs : ZipProgressEventArgs
	{
		private int _entriesExtracted;

		private string _target;

		public int EntriesExtracted
		{
			get
			{
				return _entriesExtracted;
			}
		}

		public string ExtractLocation
		{
			get
			{
				return _target;
			}
		}

		internal ExtractProgressEventArgs(string archiveName, bool before, int entriesTotal, int entriesExtracted, ZipEntry entry, string extractLocation)
			: base(archiveName, before ? ZipProgressEventType.Extracting_BeforeExtractEntry : ZipProgressEventType.Extracting_AfterExtractEntry)
		{
			base.EntriesTotal = entriesTotal;
			base.CurrentEntry = entry;
			_entriesExtracted = entriesExtracted;
			_target = extractLocation;
		}

		internal ExtractProgressEventArgs(string archiveName, ZipProgressEventType flavor)
			: base(archiveName, flavor)
		{
		}

		internal ExtractProgressEventArgs()
		{
		}

		internal static ExtractProgressEventArgs BeforeExtractEntry(string archiveName, ZipEntry entry, string extractLocation)
		{
			ExtractProgressEventArgs e = new ExtractProgressEventArgs();
			e.ArchiveName = archiveName;
			e.EventType = ZipProgressEventType.Extracting_BeforeExtractEntry;
			e.CurrentEntry = entry;
			e._target = extractLocation;
			return e;
		}

		internal static ExtractProgressEventArgs ExtractExisting(string archiveName, ZipEntry entry, string extractLocation)
		{
			ExtractProgressEventArgs e = new ExtractProgressEventArgs();
			e.ArchiveName = archiveName;
			e.EventType = ZipProgressEventType.Extracting_ExtractEntryWouldOverwrite;
			e.CurrentEntry = entry;
			e._target = extractLocation;
			return e;
		}

		internal static ExtractProgressEventArgs AfterExtractEntry(string archiveName, ZipEntry entry, string extractLocation)
		{
			ExtractProgressEventArgs e = new ExtractProgressEventArgs();
			e.ArchiveName = archiveName;
			e.EventType = ZipProgressEventType.Extracting_AfterExtractEntry;
			e.CurrentEntry = entry;
			e._target = extractLocation;
			return e;
		}

		internal static ExtractProgressEventArgs ExtractAllStarted(string archiveName, string extractLocation)
		{
			ExtractProgressEventArgs e = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_BeforeExtractAll);
			e._target = extractLocation;
			return e;
		}

		internal static ExtractProgressEventArgs ExtractAllCompleted(string archiveName, string extractLocation)
		{
			ExtractProgressEventArgs e = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_AfterExtractAll);
			e._target = extractLocation;
			return e;
		}

		internal static ExtractProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, long bytesWritten, long totalBytes)
		{
			ExtractProgressEventArgs e = new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_EntryBytesWritten);
			e.ArchiveName = archiveName;
			e.CurrentEntry = entry;
			e.BytesTransferred = bytesWritten;
			e.TotalBytesToTransfer = totalBytes;
			return e;
		}
	}
}
