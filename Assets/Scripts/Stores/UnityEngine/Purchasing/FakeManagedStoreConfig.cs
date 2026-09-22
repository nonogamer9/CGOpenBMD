using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class FakeManagedStoreConfig : IManagedStoreConfig, IStoreConfiguration
	{
		private bool catalogDisabled = false;

		private bool testStore = false;

		private string iapBaseUrl = null;

		private string eventBaseUrl = null;

		private bool? trackingOptedOut = null;

		public bool disableStoreCatalog
		{
			get
			{
				return catalogDisabled;
			}
			set
			{
				catalogDisabled = value;
			}
		}

		public bool? trackingOptOut
		{
			get
			{
				return trackingOptedOut;
			}
			set
			{
				trackingOptedOut = value;
			}
		}

		public bool storeTestEnabled
		{
			get
			{
				return testStore;
			}
			set
			{
				if (!testStore)
				{
					testStore = value;
				}
			}
		}

		public string baseIapUrl
		{
			get
			{
				return iapBaseUrl;
			}
			set
			{
				if (iapBaseUrl == null && value != null)
				{
					storeTestEnabled = true;
					iapBaseUrl = value;
				}
			}
		}

		public string baseEventUrl
		{
			get
			{
				return eventBaseUrl;
			}
			set
			{
				storeTestEnabled = true;
				eventBaseUrl = value;
			}
		}
	}
}
