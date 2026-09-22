namespace UnityEngine.Store
{
	public class StoreService
	{
		private static AndroidJavaClass serviceClass = new AndroidJavaClass("com.unity.channel.sdk.ChannelService");

		public static void Initialize(AppInfo appInfo, ILoginListener listener)
		{
			if (GameObject.Find(MainThreadDispatcher.OBJECT_NAME) == null)
			{
				GameObject gameObject = new GameObject(MainThreadDispatcher.OBJECT_NAME);
				Object.DontDestroyOnLoad(gameObject);
				gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				gameObject.AddComponent<MainThreadDispatcher>();
			}
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			LoginForwardCallback loginForwardCallback = new LoginForwardCallback(listener);
			AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("com.unity.channel.sdk.AppInfo");
			androidJavaObject2.Set("appId", appInfo.appId);
			androidJavaObject2.Set("appKey", appInfo.appKey);
			androidJavaObject2.Set("clientId", appInfo.clientId);
			androidJavaObject2.Set("clientSecret", appInfo.clientKey);
			androidJavaObject2.Set("debug", appInfo.debug);
			serviceClass.CallStatic("init", androidJavaObject, androidJavaObject2, loginForwardCallback);
		}

		public static void Login(ILoginListener listener)
		{
			LoginForwardCallback loginForwardCallback = new LoginForwardCallback(listener);
			serviceClass.CallStatic("login", loginForwardCallback);
		}
	}
}
