namespace UnityEngine.Purchasing
{
	public class PurchaseFailedEventArgs
	{
		public Product purchasedProduct { get; private set; }

		public PurchaseFailureReason reason { get; private set; }

		public string message { get; private set; }

		internal PurchaseFailedEventArgs(Product purchasedProduct, PurchaseFailureReason reason, string message)
		{
			this.purchasedProduct = purchasedProduct;
			this.reason = reason;
			this.message = message;
		}
	}
}
