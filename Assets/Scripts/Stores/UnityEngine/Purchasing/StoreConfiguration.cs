using System;
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	internal class StoreConfiguration
	{
		public AppStore androidStore { get; private set; }

		public StoreConfiguration(AppStore store)
		{
			androidStore = store;
		}

		public static string Serialize(StoreConfiguration store)
		{
			Dictionary<string, object> json = new Dictionary<string, object> { 
			{
				"androidStore",
				store.androidStore.ToString()
			} };
			return MiniJson.JsonEncode(json);
		}

		public static StoreConfiguration Deserialize(string json)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(json);
			string value = (string)dictionary["androidStore"];
			AppStore store = ((!Enum.IsDefined(typeof(AppStore), value)) ? AppStore.GooglePlay : ((AppStore)Enum.Parse(typeof(AppStore), (string)dictionary["androidStore"], true)));
			return new StoreConfiguration(store);
		}
	}
}
