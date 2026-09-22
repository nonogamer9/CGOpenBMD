namespace UnityEngine.Purchasing
{
	public enum LoginResultState
	{
		LoginSucceed = 0,
		UserNotExists = 1,
		PasswordError = 2,
		UserOrPasswordEmpty = 3,
		LoginCallBackIsNull = 4,
		NetworkError = 5,
		NotKnown = 6
	}
}
