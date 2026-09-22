using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public interface IManagedStoreConfig : IStoreConfiguration
	{
		bool disableStoreCatalog { get; set; }

		bool? trackingOptOut { get; set; }

		bool storeTestEnabled { get; set; }

		string baseIapUrl { get; set; }

		string baseEventUrl { get; set; }
	}
}
