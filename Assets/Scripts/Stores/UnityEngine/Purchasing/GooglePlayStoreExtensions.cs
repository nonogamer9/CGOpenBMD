using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class GooglePlayStoreExtensions : AndroidJavaProxy, IGooglePlayStoreExtensions, IStoreExtension, IGooglePlayConfiguration, IStoreConfiguration
	{
		private AndroidJavaObject m_Java;

		public GooglePlayStoreExtensions()
			: base("com.unity.purchasing.googleplay.GooglePlayPurchasing")
		{
		}

		public void SetAndroidJavaObject(AndroidJavaObject java)
		{
			m_Java = java;
		}

		public void SetPublicKey(string key)
		{
		}

		public void UpgradeDowngradeSubscription(string oldSku, string newSku)
		{
			m_Java.Call("UpgradeDowngradeSubscription", oldSku, newSku);
		}

		public Dictionary<string, string> GetProductJSONDictionary()
		{
			string json = m_Java.Get<string>("productJSON");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)MiniJson.JsonDecode(json);
			foreach (KeyValuePair<string, object> item in dictionary2)
			{
				dictionary.Add(item.Key, (string)item.Value);
			}
			return dictionary;
		}
	}
}
