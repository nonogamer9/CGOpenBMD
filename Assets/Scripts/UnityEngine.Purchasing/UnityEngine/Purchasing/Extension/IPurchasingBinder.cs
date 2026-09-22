using System;
using System.Collections.Generic;

namespace UnityEngine.Purchasing.Extension
{
	public interface IPurchasingBinder
	{
		void RegisterStore(string name, IStore a);

		void RegisterExtension<T>(T instance) where T : IStoreExtension;

		void RegisterConfiguration<T>(T instance) where T : IStoreConfiguration;

		void SetCatalogProvider(ICatalogProvider provider);

		void SetCatalogProviderFunction(Action<Action<HashSet<ProductDefinition>>> func);
	}
}
