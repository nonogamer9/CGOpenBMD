using System;

namespace UnityEngine.Purchasing
{
	public interface IMoolahExtension : IStoreExtension
	{
		void RestoreTransactionID(Action<RestoreTransactionIDState> result);

		void ValidateReceipt(string transactionId, string receipt, Action<string, ValidateReceiptState, string> result);
	}
}
