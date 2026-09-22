using System;

namespace UnityEngine.Purchasing
{
	[Serializable]
	public class UnityChannelPurchaseReceipt
	{
		public string storeSpecificId;

		public string transactionId;

		public string orderQueryToken;

		public string json;

		public string signature;

		public string error;
	}
}
