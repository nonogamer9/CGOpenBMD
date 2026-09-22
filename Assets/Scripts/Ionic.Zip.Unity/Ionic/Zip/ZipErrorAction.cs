namespace Ionic.Zip
{
	public enum ZipErrorAction
	{
		Throw = 0,
		Skip = 1,
		Retry = 2,
		InvokeErrorEvent = 3
	}
}
