using System;
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	public interface IStoreController
	{
		ProductCollection products { get; }

		void InitiatePurchase(Product product, string payload);

		void InitiatePurchase(string productId, string payload);

		void InitiatePurchase(Product product);

		void InitiatePurchase(string productId);

		void FetchAdditionalProducts(HashSet<ProductDefinition> products, Action successCallback, Action<InitializationFailureReason> failCallback);

		void ConfirmPendingPurchase(Product product);
	}
}
