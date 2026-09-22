using System;

namespace UnityEngine.Purchasing
{
	public interface IUnityChannelExtensions : IStoreExtension
	{
		void ConfirmPurchase(string transactionId, Action<bool, string, string> callback);

		void ValidateReceipt(string transactionId, Action<bool, string, string> callback);

		string GetLastPurchaseError();
	}
}
