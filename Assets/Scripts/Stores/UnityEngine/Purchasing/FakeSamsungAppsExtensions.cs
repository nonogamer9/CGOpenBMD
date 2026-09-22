using System;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public class FakeSamsungAppsExtensions : ISamsungAppsExtensions, IStoreExtension, ISamsungAppsConfiguration, IStoreConfiguration
	{
		public void SetMode(SamsungAppsMode mode)
		{
		}

		public void RestoreTransactions(Action<bool> callback)
		{
			callback(true);
		}
	}
}
