namespace UnityEngine.Purchasing
{
	internal class SamsungAppsJavaBridge : AndroidJavaProxy, ISamsungAppsCallback
	{
		private ISamsungAppsCallback forwardTo;

		public SamsungAppsJavaBridge(ISamsungAppsCallback forwardTo)
			: base("com.unity.purchasing.samsung.ISamsungAppsCallback")
		{
			this.forwardTo = forwardTo;
		}

		public void OnTransactionsRestored(bool result)
		{
			forwardTo.OnTransactionsRestored(result);
		}
	}
}
