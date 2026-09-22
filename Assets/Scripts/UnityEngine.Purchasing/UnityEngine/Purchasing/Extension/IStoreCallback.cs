using System.Collections.Generic;

namespace UnityEngine.Purchasing.Extension
{
	public interface IStoreCallback
	{
		ProductCollection products { get; }

		bool useTransactionLog { get; set; }

		void OnSetupFailed(InitializationFailureReason reason);

		void OnProductsRetrieved(List<ProductDescription> products);

		void OnPurchaseSucceeded(string storeSpecificId, string receipt, string transactionIdentifier);

		void OnPurchaseFailed(PurchaseFailureDescription desc);
	}
}
