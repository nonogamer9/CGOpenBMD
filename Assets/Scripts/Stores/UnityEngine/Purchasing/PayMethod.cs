namespace UnityEngine.Purchasing
{
	internal class PayMethod
	{
		public static void showPayWebView(string paymentURL, string authGlobal, string transactionId, string hashKey, string customID)
		{
			Debug.Log("CloudMoolah PayWebView is being opened");
			if (Application.platform == RuntimePlatform.Android)
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.cm.androidforunity.PaymentActivity"))
				{
					AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
					androidJavaObject.Call("JavaShowPayWebView", paymentURL, authGlobal, transactionId, hashKey, customID);
				}
			}
		}

		public static void showPaySuccess(string title, string msg)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.cm.androidforunity.PaymentActivity"))
				{
					AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
					androidJavaObject.Call("showPaySuccess", title, msg);
				}
			}
		}

		public static string getDeviceID()
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.cm.androidforunity.PaymentActivity"))
				{
					AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
					return androidJavaObject.Call<string>("getDeviceID", new object[0]);
				}
			}
			return null;
		}
	}
}
