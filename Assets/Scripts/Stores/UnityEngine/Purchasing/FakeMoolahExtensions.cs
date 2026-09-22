using System;

namespace UnityEngine.Purchasing
{
	internal class FakeMoolahExtensions : IMoolahExtension, IStoreExtension
	{
		public void RestoreTransactionID(Action<RestoreTransactionIDState> result)
		{
			result(RestoreTransactionIDState.RestoreSucceed);
		}

		public void ValidateReceipt(string transactionId, string receipt, Action<string, ValidateReceiptState, string> result)
		{
			result(transactionId, ValidateReceiptState.ValidateSucceed, "Fake Validate Receipt Succeed");
		}
	}
}
