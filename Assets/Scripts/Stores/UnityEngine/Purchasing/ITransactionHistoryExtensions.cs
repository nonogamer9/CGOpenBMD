using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public interface ITransactionHistoryExtensions : IStoreExtension
	{
		PurchaseFailureDescription GetLastPurchaseFailureDescription();

		StoreSpecificPurchaseErrorCode GetLastStoreSpecificPurchaseErrorCode();
	}
}
