namespace UnityEngine.Purchasing
{
	internal interface IInternalStoreListener
	{
		void OnInitializeFailed(InitializationFailureReason error);

		PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e);

		void OnPurchaseFailed(Product i, PurchaseFailureReason p);

		void OnInitialized(IStoreController controller);
	}
}
