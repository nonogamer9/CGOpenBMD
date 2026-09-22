namespace Ionic.Zip
{
	public enum ZipProgressEventType
	{
		Adding_Started = 0,
		Adding_AfterAddEntry = 1,
		Adding_Completed = 2,
		Reading_Started = 3,
		Reading_BeforeReadEntry = 4,
		Reading_AfterReadEntry = 5,
		Reading_Completed = 6,
		Reading_ArchiveBytesRead = 7,
		Saving_Started = 8,
		Saving_BeforeWriteEntry = 9,
		Saving_AfterWriteEntry = 10,
		Saving_Completed = 11,
		Saving_AfterSaveTempArchive = 12,
		Saving_BeforeRenameTempArchive = 13,
		Saving_AfterRenameTempArchive = 14,
		Saving_AfterCompileSelfExtractor = 15,
		Saving_EntryBytesRead = 16,
		Extracting_BeforeExtractEntry = 17,
		Extracting_AfterExtractEntry = 18,
		Extracting_ExtractEntryWouldOverwrite = 19,
		Extracting_EntryBytesWritten = 20,
		Extracting_BeforeExtractAll = 21,
		Extracting_AfterExtractAll = 22,
		Error_Saving = 23
	}
}
