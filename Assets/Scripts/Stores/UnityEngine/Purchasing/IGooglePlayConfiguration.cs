using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public interface IGooglePlayConfiguration : IStoreConfiguration
	{
		void SetPublicKey(string key);
	}
}
