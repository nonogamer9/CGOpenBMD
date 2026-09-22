using System;

namespace UnityEngine.Purchasing
{
	public class FakeUnityChannelExtensions : IUnityChannelExtensions, IStoreExtension
	{
		public void ConfirmPurchase(string transactionId, Action<bool, string, string> callback)
		{
			callback(true, "fakeTransactionId", "fakeMessage");
		}

		public void ValidateReceipt(string transactionId, Action<bool, string, string> callback)
		{
			callback(true, "fakeSignData", "fakeSignature");
		}

		public string GetLastPurchaseError()
		{
			return "{ \"error\": \"DuplicateTransaction\" }";
		}
	}
}
