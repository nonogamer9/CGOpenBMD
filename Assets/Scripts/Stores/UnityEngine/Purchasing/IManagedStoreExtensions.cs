using System;

namespace UnityEngine.Purchasing
{
	public interface IManagedStoreExtensions : IStoreExtension
	{
		Product[] storeCatalog { get; }

		void RefreshCatalog(Action callback);
	}
}
