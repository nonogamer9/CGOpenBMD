namespace UnityEngine.Purchasing
{
	internal class JavaBridge : AndroidJavaProxy, IUnityCallback
	{
		private IUnityCallback forwardTo;

		public JavaBridge(IUnityCallback forwardTo)
			: base("com.unity.purchasing.common.IUnityCallback")
		{
			this.forwardTo = forwardTo;
		}

		public JavaBridge(IUnityCallback forwardTo, string javaInterface)
			: base(javaInterface)
		{
			this.forwardTo = forwardTo;
		}

		public void OnSetupFailed(string json)
		{
			forwardTo.OnSetupFailed(json);
		}

		public void OnProductsRetrieved(string json)
		{
			forwardTo.OnProductsRetrieved(json);
		}

		public void OnPurchaseSucceeded(string id, string receipt, string transactionID)
		{
			forwardTo.OnPurchaseSucceeded(id, receipt, transactionID);
		}

		public void OnPurchaseFailed(string json)
		{
			forwardTo.OnPurchaseFailed(json);
		}
	}
}
