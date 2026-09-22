using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	internal class AnalyticsReporter
	{
		private IUnityAnalytics m_Analytics;

		public AnalyticsReporter(IUnityAnalytics analytics)
		{
			m_Analytics = analytics;
		}

		public void OnPurchaseSucceeded(Product product)
		{
			if (product.metadata.isoCurrencyCode != null)
			{
				m_Analytics.Transaction(product.definition.storeSpecificId, product.metadata.localizedPrice, product.metadata.isoCurrencyCode, product.receipt, null);
			}
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("productID", product.definition.storeSpecificId);
			dictionary.Add("reason", reason);
			dictionary.Add("price", product.metadata.localizedPrice);
			dictionary.Add("currency", product.metadata.isoCurrencyCode);
			Dictionary<string, object> data = dictionary;
			m_Analytics.CustomEvent("unity.PurchaseFailed", data);
		}
	}
}
