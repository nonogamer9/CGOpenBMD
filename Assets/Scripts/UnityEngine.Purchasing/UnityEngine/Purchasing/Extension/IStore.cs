using System.Collections.ObjectModel;

namespace UnityEngine.Purchasing.Extension
{
	public interface IStore
	{
		void Initialize(IStoreCallback callback);

		void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products);

		void Purchase(ProductDefinition product, string developerPayload);

		void FinishTransaction(ProductDefinition product, string transactionId);
	}
}
