namespace Ionic.Zip
{
	public enum ExtractExistingFileAction
	{
		Throw = 0,
		OverwriteSilently = 1,
		DoNotOverwrite = 2,
		InvokeExtractProgressEvent = 3
	}
}
