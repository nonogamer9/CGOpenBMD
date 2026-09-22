using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public interface ITizenStoreConfiguration : IStoreConfiguration
	{
		void SetGroupId(string group);
	}
}
