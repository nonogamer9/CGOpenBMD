using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public class FakeGooglePlayStoreExtensions : IGooglePlayStoreExtensions, IStoreExtension, IGooglePlayConfiguration, IStoreConfiguration
	{
		public void SetPublicKey(string s)
		{
		}

		public void UpgradeDowngradeSubscription(string oldSku, string newSku)
		{
		}

		public Dictionary<string, string> GetProductJSONDictionary()
		{
			return null;
		}
	}
}
