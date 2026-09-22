using System;
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	internal class FakeAppleExtensions : IAppleExtensions, IStoreExtension
	{
		private bool m_FailRefresh;

		public bool simulateAskToBuy { get; set; }

		public void RefreshAppReceipt(Action<string> successCallback, Action errorCallback)
		{
			if (m_FailRefresh)
			{
				errorCallback();
			}
			else
			{
				successCallback("A fake refreshed receipt!");
			}
			m_FailRefresh = !m_FailRefresh;
		}

		public void RestoreTransactions(Action<bool> callback)
		{
			callback(true);
		}

		public void RegisterPurchaseDeferredListener(Action<Product> callback)
		{
		}

		public void SetStorePromotionOrder(List<Product> products)
		{
		}

		public void SetStorePromotionVisibility(Product product, AppleStorePromotionVisibility visible)
		{
		}

		public void SetApplicationUsername(string applicationUsername)
		{
		}

		public string GetTransactionReceiptForProduct(Product product)
		{
			return "";
		}

		public void ContinuePromotionalPurchases()
		{
		}

		public Dictionary<string, string> GetIntroductoryPriceDictionary()
		{
			return new Dictionary<string, string>();
		}
	}
}
