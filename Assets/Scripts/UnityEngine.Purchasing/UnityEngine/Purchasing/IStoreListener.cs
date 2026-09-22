namespace UnityEngine.Purchasing
{
	public interface IStoreListener
	{
		void OnInitializeFailed(InitializationFailureReason error);

		PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e);

		void OnPurchaseFailed(Product i, PurchaseFailureReason p);

		void OnInitialized(IStoreController controller, IExtensionProvider extensions);
	}
}
