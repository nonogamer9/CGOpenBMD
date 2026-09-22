namespace UnityEngine.Purchasing.Extension
{
	public class PurchaseFailureDescription
	{
		public string productId { get; private set; }

		public PurchaseFailureReason reason { get; private set; }

		public string message { get; private set; }

		public PurchaseFailureDescription(string productId, PurchaseFailureReason reason, string message)
		{
			this.productId = productId;
			this.reason = reason;
			this.message = message;
		}
	}
}
