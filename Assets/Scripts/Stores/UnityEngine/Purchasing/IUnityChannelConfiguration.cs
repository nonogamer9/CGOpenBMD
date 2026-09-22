using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	public interface IUnityChannelConfiguration : IStoreConfiguration
	{
		bool fetchReceiptPayloadOnPurchase { get; set; }
	}
}
