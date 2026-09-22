using System;
using System.Collections.ObjectModel;

namespace UnityEngine.Purchasing
{
	internal interface INativeUnityChannelStore : INativeStore
	{
		void Purchase(string productId, Action<bool, string> callback, string developerPayload = null);

		void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products, Action<bool, string> callback);

		void ConfirmPurchase(string gameOrderId, Action<bool, string, string> callback);

		void ValidateReceipt(string gameOrderId, Action<bool, string, string> callback);
	}
}
