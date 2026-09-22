using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class FakeUnityChannelConfiguration : IUnityChannelConfiguration, IStoreConfiguration
	{
		public bool fetchReceiptPayloadOnPurchase { get; set; }
	}
}
