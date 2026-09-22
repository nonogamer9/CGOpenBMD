using System;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public interface IAppleConfiguration : IStoreConfiguration
	{
		string appReceipt { get; }

		bool canMakePayments { get; }

		void SetApplePromotionalPurchaseInterceptorCallback(Action<Product> callback);
	}
}
