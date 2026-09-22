namespace Ionic.Zip
{
	public class ReadProgressEventArgs : ZipProgressEventArgs
	{
		internal ReadProgressEventArgs()
		{
		}

		private ReadProgressEventArgs(string archiveName, ZipProgressEventType flavor)
			: base(archiveName, flavor)
		{
		}

		internal static ReadProgressEventArgs Before(string archiveName, int entriesTotal)
		{
			ReadProgressEventArgs e = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_BeforeReadEntry);
			e.EntriesTotal = entriesTotal;
			return e;
		}

		internal static ReadProgressEventArgs After(string archiveName, ZipEntry entry, int entriesTotal)
		{
			ReadProgressEventArgs e = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_AfterReadEntry);
			e.EntriesTotal = entriesTotal;
			e.CurrentEntry = entry;
			return e;
		}

		internal static ReadProgressEventArgs Started(string archiveName)
		{
			return new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_Started);
		}

		internal static ReadProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, long bytesXferred, long totalBytes)
		{
			ReadProgressEventArgs e = new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_ArchiveBytesRead);
			e.CurrentEntry = entry;
			e.BytesTransferred = bytesXferred;
			e.TotalBytesToTransfer = totalBytes;
			return e;
		}

		internal static ReadProgressEventArgs Completed(string archiveName)
		{
			return new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_Completed);
		}
	}
}
