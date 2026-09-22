using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class FakeGooglePlayConfiguration : IGooglePlayConfiguration, IStoreConfiguration
	{
		public void SetPublicKey(string key)
		{
		}
	}
}
