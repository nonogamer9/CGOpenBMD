using System;

namespace UnityEngine.Purchasing
{
	internal class FakeManagedStoreExtensions : IManagedStoreExtensions, IStoreExtension
	{
		public Product[] storeCatalog
		{
			get
			{
				return new Product[0];
			}
		}

		public void RefreshCatalog(Action a)
		{
			a();
		}
	}
}
