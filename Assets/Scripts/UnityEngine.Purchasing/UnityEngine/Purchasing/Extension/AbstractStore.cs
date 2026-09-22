using System.Collections.ObjectModel;

namespace UnityEngine.Purchasing.Extension
{
	public abstract class AbstractStore : IStore
	{
		public abstract void Initialize(IStoreCallback callback);

		public abstract void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products);

		public abstract void Purchase(ProductDefinition product, string developerPayload);

		public abstract void FinishTransaction(ProductDefinition product, string transactionId);
	}
}
