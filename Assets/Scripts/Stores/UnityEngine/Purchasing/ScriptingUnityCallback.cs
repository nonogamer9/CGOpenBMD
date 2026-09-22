using Uniject;

namespace UnityEngine.Purchasing
{
	internal class ScriptingUnityCallback : IUnityCallback
	{
		private IUnityCallback forwardTo;

		private IUtil util;

		public ScriptingUnityCallback(IUnityCallback forwardTo, IUtil util)
		{
			this.forwardTo = forwardTo;
			this.util = util;
		}

		public void OnSetupFailed(string json)
		{
			util.RunOnMainThread(() =>
			{
				forwardTo.OnSetupFailed(json);
			});
		}

		public void OnProductsRetrieved(string json)
		{
			util.RunOnMainThread(() =>
			{
				forwardTo.OnProductsRetrieved(json);
			});
		}

		public void OnPurchaseSucceeded(string id, string receipt, string transactionID)
		{
			util.RunOnMainThread(() =>
			{
				forwardTo.OnPurchaseSucceeded(id, receipt, transactionID);
			});
		}

		public void OnPurchaseFailed(string json)
		{
			util.RunOnMainThread(() =>
			{
				forwardTo.OnPurchaseFailed(json);
			});
		}
	}
}
