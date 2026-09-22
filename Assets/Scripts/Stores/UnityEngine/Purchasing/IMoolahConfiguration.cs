using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public interface IMoolahConfiguration : IStoreConfiguration
	{
		string appKey { get; set; }

		string hashKey { get; set; }

		string notificationURL { get; set; }

		void SetMode(CloudMoolahMode mode);
	}
}
