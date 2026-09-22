using System;
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	public interface IAppleExtensions : IStoreExtension
	{
		bool simulateAskToBuy { get; set; }

		void RefreshAppReceipt(Action<string> successCallback, Action errorCallback);

		string GetTransactionReceiptForProduct(Product product);

		void RestoreTransactions(Action<bool> callback);

		void RegisterPurchaseDeferredListener(Action<Product> callback);

		void SetApplicationUsername(string applicationUsername);

		void SetStorePromotionOrder(List<Product> products);

		void SetStorePromotionVisibility(Product product, AppleStorePromotionVisibility visible);

		void ContinuePromotionalPurchases();

		Dictionary<string, string> GetIntroductoryPriceDictionary();
	}
}
