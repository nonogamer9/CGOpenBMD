using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	public interface IGooglePlayStoreExtensions : IStoreExtension
	{
		void UpgradeDowngradeSubscription(string oldSku, string newSku);

		Dictionary<string, string> GetProductJSONDictionary();
	}
}
