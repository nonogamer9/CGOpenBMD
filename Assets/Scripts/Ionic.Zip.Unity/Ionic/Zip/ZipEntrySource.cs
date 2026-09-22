namespace Ionic.Zip
{
	public enum ZipEntrySource
	{
		None = 0,
		FileSystem = 1,
		Stream = 2,
		ZipFile = 3,
		WriteDelegate = 4,
		JitStream = 5,
		ZipOutputStream = 6
	}
}
