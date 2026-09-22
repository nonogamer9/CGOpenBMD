using System;

namespace UnityEngine.Purchasing
{
	public interface ISamsungAppsExtensions : IStoreExtension
	{
		void RestoreTransactions(Action<bool> callback);
	}
}
