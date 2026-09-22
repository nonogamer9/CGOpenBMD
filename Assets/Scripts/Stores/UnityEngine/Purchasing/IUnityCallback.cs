namespace UnityEngine.Purchasing
{
	internal interface IUnityCallback
	{
		void OnSetupFailed(string json);

		void OnProductsRetrieved(string json);

		void OnPurchaseSucceeded(string id, string receipt, string transactionID);

		void OnPurchaseFailed(string json);
	}
}
